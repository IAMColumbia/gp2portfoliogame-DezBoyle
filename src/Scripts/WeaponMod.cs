using Godot;
using System;

public partial class WeaponMod : Item
{
    public float cooldownModifier { get; private set; }
    public AudioStream shootSound { get; private set; }
}