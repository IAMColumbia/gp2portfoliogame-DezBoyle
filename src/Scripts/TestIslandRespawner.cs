using System;
using Godot;

partial class TestIslandRespawner : Node3D
{
    public override void _Process(double delta)
    {
        if (!Input.IsKeyPressed(Key.P))
        { return; }

        foreach(Island island in GetChildren())
        {
            if(island == null)
            { continue; }

            if(island.SceneFilePath == "")
            { continue; }

            Node3D newIsland = (GD.Load<PackedScene>(island.SceneFilePath)).Instantiate<Node3D>();
            island.GetParent().AddChild(newIsland);
            newIsland.GlobalPosition = island.GlobalPosition;
            newIsland.GlobalRotation = island.GlobalRotation;
            island.QueueFree();
        }
    }
}