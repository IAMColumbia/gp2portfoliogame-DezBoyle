using Godot;

public partial class RandomIslandConfig : Node
{
    [Export] public PackedScene PackedScene { get; private set; }
    [Export] public LevelGenerator.Rarity Rarity { get; private set; }
    [Export] public int RequiredSpawns { get; private set; }
}