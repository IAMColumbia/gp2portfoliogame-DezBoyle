using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerDebug : Label
{
    [Export] private bool showDebug = false;
    public bool Enabled { get; set; } = true;

    private const int maxNetworkObjectsInList = 6;

    Player player;
    string output;

    public void Init(Player p)
    {
        player = p;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!Enabled || !showDebug)
        { Text = ""; return; }
        if (player == null || !player.NetworkPlayer.IsLocalPlayer)
        {
            SetPhysicsProcess(false);
            return;
        }
        output = "";
        output += $"Speed : {(int)player.Speed} / {(int)player.SpeedLimit}\n";
        output += "NETWORK OBJECTS: \n";
        List<NetworkObject> objs = NetworkObject.NetworkObjects;
        int startIndex = 0;
        if(objs.Count > maxNetworkObjectsInList)
        { startIndex = objs.Count - maxNetworkObjectsInList; }
        for(int i = startIndex; i < objs.Count; i++)
        {
            NetworkObject o = objs[i];
            if(o != null)
            {
                output += $"[{i}]: {o.Parent.Name}- Owner: {o.IsOwner}, InstParentPath: {o.Data.InstanceParentPath}\n";
                if(o.Data.SyncData != null)
                { output += $"SyncData: {o.Data.SyncData.ToString()}\n"; }
            }
        }
		
        Text = output;
    }
}
