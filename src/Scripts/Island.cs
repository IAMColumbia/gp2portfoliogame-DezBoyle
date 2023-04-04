using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Island : Node3D
{
	[Export] private Node connectionPointsNode;
	[Export] private Node boundsNode;
	[Export] private NodePath playerSpawnPosNodePath;

	public List<Node3D> ConnectionPoints;
	public Node3D PlayerSpawnPos { get; private set; }
	public CollisionShape3D[] Bounds { get; private set; }
    public Aabb BoxBounds { get { return boxAabb * GlobalTransform; } } //needs to factor in island pos/rot too
	public Aabb boxAabb { get; private set; }

	public override void _Ready()
	{
		PlayerSpawnPos = GetNode<Node3D>(playerSpawnPosNodePath);
		ConnectionPoints = connectionPointsNode.GetChildren().Cast<Node3D>().ToList();
		Bounds = boundsNode.GetChildren().Cast<CollisionShape3D>().ToArray();
		boxAabb = new Aabb();
		foreach(CollisionShape3D shape in Bounds)
		{
			ArrayMesh arrayMesh = shape.Shape.GetDebugMesh(); //this returns 0,0,0,0 - doesnt work
			boxAabb.Merge(arrayMesh.GetAabb());
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
}
