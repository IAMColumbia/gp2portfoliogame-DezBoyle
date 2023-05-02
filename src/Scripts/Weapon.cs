using Godot;
using System;
using System.Collections.Generic;

public partial class Weapon : Item
{
    //Might split this class up into Weapon(base) -> RaycastWeapon, ProjectileWeapon, Ect..
    [Export] private float range = 100f;
    [Export] private int damage = 4;
    [Export] private float cooldown = 0.2f;
    [Export] private bool automatic = false;
    [Export] private float spreadMax = 3f;
    [Export] private float spreadMin = .2f;
    [Export] private float spreadPerShot = .45f;
    [Export] private float spreadDecayRate = 5f;
    [Export] private AudioPlayer audio;
    [Export] private AudioStream shootSound;
    [Export] private string shootAnimation;

    public float SpreadPercentage { get; private set; }
    public float Cooldown
    {
        get
        {
            return cooldown;
        }
    }
    public int Damage
    {
        get
        {
            return damage;
        }
    }

    public static event Action OnPickup;

    private PhysicsRayQueryParameters3D rayParams;
    private float cooldownTimer;
    private AnimationPlayer animation;
    private TracerSpawner tracer;
    private MuzzleFlare muzzleFlare;
    private List<WeaponMod> weaponMods = new List<WeaponMod>();
    // private Node3D[] tracers;
    // private const int tracerAmt = 6;

    public override void Init(Player p)
    {
        base.Init(p);
        OnPickup?.Invoke();

        Godot.Collections.Array<Rid> exclude = new Godot.Collections.Array<Rid>();
        exclude.Add(new Rid(Player.PlayerBody as Godot.GodotObject));
        rayParams = PhysicsRayQueryParameters3D.Create(Player.Cam.GlobalPosition, Player.Cam.GlobalPosition + Player.Cam.GlobalTransform.Basis.Z * range, 1, exclude);
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

        bool fire = base.JustClicked || (automatic && base.Clicking);
        if (fire)
        { Shoot(); }

        cooldownTimer += (float)delta; //i ran the numbers
        //this will wrap in 1.079x10^31 years
        //so yeah nah, not gunna clamp it

        SpreadPercentage = Mathf.Lerp(SpreadPercentage, 0f, (float)delta * spreadDecayRate);
        SpreadPercentage = Mathf.Clamp(SpreadPercentage, 0f, 1f);
    }

    public void Shoot()
    {
        Godot.Collections.Dictionary hit;
        Node node;
        IDamagable damagable = null;

        if(cooldownTimer < Cooldown)
        { return; }
        cooldownTimer = 0f;

        //visual stuff
        audio.PlayOneShot(shootSound, -8f, Utility.RandomRange(0.9f, 1.1f));
        animation.Stop();
        animation.Play(shootAnimation);
        muzzleFlare.Flash();

        //raycast setup
        rayParams.From = Player.Cam.GlobalPosition;
        rayParams.To = Player.Cam.GlobalPosition - Player.Cam.GlobalTransform.Basis.Z * range;

        //inaccuracy
        Vector3 inaccuracy = new Vector3();
        float spread = Mathf.Lerp(spreadMin, spreadMax, SpreadPercentage);
        inaccuracy += Player.Cam.GlobalTransform.Basis.X * Utility.RandomRange(-spread, spread);
        inaccuracy += Player.Cam.GlobalTransform.Basis.Y * Utility.RandomRange(-spread, spread);
        rayParams.To += inaccuracy;
        SpreadPercentage += spreadPerShot;

        hit = GetWorld3D().DirectSpaceState.IntersectRay(rayParams);

        if (hit == null || hit.Count == 0) //no hit
        {
            tracer.ShootAt(rayParams.To, 0f, rayParams.To);
            return;
        }
        else
        { tracer.ShootAt((Vector3)hit["position"], 0f, rayParams.To); }

        node = (Node)hit["collider"];
        // GD.Print("Hit!" + node);

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

    public void AttachWeaponMod(WeaponMod mod)
    {
		GD.Print("Attaching Weapon Mod: " + mod.Name);
        mod.GetParent().RemoveChild(mod);
		AddChild(mod);

        //put the mod in a spot
		mod.Position = Vector3.Zero;
		mod.Rotation = Vector3.Zero;

        weaponMods.Add(mod);
    }

    public Item RemoveWeaponMod()
    {
        if(weaponMods.Count == 0)
        { return null; }

        int lastIndex = weaponMods.Count - 1;
        Item removedMod = weaponMods[lastIndex];
        weaponMods.RemoveAt(lastIndex);
        return removedMod;
    }
}