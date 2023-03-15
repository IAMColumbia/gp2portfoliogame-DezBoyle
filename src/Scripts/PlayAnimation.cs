using Godot;

public partial class PlayAnimation : AnimationPlayer
{
    [Export] private string animationName;
    public override void _Ready()
    { base.Play(animationName); }
}