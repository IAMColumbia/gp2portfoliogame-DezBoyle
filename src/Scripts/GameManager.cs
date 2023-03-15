using Godot;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public partial class GameManager : Node
{
	[Export] private bool singleplayer = false;
	[Export] private bool allowCheats = false;
	
	public static GameManager instance;
	public static List<Player> Players = new List<Player>();
	public PackedScene LevelScene { get { return levelScene; } }
	public PackedScene PlayerScene { get { return playerScene; } }
	public bool Singleplayer { get {return singleplayer;} }
	public bool ConnectedToGame { get; private set; }
	public bool AllowCheats { get {return allowCheats;} }
	public LevelManager Level { get; private set; }

	[Export] private PackedScene levelScene;
	[Export] private PackedScene playerScene;

	private Player localPlayer;


	public GameManager()
	{
		if(instance != null)
		{
			QueueFree();
			return;
		}
		instance = this;
	}

	public override void _Ready()
	{
		NetworkDataManager.OnGameStateReceived += OnGameStateReceivedCallback;
		NetworkDataManager.OnNewPlayer += OnNewPlayerCallback;
		NetworkDataManager.OnPlayerLeave += OnPlayerLeaveCallback;
		SteamSocketManager.OnPlayerLeave += OnPlayerLeaveCallback; //so the host also gets it
		SteamConnectionManager.OnClientDisconnected += OnClientDisconnectedCallback;

		if(singleplayer)
		{
			UI.instance.SetTitleScreenEnabled(false);
			LoadLevel();
			if(Level.GetNodeOrNull("Player") == null)
			{ SpawnLocalPlayer(); }
		}
	}

	public void LoadLevel() {LoadLevel(LevelScene.ResourcePath);}
	public void LoadLevel(string path)
	{
		if(Level != null)
		{
			Level.QueueFree();
		}
		GD.Print("Loading Level from Path: " + path);
		Level = GD.Load<PackedScene>(path).Instantiate<LevelManager>();
		AddChild(Level);
	}

	public void ResetGame()
	{
        CleanupPlayers();
		LoadLevel();
		if(!singleplayer)
        { GD.PrintErr("this doesnt work with multiplayer yet"); }
		SpawnLocalPlayer();	
	}

	private Player SpawnLocalPlayer() //for debugging single player purposes
	{
		Player p = PlayerScene.Instantiate<Player>();
		if(singleplayer)
		{
			AddChild(p);
			Players.Add(p);
			p.GlobalPosition = Level.PlayerSpawn.GlobalPosition;
		}
		return p;
	}

	public void ResetToTitleScreen()
	{
		GD.Print("Resetting Game");
		if(Level != null)
		{ Level.QueueFree(); }
		UI.instance.SetTitleScreenEnabled(true);
		CleanupPlayers();
		NetworkObject.Reset();
		ConnectedToGame = false;
	}

	private void CleanupPlayers()
	{
		if (Players.Count != 0)
		{
			foreach (Player p in Players)
			{ p.QueueFree(); }
		}
        Players.Clear();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);
		//fullscreen toggle
		if(Input.IsActionJustPressed("fullscreen"))
		{
			if(DisplayServer.WindowGetMode() != DisplayServer.WindowMode.ExclusiveFullscreen)
			{ DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen); }
			else
			{
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
				DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
			}
		}
	}

	public void StartSingleplayerGameDebug()
	{
		singleplayer = true;
		UI.instance.SetTitleScreenEnabled(false);
		LoadLevel();
		if (Level.GetNodeOrNull("Player") == null)
		{ SpawnLocalPlayer(); }
	}

	public void JoinGame(Lobby lobby)
	{
		if(SteamManager.Instance.IsHost)
		{ ConnectedToGame = true; }
		GD.Print("Joining Game...");
		UI.instance.SetTitleScreenEnabled(false);
		GD.Print("Loading Level from Lobby... " + lobby.GetData("Level"));
		LoadLevel(lobby.GetData("Level"));
		localPlayer = PlayerScene.Instantiate<Player>();
		localPlayer.Name = SteamManager.Instance.PlayerSteamId.ToString();
		AddChild(localPlayer);
		Players.Add(localPlayer);
		localPlayer.GlobalPosition = Level.PlayerSpawn.GlobalPosition;
		localPlayer.NetworkPlayer.SetNameLabel(SteamManager.Instance.PlayerName);
		SteamManager.Instance.OnLevelLoaded();
	}

	public GameState GetGameState()
	{
		//package up gamestate
		return new GameState(Players, NetworkObject.NetworkObjects);
	}

	private void AddOtherPlayer(PlayerData data)
	{
		Player p = PlayerScene.Instantiate<Player>();
		p.Name = data.Id;
		AddChild(p);
		Players.Add(p);
		p.GlobalPosition = data.Position;
		p.NetworkPlayer.SetNameLabel(data.PlayerName);
	}

	public void LoadGameState(GameState gameState)
	{
		//loads the state of the game
		//spawn in previously connected players
		foreach(PlayerData player in gameState.Players)
		{ AddOtherPlayer(player); }
		//TODO: sync networkObjects
		foreach(NetworkObjectData data in gameState.NetworkObjects)
		{
			if(data.WasInstanced)
			{ data.NodePath = null; GD.Print("LoadingGameState: NetworkObject was Instanced, deleting NodePath"); }
			NetworkObject.ApplyNetworkObjectToScene(data);
		}
	}

	private void OnGameStateReceivedCallback(Dictionary<string, string> data)
	{
		if(data["PlayerId"] != SteamManager.Instance.PlayerSteamId.ToString())
		{ return; }
		GD.Print("Receiving GameState.  RAW: " + data["GameState"]);
		
		GameState gameState = JsonConvert.DeserializeObject<GameState>(data["GameState"]);
		
		GD.Print(gameState.Players.Count + " PLAYERS:");
		foreach(PlayerData p in gameState.Players)
		{ GD.Print("    " + p.ToString()); }
		GD.Print(gameState.NetworkObjects.Count + " NETWORK OBJECTS:");
		foreach(NetworkObjectData o in gameState.NetworkObjects)
		{ GD.Print("    " + o.ToString()); }
		
		LoadGameState(gameState);
		ConnectedToGame = true;
		
		//send message to all other clients with our playerData so they see us
		string json = JsonConvert.SerializeObject(new PlayerData(localPlayer));
		Dictionary<string, string> playerData = new Dictionary<string, string>();
		playerData.Add("DataType", "NewPlayer");
		playerData.Add("PlayerData", json);
		playerData.Add("PlayerId", SteamManager.Instance.PlayerSteamId.ToString());
		NetworkDataManager.SendMessage(playerData);
		GD.Print("GameState Received.  Sending PlayerData");
	}

	private void OnNewPlayerCallback(Dictionary<string, string> data)
	{
		GD.Print("OnNewPlayerCallback");
		//spawn in newly connected player
		if(data["PlayerId"] == SteamManager.Instance.PlayerSteamId.ToString())
		{ return; }

		PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(data["PlayerData"]);
		AddOtherPlayer(playerData);
		GD.Print("Adding newly connected player: " + playerData.PlayerName);
	}

	private void OnPlayerLeaveCallback(Dictionary<string, string> data)
	{
		Player playerToDelete = GetNode<Player>(data["Id"]);
		if(!Players.Contains(playerToDelete))
		{ GD.PrintErr("Trying to clean up player that isnt on the list"); return; }
		
		GD.Print("Cleaing up disconnected player   id: " + data["Id"]);
		Players.Remove(playerToDelete);
		playerToDelete.Enabled = false;
		playerToDelete.QueueFree();
	}

	private void OnClientDisconnectedCallback()
	{
		ResetToTitleScreen();
	}

    //public Node3D SpawnObject(PackedScene packedScene) { return SpawnObject(packedScene, null); }
	public Node3D SpawnObject(PackedScene packedScene, Node parent = null)
	{
		if(!GameManager.instance.ConnectedToGame && !Singleplayer)
		{ GD.PrintErr("Tried to spawn an object before ConnectedToGame"); return null; }

		if(parent == null)
		{ parent = Level; }

		Node3D obj = packedScene.Instantiate<Node3D>();
		if(!Singleplayer)
		{
			NetworkObject networkObject = obj.GetNode<NetworkObject>("NetworkObject");
			networkObject.Data = new NetworkObjectData();
			networkObject.Data.PackedScenePath = packedScene.ResourcePath;
			networkObject.Data.InstanceParentPath = parent.GetPath();
			networkObject.Data.OwnerId = SteamManager.Instance.PlayerSteamId.ToString();
		}
		parent.AddChild(obj);
		return obj;
	}

	public void DeleteObject(NetworkObject networkObject)
	{
		GD.Print("Deleting Object.  Still needs to work over the network");
		networkObject.Parent.QueueFree();
	}
}
