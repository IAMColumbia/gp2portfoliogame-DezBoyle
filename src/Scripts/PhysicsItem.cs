using Godot;
using System;

public partial class PhysicsItem : Item
{
    // [Export] public NodePath RigidbodyPath { get; private set; }

	// public RigidBody3D Rigidbody { get; private set; }

	// private bool canFreeze = false;

	// private const float speedToFreeze = 0.0001f;
	// private const float sqrSpeedToFreeze = speedToFreeze * speedToFreeze;

    // public override void _EnterTree()
    // {
    //     base._EnterTree();
	// 	if(Rigidbody != null)
	// 	{ return; }
	// 	Rigidbody = GetNode<RigidBody3D>(RigidbodyPath);
	// 	Rigidbody.Freeze = true;
    // }

    // public override void _Ready()
	// {
	// 	base._Ready();
	// }

	// public override void _Process(double delta)
	// {
	// 	base._Process(delta);
	// }

	// public override void _PhysicsProcess(double delta)
	// {
	// 	base._PhysicsProcess(delta);

	// 	if(Rigidbody.Freeze || !canFreeze)
	// 	{ return; }

	// 	if(Rigidbody.LinearVelocity.LengthSquared() < sqrSpeedToFreeze && Rigidbody.AngularVelocity.LengthSquared() < sqrSpeedToFreeze)
	// 	{
	// 		Rigidbody.Freeze = true;
	// 		canFreeze = false;
	// 		GD.Print("Rigidbody Idle.  Freezing");
	// 	}
	// }

	// public override void Drop(Node3D obj, Vector3 velocity, Node newParent)
	// {
	// 	base.Drop(Rigidbody, velocity, newParent);
	// 	SetCollideWithPlayer(true);
	// 	Rigidbody.Freeze = false;
	// 	CanFreezeTimer();
	// }

    // public override Node3D PickUp(Node newParent, Node3D objectToPickup = null)
    // {
	// 	base.PickUp(newParent, Rigidbody);
	// 	Rigidbody.Freeze = true;
	// 	canFreeze = false;
	// 	SetCollideWithPlayer(false);
    //     return Rigidbody;
    // }

	// public override void Reset()
	// {
	// 	Rigidbody.Freeze = true;
	// 	Rigidbody.Position = Vector3.Zero;
	// 	Rigidbody.Rotation = Vector3.Zero;
	// 	Rigidbody.LinearVelocity = Vector3.Zero;
	// 	Rigidbody.AngularVelocity = Vector3.Zero;
	// 	canFreeze = false;
	// 	base.Reset();
	// }

	// private async void CanFreezeTimer()
	// {
	// 	await ToSignal(GetTree().CreateTimer(5f), "timeout");
	// 	if(!Rigidbody.Freeze)
	// 	{ canFreeze = true; }
	// }

    // protected override Node3D GetObject()
    // { return Rigidbody; }

    // public override Node GetDroppedParent()
    // { return this; }
}
