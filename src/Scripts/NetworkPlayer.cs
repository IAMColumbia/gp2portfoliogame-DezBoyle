using Godot;
using Steamworks;
using System;
using System.Collections.Generic;

public partial class NetworkPlayer : Node
{
    public string PlayerName { get; private set; }
    public bool IsLocalPlayer { get; private set; } = true;

    private bool initialized = false;
    private Player player;
    private Label3D nameLabel;
    private Vector3 networkPosition;
    private Vector3 networkRotation;
    private Vector3 lastNetworkPosition;
    private float lastTimeSentPosition = -420f;
    private float lastTimeReceivedPosition = -420f;
    private const float transmitDelay = 0.025f;

    public void Init(Player p)
    {
        player = p;
        IsLocalPlayer = GetParent().Name == SteamManager.Instance.PlayerSteamId.ToString();
        if(GameManager.instance.Singleplayer)
        { IsLocalPlayer = true; GD.Print("Singleplayer is true. Setting IsLocalPlayer = true"); }

        NetworkDataManager.OnPlayerUpdate += OnPlayerUpdateCallback;
        NetworkDataManager.OnFart += OnFartCallback;
    }

    public override void _Ready()
    {
        base._Ready();
        initialized = true;
    }

    public void SetNameLabel(string name)
    {
        if(nameLabel == null)
        { nameLabel = GetNode<Label3D>("../NameLabel"); }
        nameLabel.Text = name;
        PlayerName = name;
    }

    public void SetNameLabelVisible(bool visible)
    {
        SetNameLabel(""); //just in case nameLabel is null
        nameLabel.Visible = visible;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(!initialized)
        { return; }
        base._PhysicsProcess(delta);
        if(GameManager.instance.Singleplayer)
        { return; }

        if(IsLocalPlayer)
        {
            if(Time.GetTicksMsec() - lastTimeSentPosition >= transmitDelay * 1000)
            {
                lastTimeSentPosition = Time.GetTicksMsec();
                UpdateNetworkPosition();
            }
            if(Input.IsActionJustPressed("fart"))
            {
                GetNode<AudioStreamPlayer3D>("../AudioPlayer").Play();
                Dictionary<string, string> data = new Dictionary<string, string>()
                {
                    {"DataType", "Fart"},
                    {"PlayerId", SteamManager.Instance.PlayerSteamId.ToString()}
                };
                NetworkDataManager.SendMessage(data, Steamworks.Data.SendType.Unreliable);
            }
        }
        else
        {
            float t = (Time.GetTicksMsec() - lastTimeReceivedPosition) / (transmitDelay * 1000f);
            t = Mathf.Clamp(t, 0, 1);
            player.GlobalPosition = lastNetworkPosition.Lerp(networkPosition, t);
            player.PlayerBody.Rotation = networkRotation;
        }
    }

    private void UpdateNetworkPosition()
    {
        Dictionary<string, string> data = new Dictionary<string, string>()
        {
            {"DataType", "UpdatePlayer"},
            {"PlayerId", SteamManager.Instance.PlayerSteamId.ToString()},
            {"PositionX", player.GlobalPosition.X.ToString()}, //later this could be just serialized with JsonConvert<Vector3>
            {"PositionY", player.GlobalPosition.Y.ToString()},
            {"PositionZ", player.GlobalPosition.Z.ToString()},
            {"PlayerBodyRotationY", player.PlayerBody.Rotation.Y.ToString()}
        };
        NetworkDataManager.SendMessage(data, Steamworks.Data.SendType.Unreliable);
    }

    private bool HasCorrectId(Dictionary<string, string> data)
    {
        if(data["PlayerId"] == SteamManager.Instance.PlayerSteamId.ToString())
        { return false; }
        if(!IsInstanceValid(this) || data["PlayerId"] != GetParent().Name)
        { return false; }
        return true;
    }

    private void OnPlayerUpdateCallback(Dictionary<string, string> data)
    {
        if(!HasCorrectId(data))
        { return; }
        
        lastNetworkPosition = networkPosition;
        networkPosition = new Vector3(float.Parse(data["PositionX"]), float.Parse(data["PositionY"]), float.Parse(data["PositionZ"]));
        if(lastNetworkPosition == null)
        { lastNetworkPosition = networkPosition; }
        lastTimeReceivedPosition = Time.GetTicksMsec();

        //for some reason, I cant access player here if the client joins, quits, then joins again.  Saying an error about accessing a disposed object
        networkRotation = new Vector3(0f, float.Parse(data["PlayerBodyRotationY"]), 0f);
    }

    private void OnFartCallback(Dictionary<string, string> data)
    {
        if(!HasCorrectId(data))
        { return; }

        GetNode<AudioStreamPlayer3D>("../AudioPlayer").Play();
    }
}
