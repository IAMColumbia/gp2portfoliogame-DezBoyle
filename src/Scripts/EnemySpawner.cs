using System;
using Godot;

public partial class EnemySpawner : Node3D
{
    [Export] private bool enabled = true;
    [Export] private PackedScene enemyScene;
    [Export] private float spawnDelayMin = 4f;
    [Export] private float spawnDelayMax = 12f;
    [Export] private float spawnDelayDecreaseRate = 0.25f;
    [Export] private int amountToSpawn = 25;

    private float lastTimeSpawned = -420f;
    private Node3D[] spawnPoints;
    private int spawnIndex;
    private int amountSpawned;
    private float spawnDelay;

    public EnemySpawner()
    {
        Weapon.OnPickup += () => { enabled = true; };
    }

    public override void _Ready()
    {
        spawnPoints = new Node3D[GetChildCount()];
        for(int i = 0; i < spawnPoints.Length; i++)
        { spawnPoints[i] = GetChild<Node3D>(i); }
        spawnDelay = spawnDelayMax;
    }

    public override void _Process(double delta)
    {
        if(!enabled || amountSpawned >= amountToSpawn)
        { return; }

        spawnDelay -= (float)delta * spawnDelayDecreaseRate;
        spawnDelay = Mathf.Clamp(spawnDelay, spawnDelayMin, spawnDelayMax);
        GD.Print(spawnDelay);

        if(Time.GetTicksMsec() - lastTimeSpawned < spawnDelay * 1000)
        { return; }

        //TEMPORARY - set position to random player
        if(GameManager.Players == null || GameManager.Players.Count == 0)
        { return; }
		GlobalPosition = GameManager.Players[Utility.RandomRange(0, GameManager.Players.Count)].Cam.GlobalPosition;

        Enemy enemy = enemyScene.Instantiate<Enemy>();
        GameManager.instance.Level.AddChild(enemy);
        enemy.GlobalPosition = spawnPoints[spawnIndex].GlobalPosition;
        amountSpawned++;

        spawnIndex++;
        if(spawnIndex >= spawnPoints.Length)
        { spawnIndex = 0; }
        lastTimeSpawned = Time.GetTicksMsec();
    }
}