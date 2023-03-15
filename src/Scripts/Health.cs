using System;

public class Health
{
    public int Hp { get; private set; }
    public int MaxHp { get; set; }
    public float PercentHp { get { return (float)Hp / (float)MaxHp; } }
    public event Action OnDeath;
    public event Action OnHealthChanged;

    private float microDamage;

    public Health(int maxHp)
    {
        MaxHp = maxHp;
        Hp = MaxHp;
    }

    public void Damage(float amount)
    {
        microDamage += amount;
        int damageInt = (int)microDamage;
        microDamage -= damageInt;
        Damage(damageInt);
    }
    public void Damage(int amount)
    {
        if(amount == 0)
        { return; }

        OnHealthChanged?.Invoke();

        Hp -= amount;
        Hp = Math.Clamp(Hp, 0, MaxHp);
        
        if(Hp <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float amount)
    { Damage(-amount); }
    public void Heal(int amount)
    { Damage(-amount); }
}