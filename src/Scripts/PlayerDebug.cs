using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerDebug : Label
{
    [Export] private bool showDebug = false;
    public bool Enabled { get; set; } = false;

    private const int maxNetworkObjectsInList = 6;

    Player player;
    string output;
    PhysicsRayQueryParameters3D rayParams;

    public void Init(Player p)
    {
        player = p;
        Text = "";

        Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid>();
        exclude.Add(new Rid(player.PlayerBody as Godot.GodotObject));
        rayParams = PhysicsRayQueryParameters3D.Create(player.Cam.GlobalPosition, player.Cam.GlobalPosition + player.Cam.GlobalTransform.Basis.Z * 10f, 1, exclude);
        rayParams.CollideWithAreas = true;
    }

    Node node;
    Godot.Collections.Dictionary hit;
    public override void _PhysicsProcess(double delta)
    {
        Text = "";
        if(!Enabled || !showDebug)
        { return; }
        if (player == null || !player.NetworkPlayer.IsLocalPlayer)
        {
            SetPhysicsProcess(false);
            return;
        }
        rayParams.From = player.Cam.GlobalPosition;
        rayParams.To = player.Cam.GlobalPosition - player.Cam.GlobalTransform.Basis.Z * 10f;
        hit = player.GetWorld3D().DirectSpaceState.IntersectRay(rayParams);

        output = "\n";
        if (hit != null && hit.Count > 0) 
        {
            node = (Node)hit["collider"];
            output += $"RaycastHit: {node.Name}: {node.GetPath()}\n";
            output += $"\n";
        }

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
