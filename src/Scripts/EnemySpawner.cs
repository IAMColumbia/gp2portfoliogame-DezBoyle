using System;
using Godot;

public partial class EnemySpawner : Node
{
    [Export] private bool enabled = true;
    [Export] private PackedScene enemyScene;
    [Export] private float spawnDelay = 4f;

    private float lastTimeSpawned = -420f;
    private Node3D[] spawnPoints;
    private int spawnIndex;

    public override void _Ready()
    {
        if(!enabled)
        { return; }
        spawnPoints = new Node3D[GetChildCount()];
        for(int i = 0; i < spawnPoints.Length; i++)
        { spawnPoints[i] = GetChild<Node3D>(i); }
    }

    public override void _Process(double delta)
    {
        if(!enabled)
        { return; }

        if(Time.GetTicksMsec() - lastTimeSpawned < spawnDelay * 1000)
        { return; }

        Enemy enemy = enemyScene.Instantiate<Enemy>();
        GameManager.instance.Level.AddChild(enemy);
        enemy.GlobalPosition = spawnPoints[spawnIndex].GlobalPosition;

        spawnIndex++;
        if(spawnIndex >= spawnPoints.Length)
        { spawnIndex = 0; }
        lastTimeSpawned = Time.GetTicksMsec();
    }
}