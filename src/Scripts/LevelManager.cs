using Godot;
using System;

public partial class LevelManager : Node
{
	public static LevelManager instance;

	[Export] private NodePath playerSpawnPath;
    [Export] private Node3D orbPosition;
    [Export] private Node3D level;
    [Export] private Node3D portal;
    [Export] private float fightDuration = 50f;
    [Export] private float fightDistance = 400f;

	public Node3D PlayerSpawn { get { return playerSpawn; } }
    public Vector3 OrbPosition { get { return orbPosition.GlobalPosition; } }

    private enum State { Explore, Fight }

	private LevelGenerator levelGenerator;
	private EnemySpawner enemySpawner;

	private Node3D playerSpawn;
	private Player player;
	private State state = State.Explore;
	private float fightTimer;
    Vector3 levelStartPos;
	Vector3 endPosition;
	Vector3 portalEndPosition;

	public LevelManager()
	{
		instance = this;
	}

	public override void _EnterTree()
	{
		playerSpawn = GetNode<Node3D>(playerSpawnPath);
		GetNode<Area3D>("FallTrigger").BodyEntered += FallTrigger;
		levelGenerator = GetNodeOrNull<LevelGenerator>("LevelGenerator");
		enemySpawner = GetNodeOrNull<EnemySpawner>("EnemySpawner");
	}

	public override void _Ready()
	{
		endPosition = new Vector3(0f, 0f, fightDistance);
		portalEndPosition = portal.GlobalPosition - endPosition;

        if (portal.GetParentOrNull<Node>() != null)
		{ portal.GetParent().RemoveChild(portal); }
	}

	public void FallTrigger(Node3D body)
	{
		player = body as Player;

		if(player != null)
		{ player.RespawnToLastIsland(PlayerSpawn.GlobalPosition); }
	}

	public void StartBattle()
	{
		orbPosition.Visible = true;
		state = State.Fight;
		fightTimer = 0f;

		level = levelGenerator.CurrentLevel;
        levelStartPos = level.GlobalPosition;
		
		level.AddChild(portal);
		portal.GlobalPosition = portalEndPosition;

        enemySpawner.StartWave();
	}

	private void ThroughPortal()
	{
        state = State.Explore;
		level.RemoveChild(portal);
		enemySpawner.End();
		levelGenerator.NewLevel();
	}

	public override void _Process(double delta)
	{
		if(state == State.Fight)
		{
			fightTimer += (float)delta;
			level.GlobalPosition = levelStartPos.Lerp(endPosition, (fightTimer / fightDuration));
			// GD.Print("FIGHTTIMER: " + fightTimer);
			if(fightTimer > fightDuration)
			{
				ThroughPortal();
			}
		}
	}
}
