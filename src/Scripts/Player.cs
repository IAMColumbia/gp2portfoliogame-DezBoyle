using Godot;
using System;
using System.Collections.Generic;

public partial class Player : RigidBody3D, IDamagable
{
	[Export] private int maxHp = 100;
	[Export] private float regenSpeed = 2.5f;
	[Export] private float speedNormal = 6f;
	[Export] private float speedDecay = 8f;
	[Export] private float antiClimbValue = -1f;
	[Export] private float speedIncreaseAmountWhenRunning = .35f;
	[Export] private float jumpForwardSpeedIncrease = 0.75f;
	[Export] private float maxSpeed = 12f;
	[Export] private float sensitivity = 2.5f;
	[Export] private float acceleration = 75f;
	[Export] private float floatHeight = 1f;
	[Export] private float groundRaycastLength = 1.5f;
	[Export] private float floatSpringStrength = 200f;
	[Export] private float floatSpringDamper = 20f;
	[Export] private float jumpCoyoteTime = 0.25f;
	[Export] private float jumpForce = 8f;
	[Export] private float jumpCooldown = .5f;
	[Export] private float gravity = 19.6f;

	public bool Enabled
	{
		get
		{ return enabled; }
		set
		{
			if(NetworkPlayer == null || !NetworkPlayer.IsLocalPlayer)
			{ return; }
			enabled = value;
			Input.MouseMode = enabled ? Input.MouseModeEnum.Captured : Input.MouseModeEnum.Visible;
		}
	}
	public bool IsGrounded
	{
		get { return (ground != null); }
	}
	public bool EnableLook { get; private set; } = true;
	public bool EnableMove { get; private set; } = true;
	public float Speed { get { return speed; } }
	public float SpeedLimit { get { return speedLimit; } }
    public float MouseSensitivity { get { return sensitivity; } }

	public Camera3D Cam { get; private set; }
	public Node3D PlayerBody { get; private set; }
	public PlayerInteraction Interaction { get; private set; }
	public NetworkPlayer NetworkPlayer { get; private set; }
	public PlayerInventory Inventory { get; private set; }
	public Node3D DroppedItemPosition { get; private set; }
    public Health Health { get; private set; }
	
	public float SensitivityMultiplier { get; set; } = 1f;
	public float SpeedMultiplier { get; set; } = 1f;

	private PlayerDebug playerDebug;
	private bool enabled;
	private Vector3 moveInput;
	private Vector2 mouseInput;
	private bool jumpInput;
	private bool jumpinputDown;
	private Vector3 wishRotation;
	private bool wishJump;
	private bool canJumpFloat = true;
	private bool wantsToMove;
	private Vector3 wishVelocity;
	private Vector3 velocity;
	private float inputVelDot;
	private PhysicsRayQueryParameters3D rayParams;
	private Godot.Collections.Dictionary hit;
	private float speedLimit;
	private float lastTimeGrounded = -420f;
	private Node ground;
	private Island island;
	private Island lastIsland;
	private bool colliding;
	private bool lastGrounded;
	private const float HalfPI = Mathf.Pi / 2.1f;
	private float speed;
	private float lastTimeJumped = -420f;
	private bool scoreForRunning;
	private bool noclip = false;
	private float yVelocityLimit;

	public override void _EnterTree()
	{
		PlayerBody = GetNode<Node3D>("PlayerBody");
		Cam = PlayerBody.GetNode<Camera3D>("Camera");
        DroppedItemPosition = PlayerBody.GetNode<Node3D>("DroppedItemPosition");
	}

	public override void _Ready()
	{
		speedLimit = speedNormal;
		Health = new Health(maxHp);
		Health.OnDeath += OnDeathCallback;

		NetworkPlayer = GetNode<NetworkPlayer>("NetworkPlayer");
		NetworkPlayer.Init(this);
		playerDebug = GetNode<PlayerDebug>("DebugText");
		playerDebug.Init(this);
		Interaction = GetNode<PlayerInteraction>("PlayerInteraction");
		Interaction.Init(this);
		Inventory = GetNode<PlayerInventory>("PlayerInventory");
		Inventory.Init(this);
		UI.instance.Init(this);

		if(!NetworkPlayer.IsLocalPlayer)
		{
			SetPhysicsProcess(false);
			SetProcess(false);
			SetProcessInput(false);
			return;
		}

		// GetNode<Node3D>("PlayerBody/MONKEYTESTMODEL").Visible = false;
		var meshes = Utility.GetAllNodesOfType<MeshInstance3D>(Utility.GetAllChildren(GetNode<Node3D>("PlayerBody/Player_Wizard_01")));
		foreach(var m in meshes)
		{
			m.SetLayerMaskValue(1, false);
			m.SetLayerMaskValue(5, true);
		}

		NetworkPlayer.SetNameLabelVisible(false);

		Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid>();
		exclude.Add(new Rid(this as Godot.GodotObject));
		rayParams = PhysicsRayQueryParameters3D.Create(PlayerBody.GlobalPosition, PlayerBody.GlobalPosition + Vector3.Down * groundRaycastLength, 1, exclude);
		rayParams.CollideWithAreas = false;

		ContactMonitor = true;
		MaxContactsReported = 1;
		
		Enabled = true;
	}

