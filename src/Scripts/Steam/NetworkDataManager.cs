using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Steamworks;
using Steamworks.Data;
using Newtonsoft.Json;

public class NetworkDataManager
{
    public static event Action<Dictionary<string, string>> OnChatMessage;
    public static event Action<Dictionary<string, string>> OnGameStateReceived;
    public static event Action<Dictionary<string, string>> OnPlayerUpdate;
    public static event Action<Dictionary<string, string>> OnNewPlayer;
    public static event Action<Dictionary<string, string>> OnPlayerLeave;
    public static event Action<Dictionary<string, string>> OnFart;
    public static event Action<Dictionary<string, string>> OnRequestToRegisterNetworkObject;
    public static event Action<Dictionary<string, string>> OnRegisteredNetworkObject;
    public static event Action<Dictionary<string, string>> OnUpdateNetworkObjectSyncData;
    public static event Action<Dictionary<string, string>> OnUpdateNetworkObjectData;

    private static Queue<Dictionary<string, string>> ReliableMessages_ToClients = new Queue<Dictionary<string, string>>();
    private static Queue<Dictionary<string, string>> UnreliableMessages_ToClients = new Queue<Dictionary<string, string>>();
    private static Queue<Dictionary<string, string>> ReliableMessages_ToHost = new Queue<Dictionary<string, string>>();
    private static Queue<Dictionary<string, string>> UnreliableMessages_ToHost = new Queue<Dictionary<string, string>>();

    public static Dictionary<string, string> ParseData(IntPtr data, int size)
    {
        byte[] managedArray = new byte[size];
        Marshal.Copy(data, managedArray, 0, size);
        var str = System.Text.Encoding.Default.GetString(managedArray);
        Dictionary<string, string> dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
        return dict;
    }

    public static void ProcessData(IntPtr data, int size)
    {
        var dict = ParseData(data, size);

        if(dict.ContainsKey("RelayToClients") && dict["RelayToClients"] == "True")
        {
            SendMessage(dict);
            if(!SteamManager.Instance.IsHost)
            { Godot.GD.PrintErr("Trying to relay to clients as a client and not a host"); }
        }

        if(!GameManager.instance.ConnectedToGame && dict["DataType"] != "GameState")
        { return; } //disable all messages besides GameState so things stay sychronized

        switch(dict["DataType"])
        {
            case "ChatMessage":
                OnChatMessage?.Invoke(dict);
            break;
            case "GameState":
                OnGameStateReceived?.Invoke(dict);
            break;
            case "UpdatePlayer":
                OnPlayerUpdate?.Invoke(dict);
            break;
            case "NewPlayer": //new player has joined the game and sends this data to the host
                OnNewPlayer?.Invoke(dict); //call on host
            break;
            case "PlayerLeave":
                OnPlayerLeave?.Invoke(dict);
            break;
            case "Fart":
                OnFart?.Invoke(dict);
            break;
            case "RequestToRegisterNetworkObject":
                OnRequestToRegisterNetworkObject?.Invoke(dict);
            break;
            case "RegisteredNetworkObject":
                OnRegisteredNetworkObject?.Invoke(dict);
            break;
            case "UpdateNetworkObjectSyncData":
                OnUpdateNetworkObjectSyncData?.Invoke(dict);
            break;
            case "UpdateNetworkObjectData":
                OnUpdateNetworkObjectData?.Invoke(dict);
            break;
            default:
            break;
        }
    }

    //If we are a client, the message only goes to the host, if we are the host, the message goes to all the clients
    public static void SendMessage(Dictionary<string, string> data, SendType sendType = SendType.Reliable)
    {
		if(SteamManager.Instance.IsHost)
        {
            if(!data.ContainsKey("RelayToClients"))
            { data.Add("RelayToClients", "False"); }
            else
            { data["RelayToClients"] = "False"; }

            SendToClients(data, sendType);
        }
		else
		{
			if(SteamManager.Instance.SteamConnectionManager != null && SteamManager.Instance.SteamConnectionManager.Connection != null)
            {
                if (!data.ContainsKey("RelayToClients"))
                { data.Add("RelayToClients", "True"); }
                else
                { data["RelayToClients"] = "True"; }

                SendToHost(data, sendType);
            }
            else
            { Godot.GD.PrintErr("Tried to send message with null SteamConnectionManager.  DataType: " + data["DataType"]); }
		}
    }

    public static void Update()
    {
        if (SteamManager.Instance.SteamConnectionManager == null)
        { return; }

        //handle queues
        //THIS IS THE UNOPTIMIZED VERSION
        //However..
        //this is ready to be optimized!
        //need to pack the entire Queue into a string to send at once
        //then loop through the queue once its RECIEVED
        //DO IT
        
        string json;
        Dictionary<string, string> data;
        while(ReliableMessages_ToClients.Count > 0)
        {
            data = ReliableMessages_ToClients.Dequeue();
            json = JsonConvert.SerializeObject(data);
            SteamManager.Instance.Broadcast(json, SendType.Reliable);
        }
        while(ReliableMessages_ToHost.Count > 0)
        {
            data = ReliableMessages_ToHost.Dequeue();
            json = JsonConvert.SerializeObject(data);
            SteamManager.Instance.SteamConnectionManager.Connection.SendMessage(json, SendType.Reliable);
        }
        while(UnreliableMessages_ToClients.Count > 0)
        {
            data = UnreliableMessages_ToClients.Dequeue();
            json = JsonConvert.SerializeObject(data);
            SteamManager.Instance.Broadcast(json, SendType.Unreliable);
        }
        while(UnreliableMessages_ToHost.Count > 0)
        {
            data = UnreliableMessages_ToHost.Dequeue();
            json = JsonConvert.SerializeObject(data);
            SteamManager.Instance.SteamConnectionManager.Connection.SendMessage(json, SendType.Unreliable);
        }
    }

    /// from steamworks docs :
    ///The only caveat is related to performance: there is per-message overhead to retain the message sizes, and so if your code sends many small chunks of data, performance will suffer.
    //Any code based on stream sockets that does not write excessively small chunks will work without any changes.

    public static void SendToHost(Dictionary<string, string> data, SendType sendType = SendType.Reliable)
    {
        //string json = JsonConvert.SerializeObject(data);
        //SteamManager.Instance.SteamConnectionManager.Connection.SendMessage(json, sendType);
        QueueMessage(data, true, sendType);
    }
    public static void SendToClients(Dictionary<string, string> data, SendType sendType = SendType.Reliable)
    {
        //string json = JsonConvert.SerializeObject(data);
        //SteamManager.Instance.Broadcast(json, sendType);
        QueueMessage(data, false, sendType);
    }
    private static void QueueMessage(Dictionary<string, string> data, bool toHost, SendType sendType)
    {
        if(sendType == SendType.Reliable)
        {
            if(toHost)
            { ReliableMessages_ToHost.Enqueue(data); }
            else
            { ReliableMessages_ToClients.Enqueue(data); }
        }
        else
        {
            if(toHost)
            { UnreliableMessages_ToHost.Enqueue(data); }
            else
            { UnreliableMessages_ToClients.Enqueue(data); }
        }
    }
}