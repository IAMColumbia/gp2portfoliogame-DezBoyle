using Godot;
using System;

public partial class Windchimes : Interactable
{
	[Export] private NodePath lookAtDoohicky;
	[Export] private NodePath pivotPath;
	[Export] private float speed = 0.1f;
	[Export] private float maxDistance = 0.3f;
	[Export] private Godot.Collections.Array<AudioStream> chimeSounds;

	private AudioPlayer audio;
	private AudioStreamPlayer3D ambientAudio;
	private Node3D pivot;
	private Node3D pos;
	private Vector3 startPos;
	private Vector3 posSine;
	private Vector3 lastPos;
	private Vector3 totalMouseDistance;
	private float lastTimeInteracted = -420f;
	private float lastTimeStartedInteract = -420f;
	private float sqrMaxDistance;
	int lastChime;
	float angle;
	float angleangle; // goofy ahh
	float highFreq = .4f;
    float lowFreq = 1f;
    float resetAngle;
	float angleangletarget;
	float angleanglecurrent; //this is so shit i know

    public override void _Ready()
    {
        base._Ready();

		pos = GetNode<Node3D>(lookAtDoohicky);
		pivot = GetNode<Node3D>(pivotPath);
		startPos = pos.Position;
		sqrMaxDistance = maxDistance * maxDistance;
		audio = GetNode<AudioPlayer>("AudioPlayer");
		ambientAudio = GetNode<AudioStreamPlayer3D>("AmbientAudio");
		resetAngle = ((Mathf.Pi * 2f) * highFreq * 4f) * ((Mathf.Pi * 2f) * highFreq * 4f);
    }

    public override void Interact(bool once, Player player, Vector2 mouseDistance, Vector3 worldMouseDistance)
    {
        base.Interact(once, player, mouseDistance, worldMouseDistance);

		if(once)
		{
			totalMouseDistance = pos.Position - startPos;
			lastTimeStartedInteract = Time.GetTicksMsec();
		}

		totalMouseDistance += worldMouseDistance * speed;
		pos.Position = startPos + totalMouseDistance;
		
		if(totalMouseDistance.LengthSquared() > sqrMaxDistance)
		{
			totalMouseDistance = totalMouseDistance.Normalized();
			// float sinAngle = Mathf.Asin(totalMouseDistance.x);
			// float cosAngle = Mathf.Asin(totalMouseDistance.z);
			float cosAngle = totalMouseDistance.X;
			float sinAngle = totalMouseDistance.Z;
			int chime = -1;
			if(cosAngle >= -0.5f && cosAngle < 0.5f && sinAngle > 0.866f)
			{
				// GD.Print("Top");
				chime = 0;
			}
			else if(cosAngle >= -0.5f && cosAngle < 0.5f && sinAngle < -0.866f)
			{
				// GD.Print("Bottom");
				chime = 1;
			}
			else if(sinAngle >= -0.866f && sinAngle < 0f && cosAngle >= 0.5f && cosAngle < 1f)
			{
				// GD.Print("Bottom Right");
				chime = 2;
			}
			else if(sinAngle >= -0.866f && sinAngle < -1f && cosAngle <= -0.5f && cosAngle < 0f)
			{
				// GD.Print("Bottom Left");
				chime = 3;
			}
			else if(cosAngle >= -1f && cosAngle < 0.5f && sinAngle <= 0.866f && sinAngle > 0f)
			{
				// GD.Print("Top Left");
				chime = 4;
			}
			else if(cosAngle >= 0.5f && cosAngle < 1f && sinAngle <= 0.866f && sinAngle > 0f)
			{
				// GD.Print("Top Right");
				chime = 5;
			}

			if(chime == -1) //so fucking done with this code lmaooo dont care mode guy
			{  chime = 3; }

			if(lastChime != chime && chime != -1)
			{
				audio.PlayOneShot(chimeSounds[chime], 0f);
				lastChime = chime;
			}

			
			totalMouseDistance = totalMouseDistance * maxDistance;
		}

		lastTimeInteracted = Time.GetTicksMsec();
		lastPos = pos.Position;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

		ambientAudio.VolumeDb = Mathf.Lerp(ambientAudio.VolumeDb, IsInteracting ? -80f : 0f, (float)delta);

		if(!IsInteracting)
		{
			float t = Time.GetTicksMsec() - lastTimeInteracted;
			t /= 1000f;
			t = Mathf.Clamp(t, 0f, 1f);
			posSine = startPos;
			angle += (float)delta * Mathf.Abs(Mathf.Sin(Time.GetTicksMsec() / 1000f));
			if(angle > resetAngle)
			{ angle = 0f; }
			posSine.X += Mathf.Sin(angle / highFreq) * 0.06f;
			posSine.Z += Mathf.Cos(angle / lowFreq) * 0.06f;
			pos.Position = pos.Position.Lerp(posSine, (float)delta * 4f);
			angleangle += (float)delta / 2f;
		}
		else
		{
			
		}

		angleangletarget = IsInteracting ? 0f : angleangle;
		pivot.LookAt(pos.GlobalPosition, -Vector3.Forward);
		angleanglecurrent = Mathf.Lerp(angleanglecurrent, Mathf.Sin(angleangletarget), (float)delta * 6f);
		pivot.RotateY(angleanglecurrent);
    }
}
