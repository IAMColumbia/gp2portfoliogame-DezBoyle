using Godot;
using Steamworks.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

public partial class UI : Node
{
	public static UI instance;

    public Control TitleScreen { get; private set; }

	[Export] private Texture2D crosshairTex_Normal;
	[Export] private Texture2D crosshairTex_Interact;
	[Export] private float crosshairMinSize = 20f;
	[Export] private float crosshairMaxSize = 75f;
	[Export] private PackedScene friendLobbyUIElementScene;
	[Export] private float hitMarkerDuration = 0.1f;

	private Vector2 crosshairTargetPos;
	private Player player;
    private TextureRect crosshair;
    private TextureRect hitMarker;
	private TextureRect healthIndicator;
	private VBoxContainer friendsLobbyListContainer;
	private float lastTimeHitmarker = -420f;
	private List<FriendLobbyUIElement> friendLobbyUIElements = new List<FriendLobbyUIElement>();
	private float crosshairSize;


	public override void _Ready()
	{
		if(instance != null)
		{
			Free();
			return;
		}
		
		instance = this;
		crosshair.GlobalPosition = GetCrosshairCenteredPosition();
        crosshairSize = crosshair.Size.X;
		SteamManager.OnMultiplayerLobbyRefreshCompleted += OnMultiplayerLobbyRefreshCompletedCallback;
	}

	public override void _EnterTree()
	{
        TitleScreen = GetNode<Control>("TitleScreen"); //forgor why i made this public
		crosshair = GetNode<TextureRect>("Crosshair");
		hitMarker = GetNode<TextureRect>("HitMarker");
		healthIndicator = GetNode<TextureRect>("HealthIndicator");
        friendsLobbyListContainer = GetNode<VBoxContainer>("TitleScreen/Friends/FriendsLobbyList");
	}

	public void Init(Player player)
	{
		this.player = player;
	}

	private float lastCrosshairSize;
	private bool crosshairSizeChanged = false;
	private void UpdateCrosshair(double delta)
	{
		crosshair.Visible = true;

		float targetSize = crosshairSize;

		//this is bad separation of concern.  The PlayerInteraction script shouldnt have anything to do with the UI
		switch(player.Interaction.CrosshairType)
		{
			case PlayerInteraction.CrosshairTypeEnum.Normal:
				crosshair.Texture = crosshairTex_Normal;
				crosshair.Visible = true;
				
				Weapon weapon = player.Inventory.HeldItem as Weapon;
				if(weapon != null)
				{ targetSize = Mathf.Lerp(crosshairMinSize, crosshairMaxSize, weapon.SpreadPercentage); }

				break;
			case PlayerInteraction.CrosshairTypeEnum.Interact:
				crosshair.Texture = crosshairTex_Interact;
				crosshair.Visible = true;
				break;
			case PlayerInteraction.CrosshairTypeEnum.Hidden:
				crosshair.Visible = false;
				break;
		}

		crosshair.Size = Vector2.One * targetSize;
		crosshairSizeChanged = crosshair.Size.X != lastCrosshairSize;
		lastCrosshairSize = crosshair.Size.X;

		//Position
        if (player.Interaction.Grabbing)
        {
            crosshairTargetPos = player.Cam.UnprojectPosition(player.Interaction.GrabPosition.GlobalPosition);
            crosshairTargetPos.X -= crosshair.Size.X / 2f;
            crosshairTargetPos.Y -= crosshair.Size.Y / 2f;
        }
        else
        { crosshairTargetPos = GetCrosshairCenteredPosition(); }

		if(!crosshairSizeChanged)
		{ crosshair.GlobalPosition = crosshair.GlobalPosition.Lerp(crosshairTargetPos, (float)delta * 32f); }
		else
		{ crosshair.GlobalPosition = crosshairTargetPos; } //dont lerp if we changing crosshair sizes
	}

	public override void _PhysicsProcess(double delta)
	{
		if(player == null)
        { return; }

        if (hitMarker.Visible && Time.GetTicksMsec() - lastTimeHitmarker > hitMarkerDuration * 1000)
		{ hitMarker.Visible = false; }

		UpdateCrosshair(delta);

		healthIndicator.Modulate = new Godot.Color(healthIndicator.Modulate.R, healthIndicator.Modulate.G, healthIndicator.Modulate.B, Mathf.Lerp(1f, 0f, player.Health.PercentHp));
		//healthIndicator.Modulate = new Godot.Color(healthIndicator.Modulate.R, healthIndicator.Modulate.G, healthIndicator.Modulate.B, 0f);
	}

    Vector2 crosshairPositionCenter;
	private Vector2 GetCrosshairCenteredPosition()
	{
		crosshairPositionCenter = GetViewport().GetVisibleRect().Size / 2f;
        crosshairPositionCenter.X -= crosshair.Size.X / 2f;
        crosshairPositionCenter.Y -= crosshair.Size.Y / 2f;
		return crosshairPositionCenter;
	}

	public void ShowHitmarker()
	{
		lastTimeHitmarker = Time.GetTicksMsec();
		hitMarker.Visible = true;
	}

	public void SetTitleScreenEnabled(bool Enabled)
	{
		UI.instance.TitleScreen.Visible = Enabled;
		Input.MouseMode = Enabled ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
	}

	private void OnMultiplayerLobbyRefreshCompletedCallback(List<Lobby> lobbies)
	{
		//TESTING
		if(SteamManager.Instance.Testing.HasValue && !lobbies.Contains(SteamManager.Instance.Testing.Value))
        { lobbies.Add(SteamManager.Instance.Testing.Value); }

		foreach(Lobby lobby in lobbies)
		{
			if(friendLobbyUIElements != null && friendLobbyUIElements.Count > 0)
			{
				bool uiAlreadyExists = friendLobbyUIElements.Any(x => x.Lobby.Id == lobby.Id);
				if(uiAlreadyExists)
            	{ continue; }
			}
			FriendLobbyUIElement ui = friendLobbyUIElementScene.Instantiate<FriendLobbyUIElement>();
			ui.Init(lobby);
			friendLobbyUIElements.Add(ui);
			friendsLobbyListContainer.AddChild(ui);
		}
	}
	
	private void _on_start_game_button_button_down()
	{
		SteamManager.Instance.CreateLobby();
	}

	private void _on_singleplayer_button_button_down()
	{
		GameManager.instance.StartSingleplayerGameDebug();
	}
}
