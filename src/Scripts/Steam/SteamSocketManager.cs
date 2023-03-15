using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Godot;
using Newtonsoft.Json;
using Steamworks;
using Steamworks.Data;

public class SteamSocketManager : SocketManager
{
    public static event Action<Dictionary<string, string>> OnPlayerLeave;

    public override void OnConnected(Connection connection, ConnectionInfo info)
    {
        base.OnConnected(connection, info);
        
        GD.Print("Player has Connected (Host)");

        //send gamestate info to the client that just connected
        string str = JsonConvert.SerializeObject(GameManager.instance.GetGameState());
		Dictionary<string, string> data = new Dictionary<string, string>();
		data.Add("DataType", "GameState");
		data.Add("GameState", str);
		data.Add("PlayerId", info.Identity.SteamId.ToString());
		GD.Print("Sending GameState Data To Client");
		NetworkDataManager.SendMessage(data);
    }

    public override void OnConnecting(Connection connection, ConnectionInfo info)
    {
        base.OnConnecting(connection, info);
        GD.Print("New Player Connecting (Host)");
    }
    public override void OnDisconnected(Connection connection, ConnectionInfo info)
    {
        base.OnDisconnected(connection, info);
        GD.Print("Player has Disconnected (Host)");

        Dictionary<string, string> data = new Dictionary<string, string>();
        data.Add("DataType", "PlayerLeave");
        data.Add("Id", info.Identity.SteamId.ToString());
        NetworkDataManager.SendMessage(data);
        OnPlayerLeave?.Invoke(data);
    }
    //Server
    public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
        NetworkDataManager.ProcessData(data, size);
    }
}