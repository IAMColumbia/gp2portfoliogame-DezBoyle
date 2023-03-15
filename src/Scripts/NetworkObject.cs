using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NetworkObjectData
{
    public int? Id { get; set; } = null;
    public string ParentName { get; set; } = null; //Parent Node name
    public string OwnerId { get; set; } = null; //steamId of the owner
    public string NodePath { get; set; } = null; //path to this NetworkObject in the scene
    public string OldNodePath { get; set; } = null; //path to this NetworkObject before it's name change (goofy)
    public string InstanceParentPath { get; set; } = null; //where to parent the instance (PackedScene)
    public string PackedScenePath { get; set; } = null; //path to the packedscene if this is an instance
    public bool WasInstanced { get; set; } //False = this object is normally in the scene.  True = this object was spawned in by a player
    public NetworkObjectSyncData SyncData { get; set; } = null;
    public NetworkObjectSyncData LastSyncData { get; set; } = null;

    public NetworkObjectData()
    {   }
	// public NetworkObjectData(NetworkObject obj)
	// {
	// 	if(!obj.IsInsideTree())
	// 	{ return; }

    //     NodePath = obj.GetPath();
	// }

    public override string ToString()
    { return $"    Id: {Id}\n    OwnerId: {OwnerId}\n    NodePath: {NodePath}\n    PackedScenePath: {PackedScenePath}\n"; }
}

public class NetworkObjectSyncData
{
    public int? Id { get; set; }
	public Vector3 ParentPosition { get; set; }
	public Vector3 ParentRotation { get; set; }

	public NetworkObjectSyncData()
	{	}
	public NetworkObjectSyncData(NetworkObject obj)
	{
		if(!obj.IsInsideTree())
		{ return; }

		ParentPosition = obj.Parent.Position;
		ParentRotation = obj.Parent.Rotation;
		Id = obj.Data.Id;
	}
    public override string ToString()
    {
        return $"Pos({ParentPosition}) Rot({ParentRotation})";
    }
}

public partial class NetworkObject : Node
{
    public static List<NetworkObject> NetworkObjects { get; private set; } = new List<NetworkObject>();

    public NetworkObjectData Data { get; set; }
    public Node3D Parent { get; private set; }
    public bool IsOwner { get; private set; }

	private const float syncInterval = 0.1f; //delay between transmissions
	private const float syncDifference = 0.025f; //only transmit sync if the values changed by this much
	private const float sqrSyncDifference = syncDifference * syncDifference;
    private float lastTimeReceivedUpdate = -420f;
	private float lastTimeSentUpdate = -420f;

	private static bool setup = false;
	private static int spawnedNetObjectCount = 0;

	public NetworkObject()
	{
		if(setup)
		{ return; }
		NetworkDataManager.OnRegisteredNetworkObject += OnRegisteredNetworkObjectCallback;
		NetworkDataManager.OnUpdateNetworkObjectSyncData += OnUpdateNetworkObjectSyncDataCallback;
		NetworkDataManager.OnRequestToRegisterNetworkObject += OnRequestToRegisterNetworkObjectCallback;
		NetworkDataManager.OnUpdateNetworkObjectData += OnUpdateNetworkObjectDataCallback;
		setup = true;
	}

	public static void Reset()
	{
		spawnedNetObjectCount = 0;
		NetworkObjects.Clear();
	}

	#region Initialization
    public override void _Ready()
    {
        base._Ready();

		if(Data == null) //generate if we weren't assigned one yet
		{ Data = new NetworkObjectData(); }

		if(Data.Id.HasValue && GetNetworkObjectFromList(Data.Id.Value) != null)
		{ return; } //we are already initialized (I DONT THINK THIS FULLY WORKS UHHHHHHH... Maybe pass a bool in here instead of checking the list- then add to the list.)

		Parent = GetParent<Node3D>();
		if(Data.OldNodePath == null)
        { Data.OldNodePath = GetPath(); }
        if (Data.ParentName != null)
        { Parent.Name = Data.ParentName; }
		Data.NodePath = GetPath();

		if(Data.OwnerId != null)
		{ RecalculateOwner(); }
		
		if(SteamManager.Instance.IsHost && (Data.OwnerId == null || IsOwner)) //register on host if the OwnerId wasnt set by a client (exists in scene) OR the host owns it (host spawned it)
		{
			GD.Print("Host registering Object");
			Data.OwnerId = SteamManager.Instance.PlayerSteamId.ToString();
			IsOwner = true;
			Register(Data, true);
		}
		else if(IsOwner)
		{
			GD.Print("Client Request to Register Object");
			//request to register request to host
			Dictionary<string, string> data = new Dictionary<string, string>();
			string json = JsonConvert.SerializeObject(Data);
			data.Add("DataType", "RequestToRegisterNetworkObject");
			data.Add("NetworkObjectData", json);
			NetworkDataManager.SendToHost(data);
		}
    }

