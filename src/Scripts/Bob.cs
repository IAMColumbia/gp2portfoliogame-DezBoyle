using Godot;
using System;

public partial class Bob : Node3D
{
	[Export] private float speed = 0.35f;
	[Export] private float amplitude = 0.2f;

	private float startY;
	private Vector3 pos;
	private float randomPhaseOffset;

	public override void _Ready()
	{
		base._Ready();
		startY = GlobalPosition.Y;
		pos = GlobalPosition;
		randomPhaseOffset = Utility.RandomRange(-Mathf.Pi, Mathf.Pi);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		pos.Y = startY + Mathf.Sin(randomPhaseOffset + (Time.GetTicksMsec() / 1000f) * speed) * amplitude;
        GlobalPosition = pos;
	}
}
