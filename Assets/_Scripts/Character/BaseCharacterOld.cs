using System;
using LM;
using UnityEngine;

public abstract class BaseCharacterOld : MonoBehaviour, ICombat
{
    [Header("Movement")]
    [Tooltip("Walk speed")]
    [Range(0.1f, 10f)]
    [SerializeField] protected float walkSpeed = 0.6f;
    [Tooltip("Run speed")]
    [Range((int)0.1f, 20f)]
    [SerializeField] protected float runSpeed = 1.5f;

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

    public event Action<float> OnHPChanged;
    public event Action<int> OnHealingUsed;

    [SerializeField] protected int level = 1;

    protected float _currentSpeed;
    protected Animator _animator;
    protected Rigidbody _rb;
    protected bool isDead = false;
    protected int healing = 0;

    protected void Start()
    {
        this.hp = this.maxHP;
        this.healing = this.maxHealables;
        this._currentSpeed = this.walkSpeed;
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        //this.weapon.InitializeWeaponStats(this.damage, this.knockbackImpulse);
    }

    protected void Update()
    {
        if (this.isDead) return;
    }

    #region ICombatInterface
    public int GetLevel() => this.level;

    public virtual void TakeDamage(float incomingDamage, float knockbackForce, Vector3 knockbackDirection)
    {
        if (this.isDead) return;
        //if (GetComponent<CombatManager>().SuccessfullParry()) return;
        
        ChangeHP(-(int)UnityEngine.Random.Range(incomingDamage * 0.9f, incomingDamage * 1.1f));

        // applies knockback by using rigid body/unity physics
        this._rb.AddForce(knockbackDirection * UnityEngine.Random.Range(knockbackForce * 0.85f, knockbackForce * 1.15f), ForceMode.Impulse);

        if (this.hp == 0)
            Die();
    }

    public virtual void Die()
    {
        isDead = true;
    }

    public bool IsDead() => this.isDead;
    #endregion

    public float GetMaxHP() => this.maxHP;

    protected void ChangeHP(float incomingValue)
    {
        this.hp = Mathf.Max(0, hp + incomingValue);
        OnHPChanged?.Invoke(this.hp);
    }

    public int GetMaxHealingItems() => this.maxHealables;

    protected virtual void Heal()
    {
        if (this.hp >= this.maxHP) return;

        if (this.healing > 0 & this.healing <= maxHealables)
        {
            healing--;
            this.hp = Mathf.Min(GetMaxHP(), hp + healAmount);
            OnHPChanged?.Invoke(this.hp);
            OnHealingUsed?.Invoke(this.healing);
        }
        else Debug.Log($"You have no more heals {healing}");
    }
}