using Godot;
using System;

public partial class LevelManager : Node
{
	public Node3D PlayerSpawn { get { return playerSpawn; } }

	[Export] private NodePath playerSpawnPath;

	private Node3D playerSpawn;
	private Player player;

	public override void _EnterTree()
	{
		playerSpawn = GetNode<Node3D>(playerSpawnPath);
		GetNode<Area3D>("FallTrigger").BodyEntered += FallTrigger; //unsure if this needs to be in _Ready to work correctly, moving it to _EnterTree for code clean beans
	}

	public override void _Ready()
	{
		//GetNode<Area3D>("FallTrigger").BodyEntered += FallTrigger;
	}

	public void FallTrigger(Node3D body)
	{
		player = body as Player;

		if(player != null)
		{ player.RespawnToLastIsland(PlayerSpawn.GlobalPosition); }
	}
}
