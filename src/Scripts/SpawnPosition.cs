using Godot;
using System;

public partial class SpawnPosition : Node3D
{
    //can change byte to ushort if I make more than 255 types
    public enum SpawnType : byte
    {
        Plant,
        Mushroom,
        Weapon,
        Orb
    }

    [Export] public SpawnType Type;

}