	private void RecalculateOwner()
	{ IsOwner = Data.OwnerId == SteamManager.Instance.PlayerSteamId.ToString(); }

	#endregion

	#region Update Sync
	//This code needs to be un-nested
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if(!IsOwner)
		{
			if(Data.SyncData != null && Data.LastSyncData != null)
			{
				float t = (Time.GetTicksMsec() - lastTimeReceivedUpdate) / (syncInterval * 1000f);
                t = Mathf.Clamp(t, 0, 1);
				Parent.Position = Data.LastSyncData.ParentPosition.Lerp(Data.SyncData.ParentPosition, t);
				Parent.Rotation = Data.SyncData.ParentRotation; //not lerping this yet cuz gimbal lock spooky.  idk
			}
		}
		else if(Time.GetTicksMsec() - lastTimeSentUpdate >= syncInterval * 1000f)
		{
			if(Data.LastSyncData != null)
			{
				bool different = false; //has it changed enough to be worth transmitting?
				if(Data.LastSyncData.ParentPosition.DistanceSquaredTo(Parent.Position) > sqrSyncDifference)
				{ different = true; }
				else if(Data.LastSyncData.ParentRotation.DistanceSquaredTo(Parent.Rotation) > sqrSyncDifference)
				{ different = true; }
				if(!different)
				{ return; }
			}

			//update SyncData
			if(Data != null && Data.Id.HasValue) //ready to sync
            {
				Data.LastSyncData = Data.SyncData;
				Data.SyncData = new NetworkObjectSyncData(this);
			}
			
			if(Data.SyncData == null)
			{ return; }
			
			Data.SyncData.ParentPosition = Parent.Position;
            Data.SyncData.ParentRotation = Parent.Rotation;
			
			//send SyncData
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("DataType", "UpdateNetworkObjectSyncData");
			string json = JsonConvert.SerializeObject(Data.SyncData);
			data.Add("NetworkObjectSyncData", json);
			NetworkDataManager.SendMessage(data, Steamworks.Data.SendType.Unreliable);

			lastTimeSentUpdate = Time.GetTicksMsec();
		}
	}

	//update with syncData
    private static void OnUpdateNetworkObjectSyncDataCallback(Dictionary<string, string> data)
	{
		NetworkObjectSyncData receivedData = JsonConvert.DeserializeObject<NetworkObjectSyncData>(data["NetworkObjectSyncData"]);
		if(!receivedData.Id.HasValue)
		{ GD.PrintErr("Receiving SyncData without an Id"); return; }
		NetworkObject networkObject = GetNetworkObjectFromList(receivedData.Id.Value);
		if(networkObject.IsOwner)
		{ return; }
		networkObject.Data.LastSyncData = networkObject.Data.SyncData;
		networkObject.Data.SyncData = receivedData;
		networkObject.lastTimeReceivedUpdate = Time.GetTicksMsec();
	}
	#endregion

	#region Registration

	private void OnRequestToRegisterNetworkObjectCallback(Dictionary<string, string> data)
	{
		if(!SteamManager.Instance.IsHost) //should be host anyways, I'm just making sure
		{ return; }

		NetworkObjectData networkObjectData = JsonConvert.DeserializeObject<NetworkObjectData>(data["NetworkObjectData"]);
		GD.Print("Host registering for a client request");
		Register(networkObjectData, false);
	}

	//Registering on the Host
	public static void Register(NetworkObjectData objectData, bool spawnedLocally)
	{
		if(objectData.Id != null)
		{ GD.PrintErr("Register Being Called Twice : NetworkObject.cs"); return; }

		NetworkObject networkObject;

		//Id
		if(!objectData.Id.HasValue) //Generate an Id if it needs one
		{ objectData.Id = NetworkObjects.Count; }

        //Owner Check
        if(objectData.OwnerId == null)
		{ GD.PrintErr("OwnerId not set! : NetworkObject.cs"); return; }

		//InstanceParentPath Check
		if(objectData.PackedScenePath != null && objectData.InstanceParentPath == null)
		{ GD.PrintErr("InstanceParentPath is null.  Set this if you want to spawn from a PackedScene"); return; }

        //manually set the name if the object was instanced to avoid the godot @ in the name, which can vary from client to client (different NodePaths = big error time)
        if(objectData.PackedScenePath != null)
		{
			PackedScene packedScene = GD.Load<PackedScene>(objectData.PackedScenePath);
			objectData.ParentName = $"NetObj_{packedScene.ResourceName}_{spawnedNetObjectCount}";
			spawnedNetObjectCount++;
		}

		//spawn / apply it locally
		if(spawnedLocally)
        { networkObject = ApplyNetworkObjectToScene(objectData); }

		//set NodePath to null before sending it over the network (if it was an instanced scene)
		if(objectData.PackedScenePath != null && objectData.NodePath != null)
		{ objectData.NodePath = null; }

		//need to do this after setting nodepath to null if it wasnt spawned locally. goofy
		if(!spawnedLocally)
		{ networkObject = ApplyNetworkObjectToScene(objectData); }

		//Relay information to clients
		Dictionary<string, string> data = new Dictionary<string, string>();
		data.Add("DataType", "RegisteredNetworkObject");
		string json = JsonConvert.SerializeObject(objectData);
		data.Add("NetworkObjectData", json);
		NetworkDataManager.SendToClients(data);

		GD.Print("Registered Object on Host: " + objectData.Id);
	}

	private static NetworkObject GetNetworkObjectFromList(int id)
	{
		if(NetworkObjects.Count <= id)
		{ return null; }
		return NetworkObjects[id];
	}

    //takes in data, applies it to NetworkObject in scene.  If not already in the scene, it spawns it in
    public static NetworkObject ApplyNetworkObjectToScene(NetworkObjectData data)
	{
		bool owner = data.OwnerId == SteamManager.Instance.PlayerSteamId.ToString();
		if(!owner && data.PackedScenePath != null) //this was instanced, needs to be spawned in - a NodePath doesnt exist for us yet
		{ data.NodePath = null; }

		NetworkObject networkObject = GetNetworkObjectFromList(data.Id.Value);
		bool justInstanced = false;

		if(networkObject == null && data.PackedScenePath != null && data.NodePath == null) 
		{
			GD.Print("Spawning in id: " + data.Id);
			PackedScene packedScene = GD.Load<PackedScene>(data.PackedScenePath);
            Node3D instance = packedScene.Instantiate<Node3D>();
			networkObject = instance.GetNode<NetworkObject>("NetworkObject");
			instance.Name = data.ParentName;
			justInstanced = true;
		}
		
		if(networkObject == null)
		{ networkObject = GameManager.instance.GetNodeOrNull<NetworkObject>(data.NodePath); } //if there is an error after here, it is probably because the node got moved / not the same path
		if(networkObject == null)
		{ networkObject = GameManager.instance.GetNodeOrNull<NetworkObject>(data.OldNodePath); } //if there is an error after here, it is probably because the node got moved / not the same path

        networkObject.Data = data;
		networkObject.IsOwner = owner;
		networkObject.Data.WasInstanced = justInstanced;
		
		if(justInstanced && data.InstanceParentPath != null)
		{ GameManager.instance.GetNode(data.InstanceParentPath).AddChild(networkObject.GetParent()); }

        if (data.PackedScenePath != null)
        { networkObject.Parent.Name = data.ParentName; }

		networkObject.Data.NodePath = networkObject.GetPath();

		GD.Print("Adding network object to scene + list : " + networkObject.Parent.Name);
		AddNetworkObjectToList(networkObject);

		return networkObject;
	}

    //adds a new network object to the list, making sure it arrives in the correct index
    private static void AddNetworkObjectToList(NetworkObject networkObject)
    {
        if (networkObject.Data.Id < NetworkObjects.Count)
        { return; }

        int amountToAdd = (networkObject.Data.Id.Value - NetworkObjects.Count) + 1;
        for (int i = 0; i < amountToAdd; i++) //free up enough slots so that the index lines up
        { NetworkObjects.Add(null); }

        NetworkObjects[networkObject.Data.Id.Value] = networkObject; //add into the correct index (Id == index in List)
    }

	//callback from the host when a new networkObject is registered (client)
    private static void OnRegisteredNetworkObjectCallback(Dictionary<string, string> data)
	{
		if(SteamManager.Instance.IsHost) //shouldn't be the host anyways, but just making sure
		{ return; }

		NetworkObjectData receivedData = JsonConvert.DeserializeObject<NetworkObjectData>(data["NetworkObjectData"]);
		ApplyNetworkObjectToScene(receivedData);
		GD.Print("Recieved Registered Callback id: " + receivedData.Id);
	}
	#endregion

	#region UpdateNetworkObjectData
	public void ChangeOwnerToHost()
	{ ChangeOwner(SteamManager.Instance.IsHost ? SteamManager.Instance.PlayerSteamId.ToString() : SteamManager.Instance.SteamConnectionManager.ConnectionInfo.Identity.SteamId.ToString()); }
	public void ChangeOwner(string newOwnerId)
	{
		GD.Print("Changing owner of Id: " + Data.Id + " to SteamId: " + newOwnerId);
		Data.OwnerId = newOwnerId;
		KeyValuePair<string, string> dataToAdd = new KeyValuePair<string, string>("NewOwnerId", newOwnerId);
		SendNetworkObjectData(dataToAdd);
	}

	public void UpdateParent()
	{
		if(!IsInsideTree())
		{ GD.PrintErr("Trying to ChangeParent() while outside of the tree"); return; }

		Data.NodePath = GetPath();
		Data.InstanceParentPath = Parent.GetParent().GetPath();
		KeyValuePair<string, string> dataToAdd = new KeyValuePair<string, string>("NewParentPath", Parent.GetParent().GetPath());

		SendNetworkObjectData(dataToAdd);
	}

	private async void SendNetworkObjectData(KeyValuePair<string, string> dataToAdd)
	{
		await Task.Run(async () => { while (!Data.Id.HasValue) await Task.Delay(10); });

		//Not sending the entire NetworkObjectData struct because there are issues with two calls overwriting each other's values
		Dictionary<string, string> data = new Dictionary<string, string>();
		data.Add("DataType", "UpdateNetworkObjectData");
		data.Add("Id", Data.Id.Value.ToString());
		Data.SyncData = new NetworkObjectSyncData();
		Data.LastSyncData = new NetworkObjectSyncData();
		Data.SyncData.ParentRotation = Data.LastSyncData.ParentRotation = Parent.Rotation;
		Data.SyncData.ParentPosition = Data.LastSyncData.ParentPosition = Parent.Position;
		data.Add("SyncData", JsonConvert.SerializeObject(Data.SyncData));
		data.Add(dataToAdd.Key, dataToAdd.Value);
		NetworkDataManager.SendMessage(data);
	}

	private static void OnUpdateNetworkObjectDataCallback(Dictionary<string, string> data)
	{
		NetworkObject networkObject = GetNetworkObjectFromList(int.Parse(data["Id"]));
		if(!networkObject.Data.Id.HasValue)
		{ GD.PrintErr("Updating NetworkObjectData with no Id"); return; }
		GD.Print("(Callback) Updating NetworkObjectData on Id: " + networkObject.Data.Id.Value);
        
		if(data.ContainsKey("NewParentPath") && networkObject.Data.OwnerId != SteamManager.Instance.PlayerSteamId.ToString())
		{
			GD.Print("    -NewParentPath: " + data["NewParentPath"]);
			networkObject.Parent.GetParent().RemoveChild(networkObject.Parent);
			Node newParent = GameManager.instance.GetNode(data["NewParentPath"]);
			newParent.AddChild(networkObject.Parent);
			networkObject.Data.InstanceParentPath = data["NewParentPath"];
		}
		if(data.ContainsKey("NewOwnerId"))
		{
            GD.Print("    -NewOwnerId: " + data["NewOwnerId"]);
			networkObject.Data.OwnerId = data["NewOwnerId"];
			networkObject.RecalculateOwner();
			networkObject.Data.SyncData = networkObject.Data.LastSyncData = JsonConvert.DeserializeObject<NetworkObjectSyncData>(data["SyncData"]);
			networkObject.Parent.Position = networkObject.Data.SyncData.ParentPosition;
			networkObject.Parent.Rotation = networkObject.Data.SyncData.ParentRotation;
		}
	}
	#endregion
}
