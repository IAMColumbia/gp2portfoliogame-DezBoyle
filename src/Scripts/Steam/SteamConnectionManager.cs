using System;
using Godot;
using Steamworks;
using Steamworks.Data;

public class SteamConnectionManager : ConnectionManager
{
    public static event Action OnClientDisconnected;

    public override void OnConnected(ConnectionInfo info)
    {
        base.OnConnected(info);
        GD.Print("Connected (Client)");
    }
    public override void OnConnecting(ConnectionInfo info)
    {
        base.OnConnecting(info);
        GD.Print("Connecting (Client)");
    }
    public override void OnDisconnected(ConnectionInfo info)
    {
        base.OnDisconnected(info);
        GD.Print("Disconnected (Client)");
        OnClientDisconnected?.Invoke();
    }
    //Client
    public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
    {
        base.OnMessage(data, size, messageNum, recvTime, channel);
        NetworkDataManager.ProcessData(data, size);
    }
}