using Godot;
using System;

public partial class InteractablePickup : Interactable
{
	[Export] public bool Float = false;
	[Export] private float floatHeight = 0.6f;
	[Export] private string itemScenePath = "res://Objects/Items/Item_CrystalPistol.tscn";
	[Export] private float pickupDistance = .25f;

	private float sqrPickupMouseDistance;
	private Vector2 totalMouseDistance;
	private PackedScene packedScene;
	private PhysicsRayQueryParameters3D rayParams;
    private Godot.Collections.Dictionary hit;
	private Vector3 groundPos;
	private float yVelocity;
	private const float terminalVelocity = 4f;
	private NetworkObject networkObject;
	private Vector3? originalPosition = null;

	public override void _Ready()
	{
		base._Ready();
		sqrPickupMouseDistance = pickupDistance * pickupDistance;
		packedScene = GD.Load<PackedScene>(itemScenePath);

		Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid>();
        exclude.Add(new Rid(this as Godot.GodotObject));
        rayParams = PhysicsRayQueryParameters3D.Create(Vector3.Zero, Vector3.Zero, 1, exclude);
        rayParams.CollideWithAreas = false;

		networkObject = GetNode<NetworkObject>("NetworkObject");
	}

	private const float horizontalLimit = 50f;
	private const float verticalLimit = 400f;
	public override void Interact(bool once, Player player, Vector2 mouseDistance, Vector3 worldMouseDistance)
	{
        base.Interact(once, player, mouseDistance, worldMouseDistance);

		if(once && !originalPosition.HasValue)
		{ originalPosition = GlobalPosition; }

		if(once)
        {
			totalMouseDistance = Vector2.Zero;
			GlobalPosition = originalPosition.Value;
		}

		//mouseDistance.Y = Mathf.Abs(mouseDistance.Y);
        totalMouseDistance += mouseDistance;
		totalMouseDistance.Y = Mathf.Clamp(totalMouseDistance.Y, 0f, verticalLimit * player.MouseSensitivity);
		totalMouseDistance.X = Mathf.Clamp(totalMouseDistance.X, -horizontalLimit * player.MouseSensitivity, horizontalLimit * player.MouseSensitivity);
		Vector3 offset = new Vector3();
		offset = player.Cam.GlobalTransform.Basis * new Vector3(totalMouseDistance.X, 0f, totalMouseDistance.Y);
		offset *= .001f * player.MouseSensitivity;
		GlobalPosition = originalPosition.Value + offset;
		
		//if(totalMouseDistance.LengthSquared() > sqrPickupMouseDistance)
		if(totalMouseDistance.Y >= verticalLimit - 5 || GlobalPosition.DistanceSquaredTo(player.Cam.GlobalPosition) < sqrPickupMouseDistance)
		{
			GD.Print(totalMouseDistance);
			base.Enabled = false;
			player.Inventory.Pickup(packedScene);
            GameManager.instance.DeleteObject(networkObject);
		}
	}

    public override void _Process(double delta)
    {
		base._Process(delta);

		if(!base.IsInteracting && originalPosition.HasValue)
		{
			GlobalPosition = GlobalPosition.Lerp(originalPosition.Value, (float)delta);
		}

        if (Float)      //temporary
        {
            RotateY((float)delta);
			return;
		}

		// if(groundPos != null)
		// {
        //     GlobalPosition = GlobalPosition.Lerp(groundPos, (float)delta * yVelocity);
        //     yVelocity = Mathf.Lerp(yVelocity, 0f, (float)delta);
		// 	return;
		// }

        // rayParams.From = GlobalPosition;
        // rayParams.To = GlobalPosition + Vector3.Down * (1f + floatHeight);
        // hit = GetWorld3D().DirectSpaceState.IntersectRay(rayParams);

		// if(hit != null && hit.Count > 0)
		// {
		// 	groundPos = (Vector3)hit["position"];	
		// }

		// yVelocity += (float)delta * 9.8f;
		// yVelocity = Mathf.Clamp(yVelocity, -terminalVelocity, terminalVelocity);

		// Translate(Vector3.Down * yVelocity);
    }
}
