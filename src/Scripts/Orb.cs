using System;
using Godot;

public partial class Orb : Node3D
{
    [Export] private float flyUpDuration = 4f;
    [Export] private float flyBackToHomeBaseDuration = 6f;

    enum State { Idle, FlyingUp, FlyingBackToHomeBase, Done }
    private State state = State.Idle;
    private Player player;
    private Vector3 upPosition;
    private Vector3 startPosition;
    private float t;

    public override void _Ready()
    {
        GetNode<Area3D>("Area3D").BodyEntered += OnBodyEnteredCallback;
    }

    public void OnBodyEnteredCallback(Node3D body)
    {
        if(player != null)
        { return; }

        player = body as Player;
        if(player == null)
        { return; }

        player.EnableMove = false;
        player.InsideOrb = true;
        upPosition = new Vector3(GlobalPosition.X, LevelManager.instance.OrbPosition.Y, GlobalPosition.Z);
        startPosition = GlobalPosition;
        state = State.FlyingUp;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if(state == State.FlyingUp)
        {
            t += (float)delta;
            GlobalPosition = startPosition.Lerp(upPosition, Utility.EvaulateCurve(Utility.Easing.Smoother, (t / flyUpDuration)));
            if(t > flyUpDuration)
            {
                state = State.FlyingBackToHomeBase;
                startPosition = GlobalPosition;
                t = 0f;
            }
        }
        else if(state == State.FlyingBackToHomeBase)
        {
            t += (float)delta;
            GlobalPosition = startPosition.Lerp(LevelManager.instance.OrbPosition, Utility.EvaulateCurve(Utility.Easing.Smooth, t / flyBackToHomeBaseDuration));
            if(t > flyBackToHomeBaseDuration)
            {
                state = State.Done;
                player.EnableMove = true;
                player.InsideOrb = false;
                Visible = true;
                LevelManager.instance.StartBattle();
                QueueFree();
            }
        }

        if(state == State.FlyingBackToHomeBase || state == State.FlyingUp)
        {
            player.GlobalPosition = player.GlobalPosition.Lerp(GlobalPosition, (float)delta * 12f);
        }
    }


}