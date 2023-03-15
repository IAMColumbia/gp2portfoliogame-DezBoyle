using Godot;
using System;

public partial class OrbitCamera : Node3D
{
	// public bool Enabled { get; set;} = false;
	// [Export] private float sensitivity = 2.5f;
	// [Export] private NodePath cameraPosPath;
	// [Export] private float startDistance = 1f;
	// [Export] private Vector2 startRotation;
	// [Export] private bool limitX = false;
	// [Export] private float xMin = -90f;
	// [Export] private float xMax = -90f;
	// [Export] private float yMin = -30f;
	// [Export] private float yMax = 30f;
	// [Export] private float minZoom = .4f;
	// [Export] private float maxZoom = 10f;
	// [Export] private float zoomLerpSpeed = 10f;
	// private Node3D camPos;
	// private Node3D actualCamPos;
	// private Vector2 mouseInput;
	// private Vector2 mousePosition;
	// private Vector2 lastMousePosition;
	// private Vector3 wishRotation;
    // private const float HalfPI = Mathf.Pi / 2.1f;
	// private Vector3 targetPos;

	// //need to rotate this to match up (within min/max)with the player cam so that the lerp is epicly smooth (get player reference from Interaction method)

	// //need to cast a ray to make sure the camera adjusts distance and doesnt go thru walls

	// public override void _Ready()
	// {
	// 	camPos = new Node3D();
	// 	AddChild(camPos);
	// 	targetPos = new Vector3(0, 0, startDistance);
	// 	camPos.Position = targetPos;
	// 	actualCamPos = GetNode<Node3D>(cameraPosPath);
	// }

	// public void StartOrbit(Player player)
	// {
	// 	//we got the player from when we clicked
	// 	//use this to rotate the pivot thingy to face the same direction or whatever
	// 	Enabled = true;
	// }

	// //also need to make sure Enabled gets set to FALSE

	// public override void _Process(double delta)
	// {
	// 	if(!Enabled)
	// 	{ return; }

	// 	if(Input.IsActionPressed("orbitCamera"))
	// 	{
	// 		 if(!limitX)
	// 		 { RotateY(-mouseInput.x); }

	// 		wishRotation = Rotation;
	// 		wishRotation.x = Mathf.Clamp((-mouseInput.y + wishRotation.x), -HalfPI * (yMin / -90), HalfPI * (yMax / 90));
	// 		Rotation = wishRotation;
	// 	}

	// 	mouseInput = Vector2.Zero;
	// 	mouseInput.x += Input.IsActionPressed("right") ? -1f : 0f;
	// 	mouseInput.x += Input.IsActionPressed("left") ? 1f : 0f;
	// 	mouseInput.y += Input.IsActionPressed("forward") ? 1f : 0f;
	// 	mouseInput.y += Input.IsActionPressed("backward") ? -1f : 0f;
	// 	mouseInput *= (float)delta;
	// 	wishRotation = Rotation;
	// 	wishRotation.x = Mathf.Clamp((-mouseInput.y + wishRotation.x), -HalfPI * (yMin / -90), HalfPI * (yMax / 90));
	// 	Rotation = wishRotation;
	// 	RotateY(-mouseInput.x);

	// 	float distanceMult = camPos.Position.z / maxZoom;
	// 	targetPos.z += Input.IsActionJustReleased("zoomOut") ? distanceMult : 0f;
	// 	targetPos.z += Input.IsActionJustReleased("zoomIn") ? -distanceMult : 0f;
	// 	targetPos.z = Mathf.Clamp(targetPos.z, minZoom, maxZoom);

	// 	camPos.Position = camPos.Position.Lerp(targetPos, (float)delta * zoomLerpSpeed);

	// 	actualCamPos.GlobalPosition = camPos.GlobalPosition;
	// 	actualCamPos.GlobalRotation = camPos.GlobalRotation;
	// }

	// public override void _Input(InputEvent @input)
    // {
	// 	if(!Enabled)
    //     { return; }

    //     if (@input is InputEventMouseMotion)
    //     {
    //         mousePosition = ((InputEventMouseMotion)@input).Position;
	// 		mouseInput = mousePosition - lastMousePosition;
	// 		mouseInput /= 256;
	// 		lastMousePosition = mousePosition;
    //     }
    // }
}
