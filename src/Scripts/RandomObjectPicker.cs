using Godot;
using System;
using System.Collections.Generic;

public partial class RandomObjectPicker : Node3D
{
    [Export] public bool IgnoreInRandomPick = false; //this object will be ignored if it's parent is another RandomObjectPicker
	[Export] private float chanceToDelete = 0.25f;
	[Export] private bool pickOne = true;

    private List<Node3D> children = new List<Node3D>();
    private short frameTimer;
    
    public override void _Process(double delta)
    {
        frameTimer++;
        if(frameTimer > 1)
        {
            SetProcess(false);
            Randomize();
        }
    }

	public void Randomize()
	{
		foreach(Node3D obj in GetChildren())
		{
            if(obj is RandomObjectPicker && ((RandomObjectPicker)obj).IgnoreInRandomPick)
            { continue; } //ugly solution
            children.Add(obj);
            obj.Visible = !pickOne;
        }

		if(pickOne)
		{ RandomPickOne(); }
		else
		{ RandomPick(); }
       
        foreach(Node3D n in children)
        {
            if(!n.Visible)
            {
                n.QueueFree();
            }
        }
	}

	private void RandomPick()
    {
        int childCount = children.Count;
        int childrenDeleted = 0;
        foreach (Node3D n in children)
        {
            float deleteRNG = Utility.RandomRange(0f, 1f);
            if (deleteRNG <= chanceToDelete)
            {
                childrenDeleted++;
                n.Visible = false;
                n.QueueFree();
            }
        }
        if (childrenDeleted >= childCount)
        {
            //all children were deleted, also delete the root object
            Visible = false;
            QueueFree();
        }
    }

    private void RandomPickOne()
    {
        float deleteRNG = Utility.RandomRange(0f, 1f);
        if (deleteRNG < chanceToDelete)
        {
			Visible = false;
            QueueFree();
            return;
        }

        int objectRNG = Utility.RandomRange(0, children.Count);
        children[objectRNG].Visible = true;
    }
}