	public override void _Process(double delta)
	{
		if (!enabled)
		{ return; }

		UpdateKeys();
		MouseLook();

		Health.Heal((float)delta * regenSpeed);
	}

	public override void _PhysicsProcess(double delta)
	{        
		if(!enabled)
		{ StopVelocity(); return; }

		SetMaxSpeed(delta);

		if (noclip)
		{ MoveNoclip(delta); }
		else
		{ Move(delta); }
	}

	private void SetMaxSpeed(double delta)
	{
		if (IsGrounded && speed < speedNormal / 2f)
		{ speedLimit = Mathf.Lerp(speedLimit, speedNormal, (float)delta * speedDecay); }

		if(speedLimit > maxSpeed)
		{ speedLimit = maxSpeed; }

		if (moveInput == Vector3.Zero)
		{ speedLimit = speedNormal; }
		return;
	}

	private void MoveNoclip(double delta)
	{
		moveInput.Y += jumpInput || Input.IsKeyPressed(Key.Q) ? 1f : 0f;
		moveInput.Y += Input.IsKeyPressed(Key.E) ? -1f : 0f;
		Vector3 targetVelocity = moveInput * speedLimit * (float)delta * (Input.IsKeyPressed(Key.Shift) ? 3f : 1f);
		Translate(PlayerBody.Transform.Basis * targetVelocity);
	}

	private void Move(double delta)
	{
		//Ground
		rayParams.From = PlayerBody.GlobalPosition;
		rayParams.To = PlayerBody.GlobalPosition + Vector3.Down * groundRaycastLength;
		hit = GetWorld3D().DirectSpaceState.IntersectRay(rayParams);

		ground = null;
		RigidBody3D hitRigidbody = null;
		Vector3 hitRigidbodyVelocity = Vector3.Zero;
		if (hit != null && hit.Count > 0)
		{
			ground = (Node)hit["collider"];
			island = Utility.GetNodeInParent<Island>(ground);
			if(island != null)
			{ lastIsland = island; }
			hitRigidbody = ground as RigidBody3D;
			lastTimeGrounded = Time.GetTicksMsec();
			if (hitRigidbody != null)
			{ hitRigidbodyVelocity = hitRigidbody.LinearVelocity; }
		}

		bool jumpCooldownDone = ((Time.GetTicksMsec() - lastTimeJumped) > (jumpCooldown * 1000f));
		bool coyoteTimeGrounded = (Time.GetTicksMsec() - lastTimeGrounded < jumpCoyoteTime * 1000f);

		//Spring force
		if (IsGrounded && jumpCooldownDone)
		{
			float velDot = Vector3.Down.Dot(LinearVelocity);
			float hitRigidbodyVelDot = Vector3.Down.Dot(hitRigidbodyVelocity);

			float relDownwardsVel = velDot - hitRigidbodyVelDot;

			float offsetFromFloatHeight = ((Vector3)hit["position"]).DistanceTo(rayParams.From) - floatHeight;
			float springForce = (offsetFromFloatHeight * floatSpringStrength) - (relDownwardsVel * floatSpringDamper);

			ApplyCentralForce(Vector3.Down * springForce);
		}
		else
		{
			float gravity;
			if (canJumpFloat && jumpInput && LinearVelocity.Y > 0f)
			{ gravity = this.gravity / 1.5f; }
			else
			{
				gravity = this.gravity;
				//canJumpFloat = false;
			}

			ApplyCentralForce(Vector3.Down * gravity);
		}

		//Horizontal Movement
		Vector3 targetVelocity = moveInput * speedLimit * SpeedMultiplier;
		Vector3 horizontalVel = new Vector3(LinearVelocity.X, 0f, LinearVelocity.Z);
		horizontalVel *= PlayerBody.Transform.Basis;
		speed = horizontalVel.Length();

		inputVelDot = horizontalVel.Normalized().Dot(moveInput);
		float acceleration = this.acceleration;
		if (inputVelDot < 0f && IsGrounded) //moving against velocity, slowing down
		{
			inputVelDot = Mathf.Abs(inputVelDot);
			acceleration = this.acceleration + inputVelDot; //turn around boost 
		}

		if (!IsGrounded)
		{ acceleration /= 2.5f; }

		wishVelocity = wishVelocity.MoveToward(targetVelocity, acceleration * (float)delta);

		Vector3 neededVelocity = (wishVelocity - horizontalVel) / (float)delta;
		neededVelocity.Y = 0f;

		if(speed > speedNormal / 2f)
		{ speedLimit += ((float)delta * speedIncreaseAmountWhenRunning); } //increase speedlimit for running

		ApplyCentralForce(PlayerBody.Transform.Basis * neededVelocity);

		//Jumping
		if (jumpInput && coyoteTimeGrounded && jumpCooldownDone && SpeedMultiplier > 0.3f)
		{
			lastTimeJumped = Time.GetTicksMsec();
			float downwardsVelocity = Mathf.Min(LinearVelocity.Y, 0f); //only get rid of negative Y vel.  Add on top Y if already going up
			ApplyCentralImpulse(Vector3.Up * (jumpForce - downwardsVelocity));

			if (jumpinputDown)
			{ canJumpFloat = true; }

			if(speed > speedNormal / 2f)
			{ speedLimit += jumpForwardSpeedIncrease; }
		}

		//yVelocityLimit
		//to prevent us from zooming up anything that's sloped 
		if(IsGrounded)
		{ yVelocityLimit = jumpForce; }
		else
		{
			yVelocityLimit -= (float)delta * jumpForce;
			yVelocityLimit = Mathf.Clamp(yVelocityLimit, antiClimbValue, jumpForce);
		}
	}


