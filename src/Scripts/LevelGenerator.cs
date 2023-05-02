using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class LevelGenerator : Node3D
{
    //NOT USED YET
    public enum Rarity 
    {
        VeryRare = 1,
        Rare = 2,
        Uncommon = 3,
        Common = 5
    }

    [Export] private bool debug = false;
    [Export] private NodePath startingIslandNodePath;
    [Export] private PackedScene[] islandScenes;
    [Export] private PackedScene[] objectScenes;
    [Export] private int islandsMin = 24;
    [Export] private int islandsMax = 32;
    [Export] private int branches = 2;
    [Export] private int islandsUntilBranchMin = 10;
    [Export] private int islandsUntilBranchMax = 25;
    [Export] private float verticalRange = .75f;

    public Node3D CurrentLevel { get; set; }

    private List<Island> spawnedIslands = new List<Island>();
    private Island[] placeholderIslands; //instantiate placeholders instead of actual islands to speed up generation
    private Island startingIsland;

    //stores the SpawnType of each object PackedScene (to know what SpawnPosition.Type to match them up with)
    private Dictionary<PackedScene, SpawnPosition.SpawnType> PackedSceneToSpawnType = new Dictionary<PackedScene, SpawnPosition.SpawnType>();
    
    public override void _Ready()
    {
        startingIsland = GetNode<Island>(startingIslandNodePath);

        //set up placeholder islands
        placeholderIslands = new Island[islandScenes.Length];
        for(int i = 0; i < placeholderIslands.Length; i++)
        {
            Island island = islandScenes[i].Instantiate<Island>();
            island.TurnIntoPlaceholder();
            AddChild(island);
            placeholderIslands[i] = island;
        }

        //get all of the spawntypes for the islandObjects
        foreach(PackedScene packedScene in objectScenes)
        {
            IslandObject islandObject = (IslandObject)packedScene.Instantiate();
            PackedSceneToSpawnType.Add(packedScene, islandObject.SpawnType);
            islandObject.QueueFree();
        }

        if(!debug)
        { NewLevel(); }
    }

    public override void _Process(double delta)
    {
        if (Input.IsKeyPressed(Key.P))
        {
            NewLevel();
        }
    }

    public void NewLevel()
    {
        Cleanup();
        GenerateIslands(Utility.RandomRange(islandsMin, islandsMax), GlobalPosition, branches);
    }

    //STEP 1.5
    private bool ConnectIsland(int islandsLeft, Island prevIsland, int branchesLeft, int islandsUntilBranch, Stack<Island> prevIslandStack = null)
    {

        if (prevIslandStack == null)
        { prevIslandStack = new Stack<Island>(); }

        if (prevIsland.ConnectionPoints.Count == 0)
        { GD.Print("generation ended early.  Out of connectionpoints"); return false; }
        Node3D connectionPointPrev = prevIsland.ConnectionPoints[Utility.RandomRange(0, prevIsland.ConnectionPoints.Count)];

        //the directionVector is the direction of the next island
        Vector3 directionVector = connectionPointPrev.GlobalPosition - prevIsland.GlobalPosition;
        directionVector = directionVector.Normalized();
        directionVector *= 20f; //extend it really far temporarily
        Vector3 pos = connectionPointPrev.GlobalPosition + directionVector;

        //spawn the island
        Island island = islandScenes[Utility.RandomRange(0, islandScenes.Length)].Instantiate<Island>();
        // Island island = (Island)placeholderIslands[Utility.RandomRange(0, placeholderIslands.Length)].Duplicate((int)DuplicateFlags.Scripts);
        CurrentLevel.AddChild(island);
        if (debug)
        { island.DebugShowConnectionPoints(); }
        island.GlobalPosition = pos;

        //get new connection point
        Node3D connectionPoint = island.ConnectionPoints[Utility.RandomRange(0, island.ConnectionPoints.Count)];

        //rotate the island until we get the desired rotation (connected points are the closest)
        Vector3 rot = island.Rotation;
        float sqrMinDistance = float.MaxValue;
        const int rotationLoops = 32;
        const float radiansPerLoop = (Mathf.Pi * 2) / rotationLoops;
        for (int i = 0; i < rotationLoops; i++)
        {
            island.RotateY(radiansPerLoop);
            float sqrDistance = connectionPoint.GlobalPosition.DistanceSquaredTo(connectionPointPrev.GlobalPosition);
            //find the minimum distance between the two points to get the best rotation of the island.
            if (sqrDistance < sqrMinDistance)
            {
                sqrMinDistance = sqrDistance;
                rot = island.Rotation;
            }
        }

        island.Rotation = rot; //apply the new rotation

        //get the island to the correct position
        island.GlobalPosition = connectionPointPrev.GlobalPosition;
        island.Translate(-connectionPoint.Position); //offset
        island.Translate(Vector3.Down * verticalRange * Utility.RandomRange(-1f, 1f)); //vertical bits

        //check if they collide
        //await ToSignal(GetTree(), "physics_frame"); //this doesnt seem to work

        bool collision = false;

        foreach (Island otherIsland in spawnedIslands)
        {
            //if(!otherIsland.BoxBounds.Intersects(island.BoxBounds))
            //{ continue; } //not close enough to test for collisions

            //GD.Print("inside bounds");

            SphereShape3D sphereX, sphereY;
            foreach (CollisionShape3D shapeX in otherIsland.Bounds)
            {
                foreach (CollisionShape3D shapeY in island.Bounds)
                {
                    //test collisions between shapes X and Y
                    //only doing spheres for now
                    sphereX = shapeX.Shape as SphereShape3D;
                    sphereY = shapeY.Shape as SphereShape3D;
                    float sqrRadiusSum = (sphereX.Radius * sphereX.Radius) + (sphereY.Radius * sphereY.Radius);
                    if (shapeX.GlobalPosition.DistanceSquaredTo(shapeY.GlobalPosition) <= sqrRadiusSum)
                    {
                        collision = true;
                    }
                    // float radiusSum = sphereX.Radius + sphereY.Radius;
                    // if (shapeX.GlobalPosition.DistanceTo(shapeY.GlobalPosition) <= radiusSum)
                    // {
                    //     collision = true;
                    // }
                }
            }
        }

        if (collision)
        {
            GD.Print("There was a collision");
            island.Free();

            if (prevIslandStack.Count == 0)
            {
                GD.Print("end of stack.  starting generation back at the source");
                prevIsland.ConnectionPoints.Remove(connectionPointPrev);

                if (prevIsland.ConnectionPoints.Count <= 0)
                { GD.Print("out of connection points on first island.  Ending early"); return false; }
                else
                { return ConnectIsland(islandsLeft + 1, prevIsland, branchesLeft, islandsUntilBranch); } //try another connection point
            }

            if(prevIsland.ConnectionPoints.Count > 0)
            { return ConnectIsland(islandsLeft + 1, prevIsland, branchesLeft, islandsUntilBranch); } //try another connection point
            else
            { return ConnectIsland(islandsLeft + 1, prevIslandStack.Pop(), branchesLeft, islandsUntilBranch); } //go back a step
        }

        //the connection point has already been used.  remove it so the next island doesnt go backwards
        island.ConnectionPoints.Remove(connectionPoint);
        spawnedIslands.Add(island);

        //branching
        islandsUntilBranch--;
        if (branchesLeft > 0 && (islandsUntilBranch <= 0))
        {
            //split the islandsLeft among the branches
            if(branchesLeft > 0)
            { islandsUntilBranch = Utility.RandomRange(islandsUntilBranchMin, islandsUntilBranchMax); }
            int biggerHalf = islandsLeft / 2;
            int smallerHalf = islandsLeft / 2;
            biggerHalf += islandsLeft % 2; //add the remainder to the bigger half
            ConnectIsland(smallerHalf, island, 0, 0); //only the main branch can make new branches (FOR NOW).  I could split it like i do with the branch amounts tho <-----
            islandsLeft = biggerHalf;
            branchesLeft--;
        }

        GD.Print("New Island");

        islandsLeft--;
        if (islandsLeft <= 0)
        { return true; }

        if (prevIsland != null)
        { prevIslandStack.Push(prevIsland); }

        return ConnectIsland(islandsLeft, island, branchesLeft, islandsUntilBranch, prevIslandStack);
    }

    //STEP 1:
    private void GenerateIslands(int islands, Vector3 position, int branchesLeft)
    {
        bool success = false;
        while (!success)
        {
            // Island startingIsland = islandScenes[0].Instantiate<Island>();
            // AddChild(startingIsland);
            CurrentLevel = new Node3D();
            AddChild(CurrentLevel);
            Island temporaryStartingIsland = (Island)GetNode<Island>(startingIslandNodePath).Duplicate((int)DuplicateFlags.Scripts);
            CurrentLevel.AddChild(temporaryStartingIsland);
            spawnedIslands.Add(temporaryStartingIsland);
            if (debug)
            { temporaryStartingIsland.DebugShowConnectionPoints(); }
            temporaryStartingIsland.GlobalPosition = position;

            success = ConnectIsland(islands, temporaryStartingIsland, branchesLeft, Utility.RandomRange(islandsUntilBranchMin, islandsUntilBranchMax));

            //Delete the duplicated startingIsland (just for checking collisions)
            spawnedIslands[0].QueueFree();
            spawnedIslands.RemoveAt(0);
            if (!success)
            { Cleanup(); }
        }
        
        GD.Print("STEP 1: GENERATING ISLANDS COMPLETE");
        
        GenerateObjects();
    }

    //STEP 2:
    private void GenerateObjects()
    {
        //Next step : fill in all the items
        List<SpawnPosition> spawnPositions;
        Island island;

        //spawn in the orb at the farthest possible island
        
        Island farthestIsland = null;
        float sqrFarthestDistance = 0f;
        for(int i = spawnedIslands.Count - 1; i >= 0; i--)
        {
            island = spawnedIslands[i];
            spawnPositions = island.SpawnPositions;

            if(spawnPositions == null)
            { continue; }

            foreach(SpawnPosition spawn in spawnPositions)
            {
                if(spawn.Type != SpawnPosition.SpawnType.Orb)
                { continue; }

                float sqrDistance = island.GlobalPosition.DistanceSquaredTo(startingIsland.GlobalPosition);
                if(sqrDistance > sqrFarthestDistance)
                {
                    sqrFarthestDistance = sqrDistance;
                    farthestIsland = island;
                }
                
                break;
            }
        }

        if (farthestIsland == null)
        { GD.PrintErr("No Orb Spawn Found"); return; }

        //spawn an orb
        GD.Print("Spawning Orb");
        Node3D orb = (Node3D)GetRandomObjectOfType(SpawnPosition.SpawnType.Orb).Instantiate();
        farthestIsland.AddChild(orb);
        foreach (SpawnPosition spawn in farthestIsland.SpawnPositions)
        {
            if (spawn.Type != SpawnPosition.SpawnType.Orb)
            { continue; }

            orb.GlobalPosition = spawn.GlobalPosition;
        }

        GD.Print("STEP 2: GENERATING OBJECTS COMPLETE");
    }

    //Temporary proof of concept guy.  Eventually I want to keep track of the amount of times something has been spawned in.
    private PackedScene GetRandomObjectOfType(SpawnPosition.SpawnType type)
    {
        int index = Utility.RandomRange(0, objectScenes.Length);
        int count = 0;
        PackedScene objectScene;

        while(count < objectScenes.Length)
        {
            objectScene = objectScenes[index];
            if(PackedSceneToSpawnType[objectScene] == type)
            { return objectScene; }

            index++;
            count++;
            if(index >= objectScenes.Length)
            { index = 0; }
        }

        GD.PrintErr("No matching object of type: " + type.ToString());
        return null;
    }

    private void Cleanup()
    {
        GD.Print("Cleaning up LevelGenerator");
        // foreach (Node n in spawnedIslands)
        // {
        //     if(spawnedIslands != null)
        //     { n.QueueFree(); }
        // }
        spawnedIslands.Clear();

        if(CurrentLevel != null)
        { CurrentLevel.Free(); }
        CurrentLevel = null;
    }
}