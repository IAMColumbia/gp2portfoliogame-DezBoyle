using System;
using Godot;

public partial class EnemyMissle : Enemy
{
	[Export] private int damage = 20;
	[Export] private float speed = 2f;
	[Export] private float turnSpeed = 2f;
	
    private Node3D target;
    private DamageFlash damageFlash;

	public override void _Ready()
	{
		base._Ready();

        damageFlash = GetNode<DamageFlash>("DamageFlash");
		GetNode<Area3D>("Area3D").BodyEntered += OnBodyEnteredCallback;
        base.health.OnHealthChanged += OnHealthChangedCallback;

		GetTarget();
	}

	private void OnBodyEnteredCallback(Node3D body)
	{
        QueueFree();

		Player player = body as Player;
		if(player == null)
		{ return; }
		
		player.Damage(damage);
	}

    private void OnHealthChangedCallback()
    {
        damageFlash.Flash();
    }
	
	private void GetTarget()
	{
        if(GameManager.Players == null || GameManager.Players.Count == 0)
        { return; }
		target = GameManager.Players[Utility.RandomRange(0, GameManager.Players.Count)];
		target = ((Player)target).Cam;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if(target == null || target.NativeInstance == IntPtr.Zero)
		{ GD.Print("Missle target == null"); GetTarget(); return; }

		//LookAt(target.GlobalPosition);
		//GlobalPosition = GlobalPosition.MoveToward(target.GlobalPosition, (float)delta * speed);
		Transform3D targetRotation = Transform.LookingAt(target.GlobalPosition, Vector3.Up);
		Utility.QuaternionLerp(this, this.Transform, targetRotation, (float)delta * turnSpeed);
		TranslateObjectLocal(Vector3.Forward * (float)delta * speed);
	}
}

