using System;
using Godot;

public partial class MuzzleFlare : Node3D
{
    [Export] private float duration = 0.04f;

    private float lastTime = -420f;

    public override void _Ready()
    {
        Visible = false;
    }

    public void Flash()
    {
        lastTime = Time.GetTicksMsec();
        Visible = true;
        RotateZ(Utility.RandomRange(-Mathf.Pi, Mathf.Pi));
    }

    public override void _Process(double delta)
    {
        if(!Visible || (Time.GetTicksMsec() - lastTime) < duration * 1000)
        { return; }

        Visible = false;
    }
}