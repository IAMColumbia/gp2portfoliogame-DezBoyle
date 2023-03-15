using Godot;
using System;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public partial class SteamManager : Node
{
	public static SteamManager Instance { get; set; }

	private static uint gameAppId { get; set; } = 2256970;
	private const int maxPlayers = 16;

	public string PlayerName { get; set; }
	public SteamId PlayerSteamId { get; set; }
	public Lobby? Testing { get { return currentLobby; } }
	public bool IsHost { get; private set; }
	public SteamSocketManager SteamSocketManager { get; private set; }
	public SteamConnectionManager SteamConnectionManager { get; private set; }

	public static event Action<List<Lobby>> OnMultiplayerLobbyRefreshCompleted;

	private bool connectedToSteam = false;
	private Lobby? currentLobby;
	private List<Lobby> friendLobbies = new List<Lobby>();

	public override void _EnterTree()
	{
		if(Instance != null)
		{ QueueFree(); return; }

		Instance = this;

		if(GameManager.instance.Singleplayer)
		{ return; }

		try
		{
			SteamClient.Init(gameAppId, true);

			if(!SteamClient.IsValid)
			{ GD.PrintErr("Steam Client Invalid"); throw new Exception(); }

			PlayerName = SteamClient.Name;
			PlayerSteamId = SteamClient.SteamId;
			connectedToSteam = true;

			GD.Print("Connected to Steam.  Name: " + PlayerName);
		}
		catch(System.Exception e)
		{
			connectedToSteam = false;
			GD.PrintErr("Error connecting to steam : " + e.Message);
		}
	}

	public override void _Ready()
	{
		if(GameManager.instance.Singleplayer)
		{ return; }

		SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
		SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
		SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
		SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
		SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
		SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
		SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequestedCallback;
		SteamConnectionManager.OnClientDisconnected += OnClientDisconnectedCallback;
		
		RefreshMultiplayerLobbies();
	}

	public async Task<bool> CreateLobby()
	{
		if(currentLobby != null)
		{ return false; }
		try
		{
			GD.Print("Creating Lobby");
			Lobby? createLobbyOutput = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
			
			if(!createLobbyOutput.HasValue)
			{ GD.PrintErr("Tried to create lobby but it didnt instance correctly"); throw new Exception(); }

			currentLobby = createLobbyOutput.Value;
			//hostedLobby?.SetFriendsOnly();
			currentLobby?.SetPublic();      //TEMPORARY.  SHOULD BE FRIENDS ONLY IN ANY SERIOUS VERSION
			currentLobby?.SetJoinable(false);
			currentLobby?.SetData("Owner", PlayerName);
			currentLobby?.SetData("OwnerId", PlayerSteamId.ToString());
			currentLobby?.SetData("Level", GameManager.instance.LevelScene.ResourcePath);

			GD.Print("Lobby Created! with owner: " + currentLobby?.GetData("Owner"));
			return true;
		}
		catch(System.Exception e)
		{
			GD.PrintErr("Failed to Create Lobby : " + e.Message);
			return false;
		}
	}

	private void OnLobbyCreatedCallback(Result result, Lobby lobby)
	{
		if(result != Result.OK)
		{ GD.PrintErr("Error creating lobby"); return; }
		
		GD.Print("Lobby Created!  Id: " + lobby.Id);
		CreateSteamSocketServer();
	}
	private void OnLobbyEnteredCallback(Lobby lobby)
	{
		if(lobby.MemberCount > 0)
		{
			GD.Print($"Joined {lobby.Owner.Name}'s Lobby");
			currentLobby = lobby;
			lobby.SetGameServer(lobby.Owner.Id);
		}
		else
		{
			GD.Print($"Joined your own Lobby");
		}

		//join the server
		JoinSteamSocketServer(lobby.Owner.Id);
	}
	private void OnLobbyGameCreatedCallback(Lobby lobby, uint id, ushort port, SteamId steamId)
	{
		GD.Print("Lobby Game Created");
		GameManager.instance.JoinGame(currentLobby.Value);
	}
	private void OnLobbyMemberJoinedCallback(Lobby lobby, Friend friend)
	{ GD.Print("User " + friend.Name + " has joined the lobby"); }
	private void OnLobbyMemberDisconnectedCallback(Lobby lobby, Friend friend)
	{ GD.Print("User " + friend.Name + " has disconnected");}
	private void OnLobbyMemberLeaveCallback(Lobby lobby, Friend friend)
	{ GD.Print("User " + friend.Name + " has left");}
	
	private void OnGameLobbyJoinRequestedCallback(Lobby lobby, SteamId id)
	{ JoinLobbyGame(lobby); }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(GameManager.instance.Singleplayer || !connectedToSteam)
		{ return; }

		SteamClient.RunCallbacks();
		try
		{
			if(SteamSocketManager != null)
			{ SteamSocketManager.Receive(); }

			if(SteamConnectionManager != null && SteamConnectionManager.Connected)
			{ SteamConnectionManager.Receive();}
		}
		catch (System.Exception e)
		{
			GD.PrintErr("Exception in _Process of SteamManager : " + e.Message);
		}

		NetworkDataManager.Update();
	}

	public override void _Notification(int what)
	{
		base._Notification(what);
		if(!connectedToSteam)
		{ return; }
		if(what == NotificationWMCloseRequest)
		{ SteamClient.Shutdown(); GetTree().Quit(); }
	}

	public async void RefreshMultiplayerLobbies()
	{
		while(true)
		{
			await GetMultiplayerLobbies();
			await ToSignal(GetTree().CreateTimer(1), "timeout");
		}
	}

	public async Task<bool> GetMultiplayerLobbies()
	{
		try
		{
			// var friends = Steamworks.SteamFriends.GetFriends();
			
			// if(friends == null)
			// { throw new Exception("Friends null"); }

			// foreach(var friend in friends)
			// {
			// 	if(!friend.GameInfo.HasValue)
			// 	{ continue; }
			// 	Lobby? lobby = friend.GameInfo.Value.Lobby;
			// 	if(!lobby.HasValue)
			// 	{ continue; }
			// 	if(friendLobbies.Contains(lobby.Value))
			// 	{ continue; }
			// 	friendLobbies.Add(lobby.Value);
			// }


			//GD.Print("TEMPORARY: Getting Multiplayer Lobbies.  THIS SHOULD BE REMOVED IN RELEASE VERSIONS.  Use friends list instead");
			Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithMaxResults(10).RequestAsync();
			if (lobbies == null || lobbies.Length == 0)
			{ return false; }

			foreach(Lobby lobby in lobbies)
			{
				if(!friendLobbies.Contains(lobby))
				{ friendLobbies.Add(lobby); }
			}

			foreach(Lobby lobby in friendLobbies)
			{
				//GD.Print("    Lobby : " + lobby.GetData("Owner"));
			}
		}
		catch(System.Exception e)
		{
			GD.PrintErr("Error fetching Lobbies: " + e.Message);
			return false;
		}

		OnMultiplayerLobbyRefreshCompleted?.Invoke(friendLobbies);
		return true;
	}

	private void CreateSteamSocketServer()
	{
		try
		{
			SteamSocketManager = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>(0);
			SteamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(PlayerSteamId);
			IsHost = true;
			GD.Print("Created Socket Server");
		}
		catch(System.Exception e)
		{
			GD.PrintErr("Error Creating Socket Server : " + e.Message);
			LeaveLobby();
		}
	}

	private void JoinSteamSocketServer(SteamId host)
	{
		if(!IsHost)
		{
			try
			{
				SteamConnectionManager = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(host, 0);
				GD.Print("Joining Socket Server");
			}
			catch(System.Exception e)
			{
				GD.PrintErr("Error Joining Socket Server : " + e.Message);
				LeaveLobby();
			}
			
		}
	}

	public void Broadcast(string data, SendType sendType)
	{
		foreach(var item in SteamSocketManager.Connected.Skip(1).ToArray())
		{
			item.SendMessage(data, sendType);
		}
	}

	public void JoinLobbyButton(Lobby lobby)
	{
		JoinLobbyGame(lobby);
	}

	private async void JoinLobbyGame(Lobby lobby)
	{
		if(currentLobby.HasValue)
		{ return; }

		RoomEnter joinSuccessful = await lobby.Join();
		if(joinSuccessful != RoomEnter.Success)
		{
			GD.PrintErr("Failed to join lobby: " + joinSuccessful.ToString());
			LeaveLobby();
		}
		else
		{ GameManager.instance.JoinGame(lobby); }
	}

	public void OnLevelLoaded()
	{
		if(IsHost)
		{ currentLobby?.SetJoinable(true); }
	}

	private void LeaveLobby()
	{
		GD.Print("leaving lobby");
		if(currentLobby.HasValue)
		{ currentLobby.Value.Leave(); }
		currentLobby = null;
	}

	private void OnClientDisconnectedCallback()
	{
		LeaveLobby();
	}
}
