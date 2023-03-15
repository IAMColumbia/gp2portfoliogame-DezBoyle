using Godot;
using System;

public partial class TestItemSpawner : Node3D
{
	[Export] private PackedScene sceneToSpawn;
	[Export] private float delay = 3f;
	private float lastTimeSpawned = -420f;

	public override void _Ready()
	{
		base._Ready();

		//SetProcess(false);
	}

	public override void _Process(double delta)
	{		
		//if(Time.GetTicksMsec() - lastTimeSpawned > delay * 1000f)
		if(Input.IsActionJustPressed("fart"))
		{
			lastTimeSpawned = Time.GetTicksMsec();
			Node3D obj = GameManager.instance.SpawnObject(sceneToSpawn, this);
			if(obj == null)
			{ return; }
			((RigidBody3D)obj).Freeze = false;
			obj.Position = new Vector3(Utility.RandomRange(-2f, 2f), Utility.RandomRange(-2f, 2f), Utility.RandomRange(-2f, 2f));
		}
	}
}
