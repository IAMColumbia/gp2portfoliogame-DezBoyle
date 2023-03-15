using Godot;
using System;

public partial class Weapon : Item
{
    //Might split this class up into Weapon(base) -> RaycastWeapon, ProjectileWeapon, Ect..
    [Export] public float Range = 100f;
    [Export] public int Damage = 4;
    [Export] public float Cooldown = 0.2f;
    [Export] public bool Automatic = false;
    [Export] public float Spread = 10f;
    [Export] private AudioPlayer audio;
    [Export] private AudioStream shootSound;
    [Export] private string shootAnimation;

    private PhysicsRayQueryParameters3D rayParams;
    private float lastTimeShot = -420f;
    private AnimationPlayer animation;
    private TracerSpawner tracer;
    private MuzzleFlare muzzleFlare;
    // private Node3D[] tracers;
    // private const int tracerAmt = 6;

    public override void Init(Player p)
    {
        base.Init(p);

        Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid>();
        exclude.Add(new Rid(Player.PlayerBody as Godot.GodotObject));
        rayParams = PhysicsRayQueryParameters3D.Create(Player.Cam.GlobalPosition, Player.Cam.GlobalPosition + Player.Cam.GlobalTransform.Basis.Z * Range, 1, exclude);
        rayParams.CollideWithAreas = true;

        animation = GetNode<AnimationPlayer>("AnimationPlayer");
        muzzleFlare = GetNode<MuzzleFlare>("MuzzleFlare");
        tracer = GetNode<TracerSpawner>("TracerSpawner");
        // tracers = new Node3D[tracerAmt];
        // Node3D tracerScene = GetNode<Node3D>("Tracer");
        // tracers[0] = tracerScene;
        // for(int i = 1; i < tracerAmt; i++)
        // {
        //     tracers[i] = (Node3D)tracerScene.Duplicate((int)DuplicateFlags.UseInstantiation);
        // }
        // foreach(Node3D node in tracers)
        // { node.Visible = false; }


    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if(base.JustClicked || (Automatic && base.Clicking))
        { Shoot(); }
    }

    private void Shoot()
    {
        Godot.Collections.Dictionary hit;
        Node node;
        IDamagable damagable = null;

        if(Time.GetTicksMsec() - lastTimeShot < Cooldown * 1000)
        { return; }

        //visual stuff
        lastTimeShot = Time.GetTicksMsec();
        audio.PlayOneShot(shootSound, -8f, Utility.RandomRange(0.9f, 1.1f));
        animation.Stop();
        animation.Play(shootAnimation);
        muzzleFlare.Flash();

        //raycast setup
        rayParams.From = Player.Cam.GlobalPosition;
        rayParams.To = Player.Cam.GlobalPosition - Player.Cam.GlobalTransform.Basis.Z * Range;

        //inaccuracy
        Vector3 inaccuracy = new Vector3();
        inaccuracy += Player.Cam.GlobalTransform.Basis.X * Utility.RandomRange(-Spread, Spread);
        inaccuracy += Player.Cam.GlobalTransform.Basis.Y * Utility.RandomRange(-Spread, Spread);
        rayParams.To += inaccuracy;

        hit = GetWorld3D().DirectSpaceState.IntersectRay(rayParams);

        if (hit == null || hit.Count == 0) //no hit
        {
            tracer.ShootAt(rayParams.To, 0f, rayParams.To);
            return;
        }
        else
        { tracer.ShootAt((Vector3)hit["position"], 0f, rayParams.To); }

        node = (Node)hit["collider"];
        GD.Print("Hit!" + node);

        while (damagable == null) //search for interactables.
        {
            damagable = node as IDamagable;
            node = node.GetParentOrNull<Node>();
            if (node == null) //Went up entire tree, no interactable
            { break; }
        }

        if(damagable == null)
        { return; }

        damagable.Damage(Damage);
        UI.instance.ShowHitmarker();
    }
}