	private void MouseLook()
	{
		wishRotation = Cam.Rotation;
		if (EnableLook)
		{
			PlayerBody.RotateY(mouseInput.X);
			wishRotation.X = Mathf.Clamp((mouseInput.Y + wishRotation.X), -HalfPI, HalfPI);
		}
		Cam.Rotation = wishRotation;
		mouseInput = Vector2.Zero;
	}

	private void UpdateKeys()
	{
		moveInput = Vector3.Zero;
		jumpInput = false;

		if (!EnableMove)
			return;
		moveInput.Z += Input.IsActionPressed("forward") == true ? 1f : 0f;
		moveInput.Z += Input.IsActionPressed("backward") == true ? -1f : 0f;
		moveInput.X += Input.IsActionPressed("left") == true ? 1f : 0f;
		moveInput.X += Input.IsActionPressed("right") == true ? -1f : 0f;
		moveInput = moveInput.Normalized();

		jumpInput = Input.IsActionPressed("jump");
		jumpinputDown = Input.IsActionJustPressed("jump");

		//cheats
		if(Input.IsActionJustPressed("cheats") && GameManager.instance.AllowCheats)
		{
			noclip = !noclip;
			Freeze = noclip;
			Interaction.Enabled = !noclip;
			Interaction.HideCrosshair();
			playerDebug.Enabled = noclip;
		}
	}

	public override void _Input(InputEvent @input)
	{
		if (@input is InputEventMouseMotion)
		{
			mouseInput += ((InputEventMouseMotion)@input).Relative * -Vector2.One;
			mouseInput *= (sensitivity / 2048) * SensitivityMultiplier;
		}
	}

	public void RespawnToLastIsland(Vector3 defaultSpawn)
	{
		if(!GameManager.instance.Singleplayer && !NetworkPlayer.IsLocalPlayer)
		{ return; }
		//go to spawn point of the last island we were on OR default
		if((lastIsland != null && !IsInstanceValid(lastIsland)) || lastIsland == null)
		{ GlobalPosition = defaultSpawn; }
		else
		{ GlobalPosition = lastIsland.PlayerSpawnPos.GlobalPosition; }
		StopVelocity();
	}

	public void StopVelocity()
	{
		if(!NetworkPlayer.IsLocalPlayer)
		{ return; }
		LinearVelocity = Vector3.Zero;
		wishVelocity = Vector3.Zero;
	}

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
	{
		base._IntegrateForces(state);
		colliding = false;
		if(state.GetContactCount() == 0)
		{ return; }
		colliding = true;
		state.LinearVelocity = new Vector3(state.LinearVelocity.X, Mathf.Min(state.LinearVelocity.Y, yVelocityLimit), state.LinearVelocity.Z); 
	}

    public void Damage(int amount)
    {
        Health.Damage(amount);
    }

	private void OnDeathCallback()
	{
		GameManager.instance.ResetGame();
	}
}
