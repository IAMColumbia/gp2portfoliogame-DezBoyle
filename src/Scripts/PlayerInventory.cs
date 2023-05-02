using Godot;
using System;

public partial class PlayerInventory : Node
{
    public Item EquippedItem { get; private set; }
	
	private Player player;
	private Weapon weapon;

	public void Init(Player p)
	{
		player = p;
	}

	public override void _Process(double delta)
	{
		if(EquippedItem == null)
		{ return; }

		if(Input.IsActionJustPressed("drop"))
		{
			Item itemToDrop = EquippedItem;
			if(weapon != null)
            { itemToDrop = weapon.RemoveWeaponMod(); }
			if(itemToDrop == null)
            { itemToDrop = EquippedItem; }

			Drop(itemToDrop);
			return;
		}
		
		EquippedItem.Clicking = Input.IsActionPressed("attack");
		EquippedItem.JustClicked = Input.IsActionJustPressed("attack");
	}

    private void Drop() { Drop(EquippedItem); }
	private void Drop(Item itemToDrop)
	{
		if(itemToDrop == null)
		{ GD.PrintErr("tried to drop a null Item"); return; }
        
		//GameManager.instance.DeleteObject(HeldItem.NetworkObject);
        Node3D droppedItem = GameManager.instance.SpawnObject(itemToDrop.PickupPackedScene);
        droppedItem.GlobalPosition = player.DroppedItemPosition.GlobalPosition;
        ((InteractablePickup)droppedItem).Float = true;
        
		if(itemToDrop == EquippedItem)
		{ EquippedItem = null; weapon = null; }

		itemToDrop.QueueFree();
        itemToDrop = null;
	}

	public void Pickup(PackedScene itemScene)
	{
		Item item = (Item)GameManager.instance.SpawnObject(itemScene, player.Cam);
		item.Position = Vector3.Zero;
		item.Rotation = Vector3.Zero;

		WeaponMod weaponMod = item as WeaponMod;

		if(weapon != null && weaponMod != null)
		{ //we have a weaponMod and a weapon
			weapon.AttachWeaponMod(weaponMod);
			return;
		}

		//TODO: if EquippedItem is a mod and we pick up a weapon, equip the weapon and apply the mod

        if(EquippedItem != null)
		{ Drop(); } //if its not a mod and we already have a weapon, swap

        // weaponMod = EquippedItem as WeaponMod;

        EquippedItem = (Item)item;
        EquippedItem.Init(player);
        weapon = EquippedItem as Weapon;

		// if(weapon != null && weaponMod != null)
		// { weapon.AttachWeaponMod(weaponMod); } //we were holding a weaponMod before we picked up a weapon
	}
}