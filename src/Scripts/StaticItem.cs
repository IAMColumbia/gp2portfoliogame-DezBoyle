using Godot;
using System;

public partial class StaticItem : Item
{
	// [Export] private float yOffset = 0f;
	// [Export] private float bobAmt = 0.2f;
	// private Vector3 dropVelocity;
	// private bool falling = false;
	// private Vector3? groundPos = null;
	// private Vector3? lastGroundPos = null;
	// private PhysicsRayQueryParameters3D rayParams;
    // private Godot.Collections.Dictionary hit;
	// private Node lastNodeHit;
	// private Vector3? landPosition = null;
	

	// public override void _Ready()
	// {
	// 	base._Ready();

	// 	falling = false;
	// 	Godot.Collections.Array<RID> exclude = new Godot.Collections.Array<RID>();
    //     exclude.Add(new RID(this as Godot.Object));
    //     rayParams = PhysicsRayQueryParameters3D.Create(Vector3.Zero, Vector3.Zero, 1, exclude);
    //     rayParams.CollideWithAreas = false;
	// 	landPosition = null;
	// }

	// public override void Reset()
	// {
	// 	if(!GameManager.instance.Singleplayer && !NetworkObject.IsOwner)
	// 	{ return; }

	// 	falling = false;

	// 	base.Reset();
	// }

	// public override void _Process(double delta)
	// {
	// 	base._Process(delta);

	// 	if(!GameManager.instance.Singleplayer && !NetworkObject.IsOwner)
	// 	{ return; }

	// 	if(!falling)
	// 	{
	// 		if(landPosition.HasValue)
	// 		{
	// 			float yTarget = landPosition.Value.y + (bobAmt * Mathf.Sin(Time.GetTicksMsec() / 1000f));
	// 			float y = landPosition.Value.y + yTarget;
	// 			// float y = Mathf.Lerp(GlobalPosition.y, (landPosition.Value.y + yTarget), (float)delta);
	// 			//shits dumb as hell idk why lerping it is all busted
    //             GlobalPosition = GlobalPosition.Lerp(new Vector3(landPosition.Value.x, y + 0.4f, landPosition.Value.z), (float)delta * 2f);
    //             RotateY((float)delta);
    //         }
	// 		return;
	// 	}
		
	// 	dropVelocity.y -= (float)delta * 19.6f;
	// 	//Translate(dropVelocity * (float)delta);
	// 	GlobalPosition += dropVelocity * (float)delta;

	// 	rayParams.From = GlobalPosition;
    //     rayParams.To = GlobalPosition + Vector3.Down * 1f;
    //     hit = GetWorld3d().DirectSpaceState.IntersectRay(rayParams);

	// 	if(hit != null && hit.Count > 0)
	// 	{
	// 		groundPos = (Vector3)hit["position"];

	// 		if(lastNodeHit == null)
	// 		{ lastNodeHit = (Node)hit["collider"]; }

	// 		if(lastNodeHit != (Node)hit["collider"]) //we went through the ground and are detecting another object
	// 		{ falling = false; }

	// 		lastNodeHit = (Node)hit["collider"];
	// 	}

	// 	if(!falling || groundPos.HasValue && GlobalPosition.y < groundPos.Value.y + yOffset)
	// 	{
	// 		if(!falling && lastGroundPos.HasValue)
	// 		{ GlobalPosition = lastGroundPos.Value; } //if we clipped through the ground, use the lastGroundPos 
	// 		else 
	// 		{ GlobalPosition = groundPos.Value + Vector3.Up * yOffset; } 
			
	// 		Pickup.Enabled = true;
	// 		falling = false;
	// 		landPosition = GlobalPosition;

	// 		if(!GameManager.instance.Singleplayer)
	// 		{
	// 			NetworkObject.UpdateParent();
	// 			NetworkObject.ChangeOwnerToHost();
	// 		}
	// 	}

	// 	if(groundPos.HasValue)
	// 	{ lastGroundPos = groundPos.Value; }
	// }

	// public override void Drop(Node3D obj, Vector3 velocity, Node newParent)
	// {
	// 	base.Drop(obj, velocity, newParent);

	// 	falling = true;
	// 	groundPos = null;
	// 	lastGroundPos = null;
	// 	dropVelocity = velocity;
	// }
}
