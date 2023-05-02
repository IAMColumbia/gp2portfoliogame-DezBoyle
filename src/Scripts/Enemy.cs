using System;
using Godot;

public partial class Enemy : Node3D, IDamagable
{
    [Export] private int hp = 20;

    protected Health health;

    public override void _Ready()
    {
        base._Ready();

        health = new Health(hp);
        health.OnDeath += OnDeathCallback;
        health.OnHealthChanged += OnHealthChangedCallback;
    }

    public void Damage(int amount)
    { health.Damage(amount); }

    private void OnDeathCallback()
    {
        QueueFree();
    }

    private void OnHealthChangedCallback()
    {
        // GD.Print("Enemy Health: " + health.Hp);
    }
}