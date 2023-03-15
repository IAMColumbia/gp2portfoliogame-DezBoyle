using System;
using System.Collections.Generic;
using Steamworks;

public class PlayerData
{
    public Godot.Vector3 Position { get; set; }
    public string Id { get; set; }
    public string PlayerName { get; set; }

    public PlayerData()
    {   }
    public PlayerData(Player player)
    {
        Position = player.GlobalPosition;
        Id = player.Name;
        PlayerName = player.NetworkPlayer.PlayerName;
    }

    public override string ToString()
    { return PlayerName + ": " + Id + " | " + Position; }
}

public class GameState
{
    public List<PlayerData> Players;
    public List<NetworkObjectData> NetworkObjects;

    public GameState()
    {   }
    public GameState(List<Player> players, List<NetworkObject> networkObjects)
    {
        Players = new List<PlayerData>();
        NetworkObjects = new List<NetworkObjectData>();
        Godot.GD.Print("Creating new Gamestate.  Players: " + players.Count + " NetworkObjects: " + NetworkObjects.Count);
        foreach(Player p in players)
        {
            PlayerData data = new PlayerData(p);
            Players.Add(data);
        }
        foreach(NetworkObject o in networkObjects)
        {
            NetworkObjectData data = o.Data;
            NetworkObjects.Add(data);
        }
    }
}