using Godot;
using System;

public partial class InteractableSnapOff : Interactable
{
	// [Export] private float distance = 1f;
	// [Export] private float progressToSnap = 0.18f;
	// [Export] private float speed = 0.025f;
	// [Export] private Node3D pivot;
	// [Export] private Node3D snag;
	// [Export] private Godot.Collections.Array<AudioStream> snapSounds;
	// [Export] private PhysicsItem physicsItem;

	// private Vector3 startLookAtPosition;
	// private Vector3 totalMouseDistance = Vector3.Zero;
	// private Vector3 lookAtPosition;  //SYNCHORNIZEER this
	// private float progress; //THIS TOO
	// private AudioPlayer audio;
	// private Vector3 originalScale;
	// private bool reEnabling;
	// private float reEnableProgress;

    // public override void _Ready()
    // {
    //     base._Ready();

	// 	startLookAtPosition = pivot.GlobalPosition + Vector3.Up * 1f;
	// 	lookAtPosition = startLookAtPosition;
	// 	originalScale = pivot.Scale;

	// 	if(physicsItem.Pickup != null)
	// 	{
	// 		physicsItem.Pickup.Enabled = false;
	// 		physicsItem.OnFellOffMap += ReEnable;
	// 	}

	// 	physicsItem.Rigidbody.Freeze = true;
	// 	physicsItem.Visible = false;

	// 	audio = GetNode<AudioPlayer>("AudioPlayer");
    // }

    // public override void Interact(bool once, Player player, Vector2 mouseDistance, Vector3 worldMouseDistance)
    // {
    //     base.Interact(once, player, mouseDistance, worldMouseDistance);

	// 	if(once)
	// 	{ totalMouseDistance = Vector3.Zero; }

	// 	float distance = totalMouseDistance.Length();
    //     progress = (distance / this.distance);
	// 	float resistance = Utility.EvaulateCurve(Utility.Easing.Slowest, 1f - progress);
	// 	totalMouseDistance += worldMouseDistance * resistance * speed;

	// 	//GD.Print($"Resistance: {resistance}, Progress: {progress}");

    //     lookAtPosition.x = totalMouseDistance.x + startLookAtPosition.x;
    //     lookAtPosition.z = totalMouseDistance.z + startLookAtPosition.z;
    // }

    // public override void _Process(double delta)
    // {
    //     base._Process(delta);

	// 	if(!Enabled)
	// 	{
	// 		if(!reEnabling)
	// 		{ return; }

    //         physicsItem.GlobalPosition = snag.GlobalPosition;
    //         physicsItem.GlobalRotation = snag.GlobalRotation;

    //         reEnableProgress += (float)delta;
    //         float curved = Utility.EvaulateCurve(Utility.Easing.Smooth, reEnableProgress);
	// 		curved = Mathf.Clamp(curved, 0f, 1f);
    //         pivot.Scale = Vector3.Zero.Lerp(originalScale, curved);

	// 		if(reEnableProgress < .99)
	// 		{ return; }

    //         pivot.Scale = originalScale;
    //         Enabled = true;
	// 		reEnabling = false;
	// 		return;
	// 	}

	// 	if(progress >= progressToSnap)
    //     {
    //         audio.PlayRandom(snapSounds);
    //         progress = 0f;
    //         Disable();
	// 		return;
    //     }

	// 	if(!IsInteracting || !Enabled)
	// 	{
	// 		lookAtPosition = lookAtPosition.Lerp(startLookAtPosition, (float)delta * 8f); //go back
	// 	}

	// 	pivot.LookAt(lookAtPosition, -Vector3.Forward);
    // }

	// //Move this code to a seperate class

	// public void Disable()
	// {
	// 	physicsItem.Reset();
    //     physicsItem.Rigidbody.Freeze = false;
    //     physicsItem.Visible = true;
    //     physicsItem.GlobalPosition = snag.GlobalPosition;
    //     physicsItem.GlobalRotation = snag.GlobalRotation;		

	// 	if(physicsItem.Pickup != null)
	// 	{ physicsItem.Pickup.Enabled = true; }

    //     Enabled = false;
    //     pivot.Visible = false;
	// 	pivot.ProcessMode = ProcessModeEnum.Disabled;
	// }

	// public void ReEnable()
	// {
	// 	if(physicsItem.Pickup != null)
	// 	{ physicsItem.Pickup.Enabled = false; }
	// 	physicsItem.Rigidbody.Freeze = true;
	// 	physicsItem.Visible = false;
    //     pivot.ProcessMode = ProcessModeEnum.Inherit;
    //     pivot.Visible = true;
	// 	reEnabling = true;
	// 	reEnableProgress = 0f;
	// }
}
