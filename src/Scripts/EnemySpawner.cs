using System;
using System.Collections.Generic;
using Godot;

public partial class EnemySpawner : Node3D
{
    [Export] private bool enabled = true;
    [Export] private PackedScene enemyScene;
    [Export] private int totalEnemies = 25;
    [Export] private float waveDelayMin = 10f;
    [Export] private float waveDelayMax = 15f;
    [Export] private float waveDelayDecreaseRate = 0.25f;
    [Export] private int amountPerWave = 4;
    [Export] private float spawnDelay = .5f;

    private Node3D topLeft;
    private Node3D bottomRight;
    private List<Enemy> spawnedEnemies = new List<Enemy>();

    private float waveTimer;
    private float spawnTimer;
    private int enemiesToSpawnInWave;
    private int amountSpawned;
    private float waveDelay;

    public EnemySpawner()
    {
        //Weapon.OnPickup += () => { enabled = true; };
    }
    
    public void StartWave()
    {
        GD.Print("Starting wave");
        enabled = true;
        amountSpawned = 0;
        waveTimer = 123456789f;
        spawnTimer = 0f;
        waveDelay = waveDelayMax;
        enemiesToSpawnInWave = 0;
    }

    public void End()
    {
        enabled = false;
        foreach(Node n in spawnedEnemies)
        {
            if(n == null || n.NativeInstance == IntPtr.Zero)
            { continue; }

            n.QueueFree();
        }
        spawnedEnemies.Clear();
    }

    public override void _Ready()
    {
        topLeft = GetNode<Node3D>("Spawn_TopLeft");
        bottomRight = GetNode<Node3D>("Spawn_BottomRight");

        if(enabled)
        { StartWave(); }
    }

    public override void _Process(double delta)
    {
        if(!enabled)
        { return; }

        waveDelay -= (float)delta * waveDelayDecreaseRate;
        waveDelay = Mathf.Clamp(waveDelay, waveDelayMin, waveDelayMax);

        spawnTimer += (float)delta;
        waveTimer += (float)delta;

        if(enemiesToSpawnInWave > 0 && spawnTimer >= spawnDelay) //spawn enemies in a wave
        {
            Enemy enemy = enemyScene.Instantiate<Enemy>();
            spawnedEnemies.Add(enemy);
            GameManager.instance.AddChild(enemy);
            Vector3 spawnPos = new Vector3(Utility.RandomRange(topLeft.GlobalPosition.X, bottomRight.GlobalPosition.X), Utility.RandomRange(bottomRight.GlobalPosition.Y, topLeft.GlobalPosition.Y), topLeft.GlobalPosition.Z);
            enemy.GlobalPosition = spawnPos;
            amountSpawned++;
            enemiesToSpawnInWave--;
            spawnTimer = 0f;

            if(amountSpawned >= totalEnemies)
            {
                enabled = false;
                GD.Print("Enemies done spawning");
            }

            //TEMPORARY - set position to random player
            // if(GameManager.Players == null || GameManager.Players.Count == 0)
            // { return; }
            // GlobalPosition = GameManager.Players[Utility.RandomRange(0, GameManager.Players.Count)].Cam.GlobalPosition;
        }

        if(waveTimer > waveDelay) //new wave
        {
            GD.Print("New Wave " + waveDelay);
            enemiesToSpawnInWave += amountPerWave;
            waveTimer = 0f;
        }
    }
}