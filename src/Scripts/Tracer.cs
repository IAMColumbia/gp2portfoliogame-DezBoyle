using System;
using Godot;

public partial class Tracer : Node3D
{
    [Export] private float tracerSpeed = 6f;

    private float sqrDistanceToTravel;
    private Vector3 originalPosition;
    private Vector3 originalGlobalPosition;
    private Vector3 originalRotation;
    private Node originalParent;
    private Vector3 targetGlobalPosition;
    private float lastTimeShot = -420f;
    private const float closeDistance = 1.5f;
    private const float sqrCloseDistance = closeDistance * closeDistance;

    public override void _Ready()
    {
        originalPosition = Position;
        originalParent = GetParent();
        Visible = false;
    }

    public void ShootAt(Vector3 target, float sqrDistanceToTravel, Vector3 defaultForward)
    {
        Visible = true;
        lastTimeShot = Time.GetTicksMsec();

        //re-add as child of original parent and reset local position
        GetParent().RemoveChild(this);
        originalParent.AddChild(this);
        Position = originalPosition;

        //reparent it to the world
        Utility.ChangeParent(this, GameManager.instance.Level);

        targetGlobalPosition = target;
        originalGlobalPosition = GlobalPosition;

        this.sqrDistanceToTravel = sqrDistanceToTravel;
        
        if (GlobalPosition.DistanceSquaredTo(target) >= sqrCloseDistance)
        { LookAt(target); }
        else
        { LookAt(defaultForward); }
    }

    public override void _Process(double delta)
    {
        if(!Visible)
        { return; }

        // I did this on accident but it looks good
        // the tracers accelerate exponentially but still look good at short and long range distances
        float t = ((Time.GetTicksMsec() - lastTimeShot) / 1000f) * tracerSpeed;
        GlobalPosition = GlobalPosition.MoveToward(targetGlobalPosition, t); 

        if(GlobalPosition.DistanceSquaredTo(targetGlobalPosition) <= 0.25f)
        { Visible = false; }
    }
}