using Godot;
using System;

public partial class LightTunnel : Area3D
{	
	private Player player;
	private Player playerTest;

	public LightTunnel()
	{
		BodyEntered += BodyEnteredCallback;
		BodyExited += BodyExitedCallback;
	}

	public override void _Ready()
	{
		
	}

	public override void _PhysicsProcess(double delta)
	{
		if(player == null)
		{ return; }

		Vector3 dir = GlobalPosition - player.GlobalPosition;
		dir = dir.Normalized();

		player.ApplyForce(dir * 8000f * (float)delta);
	}

	private void BodyEnteredCallback(Node3D body)
	{
		playerTest = body as Player;
		if(playerTest == null)
		{ return; }
		if(!playerTest.NetworkPlayer.IsLocalPlayer)
		{ return; }

		player = playerTest;
		GD.Print("LightTunnel Enter");
	}
	private void BodyExitedCallback(Node3D body)
	{
		playerTest = body as Player;
		if(playerTest == null)
		{ return; }
		if(!playerTest.NetworkPlayer.IsLocalPlayer)
		{ return; }

		player = null;
		GD.Print("LightTunnel Exit");
	}
}
