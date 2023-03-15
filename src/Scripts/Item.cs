using Godot;
using System;
using System.Collections.Generic;

public partial class Item : Node3D
{
	[Export] private string pickupScenePath = "res://Objects/Items/Item_CrystalPistol";
	[Export] public Texture2D IconTexture;
	[Export] public string ItemName;

	public PackedScene PackedScene { get; private set; } = null;
    public PackedScene PickupPackedScene { get; private set; }
	public NetworkObject NetworkObject { get; private set; }
	public bool Clicking { get; set; }
	public bool JustClicked { get; set; }
	public Player Player { get; private set; }

	public event Action OnFellOffMap;

	private const float yMinimum = -256f;
	private bool belowMinimum = false;
	

	public override void _Ready()
	{
		base._Ready();
		
		PackedScene = GD.Load<PackedScene>(SceneFilePath);
		PickupPackedScene = GD.Load<PackedScene>(pickupScenePath);
		
		if(!GameManager.instance.Singleplayer)
		{ NetworkObject = Utility.GetAllNodesOfType<NetworkObject>(Utility.GetAllChildren(this))[0]; }
		//idk how bad this is for performance ^ 
	}

	public virtual void Init(Player p)
	{
		Player = p;
	}

	public override void _PhysicsProcess(double delta)
	{
		if(belowMinimum)
		{
			if(GetObject().GlobalPosition.Y <= yMinimum)
			{ belowMinimum = false; GD.Print("BelowMinimum = false"); }
			return;
		}

		belowMinimum = GetObject().GlobalPosition.Y < yMinimum;
		if(belowMinimum)
		{ FellOffTheMap(); GD.Print("BelowMinimum = true"); }
	}

	private void FellOffTheMap()
	{
		OnFellOffMap?.Invoke();
		Reset();
		GD.Print("Item fall off the map.  Resetting...");
	}

	public virtual void Reset() { }

	public virtual Node GetDroppedParent()
	{ return GameManager.instance.Level; }

	protected virtual Node3D GetObject()
	{ return this; }
}
