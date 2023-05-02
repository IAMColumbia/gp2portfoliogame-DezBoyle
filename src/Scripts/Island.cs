using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Island : Node3D
{
	[Export] private Node connectionPointsNode;
	[Export] private Node boundsNode;
	[Export] private NodePath playerSpawnPosNodePath;
    [Export] private Node spawnPositionsNode;

	public List<Node3D> ConnectionPoints;
    public List<SpawnPosition> SpawnPositions;
	public Node3D PlayerSpawnPos { get; private set; }
	public CollisionShape3D[] Bounds { get; private set; }
    public Aabb BoxBounds { get { return BoxAabb * GlobalTransform; } } //needs to factor in island pos/rot too
	public Aabb BoxAabb { get; private set; }

	public override void _Ready()
	{
		PlayerSpawnPos = GetNode<Node3D>(playerSpawnPosNodePath);
		ConnectionPoints = connectionPointsNode.GetChildren().Cast<Node3D>().ToList();
		Bounds = boundsNode.GetChildren().Cast<CollisionShape3D>().ToArray();
		if(spawnPositionsNode != null && spawnPositionsNode.GetChildCount() > 0)
		{ SpawnPositions = spawnPositionsNode.GetChildren().Cast<SpawnPosition>().ToList(); }
		BoxAabb = new Aabb();
		foreach(CollisionShape3D shape in Bounds)
		{
			ArrayMesh arrayMesh = shape.Shape.GetDebugMesh(); //this returns 0,0,0,0 - doesnt work
			BoxAabb.Merge(arrayMesh.GetAabb());
		}
	}

	public void DebugShowConnectionPoints()
	{
		foreach(Node n in ConnectionPoints)
		{
			CsgBox3D visual = new CsgBox3D();
			visual.Size = new Vector3(.25f, 3f, .25f);
			n.AddChild(visual);
			visual.Position = Vector3.Zero;
		}
	}

	public void TurnIntoPlaceholder()
	{
		PlayerSpawnPos = GetNode<Node3D>(playerSpawnPosNodePath); //copy paste bad
        foreach (Node child in GetChildren())
        {
            if (child != boundsNode && child != connectionPointsNode && child != PlayerSpawnPos)
            { child.QueueFree(); }
		}
	}
}
