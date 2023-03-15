using Godot;
using Steamworks.Data;
using System;

public partial class FriendLobbyUIElement : Control
{
    public Lobby Lobby { get; private set; }
	private Label label;

	// Called when the node enters the scene tree for the first time.

	public void Init(Lobby lobby)
	{
		this.Lobby = lobby;
		if(label == null)
		{ label = GetNode<Label>("Name"); }
		label.Text = lobby.GetData("Owner");
	}

	private void _on_join_button_button_down()
	{
		SteamManager.Instance.JoinLobbyButton(Lobby);
	}
}
