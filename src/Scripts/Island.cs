using Godot;
using System;

public partial class Island : Node3D
{
	[Export] private NodePath playerSpawnPosNodePath;

    public Node3D PlayerSpawnPos { get; private set; }

	public override void _Ready()
	{
		PlayerSpawnPos = GetNode<Node3D>(playerSpawnPosNodePath);
	}
}
