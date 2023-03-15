using Godot;
using System;

public partial class PlayerInventory : Node
{
    public Item HeldItem { get; private set; }
    public Node3D HeldObject { get; private set; }
	
	private Player player;


	public void Init(Player p)
	{
		player = p;
	}
	
	public override void _Process(double delta)
	{
		if(HeldItem == null)
		{ return; }

		if(Input.IsActionJustPressed("drop"))
		{
			//GameManager.instance.DeleteObject(HeldItem.NetworkObject);
			Node3D droppedItem = GameManager.instance.SpawnObject(HeldItem.PickupPackedScene);
			droppedItem.GlobalPosition = player.DroppedItemPosition.GlobalPosition;
			((InteractablePickup)droppedItem).Float = true;
			HeldItem.QueueFree();
			HeldItem = null;
			HeldObject = null;
			return;
		}

		HeldItem.Clicking = Input.IsActionPressed("attack");
		HeldItem.JustClicked = Input.IsActionJustPressed("attack");
	}

	public void Pickup(PackedScene itemScene)
	{
		if(HeldObject != null)
		{ return; } //TODO: swap item instead
		
		HeldObject = GameManager.instance.SpawnObject(itemScene, player.Cam);
		HeldObject.Position = Vector3.Zero;
		HeldObject.Rotation = Vector3.Zero;

		HeldItem = (Item)HeldObject;
		HeldItem.Init(player);
	}
}