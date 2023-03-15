using System;
using Godot;

public partial class TracerSpawner : Node3D
{
    [Export] private int amount = 6;
    
    private Tracer[] tracers;
    private int index;

    public override void _Ready()
    {
        tracers = new Tracer[amount];
        tracers[0] = GetChild<Tracer>(0);

        for(int i = 1; i < amount; i++)
        {
            tracers[i] = (Tracer)tracers[0].Duplicate((int)15);
            tracers[0].GetParent().AddChild(tracers[i]);
        }
    }

    public void ShootAt(Vector3 target, float sqrDistanceToTravel, Vector3 defaultForward)
    {
        tracers[index].ShootAt(target, sqrDistanceToTravel, defaultForward);

        index++;
        if(index >= amount)
        { index = 0; }
    }
}