using Godot;
using System;

public partial class TestInteractable : Interactable
{
	[Export] private Node3D thingy;
	private Vector3 thingyStartPos;

    public override void _Ready()
    {
        base._Ready();
		thingyStartPos = thingy.GlobalPosition;
    }

    public override void Interact(bool once, Player player, Vector2 mouseDistance, Vector3 worldMouseDistance)
    {
        thingy.Translate(worldMouseDistance);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

		if(!IsInteracting)
		{
			thingy.GlobalPosition = thingy.GlobalPosition.Lerp(thingyStartPos, (float)delta * 5f);
		}
    }
}
