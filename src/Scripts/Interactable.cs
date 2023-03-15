using Godot;
using System;
using System.Collections.Generic;

public partial class Interactable : Node3D
{
    [Export] public bool Enabled 
	{
		get{ return enabled; }
		set
		{
			enabled = value;
            if(allCollisionShapes == null)
            { return; }
			foreach(CollisionShape3D col in allCollisionShapes)
            { col.Disabled = !enabled; }
		}
	}
    [Export] public bool PlayerNeedsToBeGrounded = false;
    [Export] public Node3D GrabPosition;
    [Export] public bool SlowFadeOutSpeedAndSensitivity = false;
    [Export] public string RequiredItem { get; protected set; } = "NONE";

    public bool IsHovering { get; private set; } = false;
    public bool IsInteracting { get; private set; } = false;

    private int framesSinceHover;
    private int framesSinceInteract;
	private bool enabled = true;
	private List<CollisionShape3D> allCollisionShapes;

    public override void _EnterTree()
    {
        base._Ready();
		allCollisionShapes = Utility.GetAllNodesOfType<CollisionShape3D>(Utility.GetAllChildren(this));
    }

    public virtual void Interact(bool once, Player player, Vector2 mouseDistance, Vector3 worldMouseDistance)
    {
        
    }

    public void Hover()
    {
        IsHovering = true;
        framesSinceHover = 0;
    }

    public void Interacting()
    {
        IsInteracting = true;
        framesSinceInteract = 0;
    }

    public override void _Process(double delta)
    {
        if (framesSinceHover > 1)
        { IsHovering = false; }
        else
        { framesSinceHover++; }

        if (framesSinceInteract > 1)
        { IsInteracting = false; }
        else
        { framesSinceInteract++; }


    }
}
