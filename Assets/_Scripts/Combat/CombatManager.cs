using System;
using StarterAssets;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour, ICombat
{
    private static readonly int bIsBlocking = Animator.StringToHash("bIsBlocking");

    [SerializeField] public Weapon weaponItem;
    [SerializeField] public WeaponObject weapon;
    
    [SerializeField] private BaseCharacter owner;
    [SerializeField] private BaseCharacter target;

    public event Action OnAttackStarted;
    public event Action OnAttackLanded;
    public event Action<Transform> OnParrySuccessful;
    
    public GameObject hitFX;

    public List<AnimationClip> _primaryAttacks;
    public List<AnimationClip> _optionalAttacks;

    private GameplayAttributeComponent attributeComp;

    [SerializeField] int comboIndex;
    [SerializeField] private bool isAttacking;
    [SerializeField] float comboTimer;

    [SerializeField] private AnimatorOverrideController animOverrideController;
    public Animator animator;

    public bool isBlocking = false;

    void Start()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip += OnWeaponChanged;

        animOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animOverrideController;
        
        attributeComp = owner.GetComponent<GameplayAttributeComponent>();
    }

    private void OnEnable()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip += OnWeaponChanged;
    }

    private void OnDisable()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip -= OnWeaponChanged;
    }

    public void TriggerAttackStarted() => OnAttackStarted?.Invoke();
    public void TriggerAttackLanded() => OnAttackLanded?.Invoke();
    
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (transform.tag == "Player")
                PerformAttack();
        }

        if (Input.GetKey(KeyCode.X))
        {
            Debug.Log("Blocking");
            PerformBlock();
        }
        else
        {
            this.isBlocking = false;
            //animator.SetBool(bIsBlocking, this.isBlocking);
        }
        //Debug.Log(this.isBlocking);
    }

    private void PerformHitDetectionMelee(Collider other)
    {
        weapon.ToggleHitDetection();
    }

    public void Attack()
    {
        OnAttackStarted?.Invoke();
        PerformAttack();
    }

    private void PerformAttack()
    {
        if (weaponItem == null || weaponItem.animations.Count == 0)
            return;

        if (TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.stamina) == 0) return;

        if (comboIndex <= _primaryAttacks.Count && !isAttacking)
        {
            animOverrideController["TestAnim1"] = _primaryAttacks[comboIndex];

            animator.Play("Attack", 0, 0);
            isAttacking = true;
            comboIndex = (comboIndex + 1) % _primaryAttacks.Count;

            if (transform.root.tag == "Player")
                TempPlayerAttributes.instance.ModifyStamina(-10);

            Invoke("ResetCombo", 1f);
        }
        comboTimer = 1.0f;
    }

    private void PerformBlock()
    {
        if (TryParry())
        {
            //TODO parry instead of block
        }

        this.isBlocking = true;
        //animator.SetBool(bIsBlocking, this.isBlocking);
    }

    private bool TryParry()
    {
        return UnityEngine.Random.Range(0, 2) > 0;
    }

    void OnWeaponChanged(IEquipable newWeapon)
    {
        if (newWeapon.GetSlot() != EEquipSlot.Weapon)
        {
            return;
        }
        else
        {
            /*weaponItem = weapon.weaponData;
            combatAnimations.Clear();
            combatAnimations = new(weaponItem.animations);*/
        }    
    }

    private void ResetCombo()
    {
        isAttacking = false;
    }

    public void TakeDamage(float incomingDamage, float knockbackForce, Vector3 knockbackDirection)
    {
        var healthAttribute = attributeComp.GetAttribute("Health");
        if (healthAttribute == null) return;
        
        var damageEffect = GameplayEffectFactory.CreateEffect(
            "damagable", EEffectDurationType.Instant, 0, 0,
            EModifierOperationType.Add, healthAttribute, new ConstantValueStrategy() { value = -incomingDamage });
        attributeComp.ApplyEffect(damageEffect);

        if (healthAttribute.CurrentValue() <= 0)
            owner.Die();
    }

    public bool IsDead() => owner.IsDead();

    public int GetLevel() => 1;// attributeComp.GetLevel();
}