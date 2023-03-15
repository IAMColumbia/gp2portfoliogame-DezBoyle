using Godot;
using System;

public partial class PlayerInteraction : Node3D
{
    public enum CrosshairTypeEnum { Normal, Interact, Hidden }

    public bool Enabled { get; set; } = true;
    public CrosshairTypeEnum CrosshairType { get; private set; }
    public bool Grabbing { get { return grabbedInteractable != null && IsInstanceValid(grabbedInteractable); } }
    public Node3D GrabPosition { get { return Grabbing ? grabbedInteractable.GrabPosition : null; } }

    [Export] private float interactRaycastDistance = 8f;
    [Export] private float sensitivityAndSpeedLerpSpeed = 13f;
    
    private float sqrInteractRaycastDistance;
    private Player player;
    private PhysicsRayQueryParameters3D rayParams;
    private Godot.Collections.Dictionary hit;
    private Interactable interactable;
    private Interactable lastInteractable;
    private InteractableRelayCollider interactableRelay;
    private Node node;
    private Interactable grabbedInteractable;
    private bool justGrabbed;
    private Vector2 mouseDistance;
    private float sqrHandTooFarDistance;

    void CheckMultiplayer()
    {
        if (!GameManager.instance.Singleplayer && !IsMultiplayerAuthority())
        {
            SetProcess(false);
            SetPhysicsProcess(false);
            return;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        CheckMultiplayer();
    }

    public void Init(Player p)
    {
        CheckMultiplayer();

        player = p;
        if(!player.NetworkPlayer.IsLocalPlayer)
        {
            SetPhysicsProcess(false);
            SetProcess(false);
            SetProcessInput(false);
            return;
        }

        Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid>();
        exclude.Add(new Rid(player.PlayerBody as Godot.GodotObject));
        rayParams = PhysicsRayQueryParameters3D.Create(player.Cam.GlobalPosition, player.Cam.GlobalPosition + player.Cam.GlobalTransform.Basis.Z * interactRaycastDistance, 1, exclude);
        rayParams.CollideWithAreas = true;
        sqrHandTooFarDistance = Mathf.Pow(interactRaycastDistance * 2, 2);
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!Enabled)
        { return; }
        
        CheckInteraction(delta);
        
        if(grabbedInteractable == null)
        { CrosshairType = interactable == null ? CrosshairTypeEnum.Normal : CrosshairTypeEnum.Interact; }
		else
        { CrosshairType = CrosshairTypeEnum.Interact; }

        lastInteractable = interactable;
    }

    public void HideCrosshair()
    { CrosshairType = CrosshairTypeEnum.Hidden; }

    public override void _Process(double delta)
    {
        base._Process(delta);

        GrabInteraction(delta);
        SetSpeedAndSensitivityMultipliers(delta);
    }

    bool slowFade = false;
    private void SetSpeedAndSensitivityMultipliers(double delta)
    {
        float sensitivityMultiplier = 1f;
        float speedMultiplier = 1f;

        if(grabbedInteractable != null)
        {
            slowFade = grabbedInteractable.SlowFadeOutSpeedAndSensitivity;
            sensitivityMultiplier = 0.01f;
            speedMultiplier = 0.03f;
        }

        float mouseLerpSpeed = sensitivityAndSpeedLerpSpeed;
        if(player.SensitivityMultiplier < sensitivityMultiplier)
        {
            if(slowFade) //slower fade out
            { mouseLerpSpeed = mouseLerpSpeed / 5f; }
            if(player.SensitivityMultiplier > .99f) //snap (for sanity's sake)
            { player.SensitivityMultiplier = 1f; } 
        } 
        player.SensitivityMultiplier = Mathf.Lerp(player.SensitivityMultiplier, sensitivityMultiplier, (float)delta * mouseLerpSpeed);
        player.SpeedMultiplier = Mathf.Lerp(player.SpeedMultiplier, speedMultiplier, (float)delta * sensitivityAndSpeedLerpSpeed);
    }

    private void CheckInteraction(double delta)
    {
        if(grabbedInteractable != null)
        { return; }

        interactable = null;
        rayParams.From = player.Cam.GlobalPosition;
        rayParams.To = player.Cam.GlobalPosition - player.Cam.GlobalTransform.Basis.Z * interactRaycastDistance;
        hit = GetWorld3D().DirectSpaceState.IntersectRay(rayParams);

        if (hit == null || hit.Count == 0) //no hit
        { return; }

        node = (Node)hit["collider"];
        while (interactable == null) //search for interactables.
        {
            interactable = node as Interactable;
            node = node.GetParentOrNull<Node>();
            if (node == null) //Went up entire tree, no interactable
            { break; }
        }

        //see if its a relay collider
        // interactableRelay = interactable as InteractableRelayCollider;
        // if(interactableRelay != null)
        // { interactable = interactableRelay.Interactable; }


        bool groundedCheck = interactable != null && (!interactable.PlayerNeedsToBeGrounded || interactable.PlayerNeedsToBeGrounded == player.IsGrounded); //grounded check
        if(interactable == null || !groundedCheck || (interactable != null && !interactable.Enabled))
        { interactable = null; return; }

        //item limits
        if(interactable.RequiredItem != "NONE" && (player.Inventory.HeldItem == null || interactable.RequiredItem != player.Inventory.HeldItem.ItemName)) //dont match
        { interactable = null; return; }
        if(interactable.RequiredItem == "NONE" && player.Inventory.HeldItem != null) //cant interact while holding item
        { interactable = null; return; }

        interactable.Hover();
        
        if (Input.IsActionJustPressed("interact"))
        {
            grabbedInteractable = interactable;
            justGrabbed = true;
        }
    }

    private void GrabInteraction(double delta)
    {
        if(grabbedInteractable == null || !IsInstanceValid(grabbedInteractable))
        { DoneGrabbing(); return; }

        //required item (Copy pasted bad mode)
        if(interactable.RequiredItem != "NONE" && (player.Inventory.HeldItem == null || interactable.RequiredItem != player.Inventory.HeldItem.ItemName))
        { DoneGrabbing(); return; }

        if(grabbedInteractable.GlobalPosition.DistanceSquaredTo(player.Cam.GlobalPosition) > sqrHandTooFarDistance)
        { DoneGrabbing(); return; }

        if (Input.IsActionPressed("interact") && grabbedInteractable.Enabled)
        {
            interactable.Interacting();

            Vector3 worldMouseDistance = new Vector3(-mouseDistance.X * .01f, 0f, -mouseDistance.Y * .01f);
            worldMouseDistance = player.PlayerBody.Transform.Basis * worldMouseDistance;
            interactable.Interact(justGrabbed, player, mouseDistance, worldMouseDistance);
            justGrabbed = false;
            mouseDistance = Vector2.Zero;
        }
        else
        { DoneGrabbing(); }
    }

    private void DoneGrabbing()
    {
        justGrabbed = false;
        grabbedInteractable = null;
    }

    public override void _Input(InputEvent @input)
    {
        mouseDistance = Vector2.Zero;
        if (@input is InputEventMouseMotion)
        {
            mouseDistance += ((InputEventMouseMotion)@input).Relative;
        }
    }
}
