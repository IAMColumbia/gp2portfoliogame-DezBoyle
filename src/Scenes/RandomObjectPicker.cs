using Godot;
using System;

public partial class RandomObjectPicker : Node3D
{
	[Export] private float chanceToDelete = 0.25f;
	[Export] private bool pickOne = true;

	public override void _Ready()
	{
		foreach(Node3D obj in GetChildren())
		{ obj.Visible = !pickOne; }

		if(pickOne)
		{
			float deleteRNG = Utility.RandomRange(0f, 1f);
            if (deleteRNG >= chanceToDelete)
            {
                int objectRNG = Utility.RandomRange(0, GetChildCount());
                GetChild<Node3D>(objectRNG).Visible = true;
				GD.Print("turning on " + objectRNG);
            }
		}
		else
		{
			foreach(Node3D obj in GetChildren())
			{
                float deleteRNG = Utility.RandomRange(0f, 1f);
                if (deleteRNG <= chanceToDelete)
                { obj.Visible = false; }
			}
		}
		
		foreach(Node3D obj in GetChildren())
        {
            if (!obj.Visible)
			{ obj.QueueFree(); }
			//delete invisible objects
        }
	}
}
