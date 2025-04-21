using System;
using UnityEngine;

public class BaseCharacter : MonoBehaviour, ICombat
{
    // Basic Variables common for movement of all characters
    [Header("Movement")]
    [Tooltip("Walk speed")]
    [Range(0.1f, 10f)]
    [SerializeField] protected float walkSpeed = 0.6f;
    [Tooltip("Run speed")]
    [Range((int)0.1f, 20f)]
    [SerializeField] protected float runSpeed = 1.5f;

    // Variables needed for combat, dealing damage and knockback
    [Header("Combat")]
    [Tooltip("Max Hit Points, maximum amount of damage the player can handle")]
    [Range(10f, 1000f)]
    [SerializeField] protected float maxHP = 100f;
    private float hp;
    [SerializeField] protected int maxHealables = 3;
    [SerializeField] protected float healAmount = 5f;
    [Tooltip("Damage, how much default damage is dealt per attack")]
    [Range(1f, 99f)]
    [SerializeField] protected float damage = 20;
    [Tooltip("Knockback, how much default knockback is dealt in attacks")]
    [Range(0.1f, 10f)]
    [SerializeField] private float knockbackImpulse = 6;
    [SerializeField] private WeaponObject weapon;

    // Event used for displaying Hit Point changes in the HUD and GUI
    public event Action<float> OnHPChanged;
    public event Action<int> OnHealingUsed;

    // "private" common variables for all characters
    protected float _currentSpeed;
    protected Animator _animator;
    protected Rigidbody _rb;
    protected bool isDead = false;
    protected int healing = 0;

    protected void Start()
    {
        // initiates starting values that depend on other things and sets the variables of the weapon to be correct with what the character has for simplicity
        this.hp = this.maxHP;
        this.healing = this.maxHealables;
        this._currentSpeed = this.walkSpeed;
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }

    protected void Update()
    {
        if (this.isDead) return;
    }

    #region ICombatInterface
    // virtual methods that have base functionality common for all, but overridable by inherited classes if they need special logic or similar function but don't want the base logic
    public virtual void TakeDamage(float incomingDamage, float knockbackForce, Vector3 knockbackDirection)
    {
        if (this.isDead) return;
        // deals damage by giving a negative value for changing the hit points value by. also picks a random number between -10% and +10% of the actual damage for some randomness
        // casts the random range value to an integer so the health is not reduced to weird numbers, could also be done for example in ChangeHP method instead, or just simply use integer and not float variables
        ChangeHP(-(int)UnityEngine.Random.Range(incomingDamage * 0.9f, incomingDamage * 1.1f));

        // applies knockback by using rigid body/unity physics
        this._rb.AddForce(knockbackDirection * UnityEngine.Random.Range(knockbackForce * 0.85f, knockbackForce * 1.15f), ForceMode.Impulse);

        // checks if the hp is 0 and if true triggers Die
        if (this.hp == 0)
            Die();
    }

    public virtual void Die()
    {
        isDead = true;
    }

    public void PerformAttack()
    {
        weapon.ToggleHitDetection();
    }
    #endregion

    public float GetMaxHP()
    {
        return this.maxHP;
    }

    protected void ChangeHP(float incomingValue)
    {
        this.hp = Mathf.Clamp(hp + incomingValue, 0, GetMaxHP());
        OnHPChanged?.Invoke(this.hp);
    }

    public int GetMaxHealingItems()
    {
        return this.maxHealables;
    }

    protected virtual void Heal()
    {
        if (this.hp >= this.maxHP) return;

        if (this.healing > 0 & this.healing <= maxHealables)
        {
            healing--;
            ChangeHP(healAmount);
            OnHealingUsed?.Invoke(this.healing);
        } else Debug.Log($"You have no more heals {healing}");
    }
}