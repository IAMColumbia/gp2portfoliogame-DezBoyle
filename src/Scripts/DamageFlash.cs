using System;
using System.Collections.Generic;
using Godot;

public partial class DamageFlash : Node
{
    [Export] private float flashDuration = 0.1f;
    [Export] private Material flashMaterial;

    private float lastTime = -420f;
    private bool flashed = true;
    private List<MeshInstance3D> meshes = new List<MeshInstance3D>();

    public override void _Ready()
    {
        meshes = Utility.GetAllNodesOfType<MeshInstance3D>(Utility.GetAllChildren(GetParent()));
    }

    public void Flash()
    {
        flashed = true;
        lastTime = Time.GetTicksMsec();
        SetMaterials();
    }

    private void SetMaterials()
    {
        foreach(MeshInstance3D mesh in meshes)
        {
            mesh.MaterialOverride = flashed ? flashMaterial : null;
        }
    }

    public override void _Process(double delta)
    {
        if(!flashed || Time.GetTicksMsec() - lastTime < flashDuration * 1000f)
        { return; }

        flashed = false;
        SetMaterials();
    }
}