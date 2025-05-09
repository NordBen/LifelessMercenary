using System;
using StarterAssets;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour//, ICombat
{
    private static readonly int bIsBlocking = Animator.StringToHash("bIsBlocking");
    private static readonly int ParryHash = Animator.StringToHash("tParry");
    
    [SerializeField] private float parryWindowDuration = 0.2f;
    [SerializeField] private float parryCooldown = 1f;
    private bool _Parriable = false;
    private bool _parried = false;
    private float _lastParryTime = 0;
    [SerializeField] private LayerMask enemyLayer;
    
    [SerializeField] private Vector3 lastCheckPosition;
    [SerializeField] private bool wasLastCheckSuccessful;
    [SerializeField] private Collider[] lastHitColliders;
    
    public event Action<Transform> OnParry;
    [SerializeField] private float parryRange = 2;
    
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

    [SerializeField] private bool attackTagged = false;
    
    public bool IsAttacking() => attackTagged;

    void Start()
    {
        animOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animOverrideController;
        
        //attributeComp = owner.GetComponent<GameplayAttributeComponent>();
    }

    private void OnEnable()
    {
        Invoke("SubscribeWeaponChange", 1);
        //SubscribeWeaponChange();
        //GameManager.instance.player.GetEquipmentManager().OnEquip += OnWeaponChanged;
    }

    private void SubscribeWeaponChange()
    {
        Debug.Log("CombatManager OnEnable called");
        if (GameManager.instance?.player?.GetEquipmentManager() != null)
        {
            Debug.Log("Successfully subscribing to OnEquip event");
            GameManager.instance.player.GetEquipmentManager().OnEquip += OnWeaponChanged;
        }
        else
        {
            Debug.Log("Failed to subscribe to OnEquip event - dependencies not ready");
        }
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
        
        if (Input.GetKeyDown(KeyCode.X))
            this.TestPerformParryCheck();
        
        if (Input.GetKey(KeyCode.X))
        {
            //Debug.Log("Blocking");
            PerformBlock();
        }
        else
        {
            this.isBlocking = false;
            animator.SetBool(bIsBlocking, this.isBlocking);
        }
    }

    public void ParryWindwow()
    {
        StartCoroutine(ParryWindowCoroutine());
    }
    
    private IEnumerator ParryWindowCoroutine()
    {/*
        _Parriable = true;
        _lastParryTime = Time.time;
        yield return new WaitForSeconds(parryWindowDuration);
        _Parriable = false;*/
        this._Parriable = true;
        yield return new WaitForSeconds(parryWindowDuration);
        this._Parriable = false;
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
            attackTagged = true;

            animator.Play("Attack", 0, 0);
            isAttacking = true;
            comboIndex = (comboIndex + 1) % _primaryAttacks.Count;

            if (transform.root.tag == "Player")
                TempPlayerAttributes.instance.ModifyStamina(-10);

            Invoke("ResetCombo", 1f);
            Invoke("ResetAttacking", 6f);
        }
        comboTimer = 1.0f;
    }
    
    private void ResetAttacking()
    {
        attackTagged = false;
    }

    private void PerformBlock()
    {
        
        /*
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, parryRange);

        foreach (var hitCollider in hitColliders)
        {
            var attacker = hitCollider.GetComponent<CombatManager>();
            if (TryParry(attacker))
            {
                _parried = true;
                attacker._Parriable = false;
                animator.SetTrigger(ParryHash);
                OnParry?.Invoke(hitCollider.transform);
                Debug.Log($"{gameObject.name} performed successful parry against {hitCollider.name}");
                return;
            }
        }*/
        

        this.isBlocking = true;
        animator.SetBool(bIsBlocking, this.isBlocking);
    }

    private void PerformParry(CombatManager enemyCombat, Collider hitCollider)
    {
        wasLastCheckSuccessful = true;
        this._parried = true;
        enemyCombat._Parriable = false;
        animator.SetTrigger(ParryHash);
        OnParrySuccessful?.Invoke(hitCollider.transform);
        Debug.Log($"{this.gameObject.name} _parried: {_parried}");
        Debug.Log($"{gameObject.name} performed successful parry against {hitCollider.name}");
        Invoke("ResetParry", 0.86f);
    }

    private void ResetParry()
    {
        this._parried = false;
    }

    private void TestPerformParryCheck()
    {
        lastCheckPosition = transform.position;
        lastHitColliders = Physics.OverlapSphere(transform.position, parryRange, enemyLayer);
        wasLastCheckSuccessful = false;

        foreach (var hitCollider in lastHitColliders)
        {
            // Skip if we're checking ourselves
            if (hitCollider.gameObject == gameObject) continue;

            var enemyCombat = hitCollider.GetComponent<CombatManager>();
            if (enemyCombat != null && enemyCombat._Parriable)
                PerformParry(enemyCombat, hitCollider);
        }
    }
    
    private void OnValidate()
    {
        // Ensure we're not on the enemy layer ourselves
        if (((1 << gameObject.layer) & enemyLayer.value) != 0)
        {
            Debug.LogWarning($"CombatManager on {gameObject.name} is on a layer included in its enemyLayer mask. This might cause self-detection issues.");
        }
    }

    private void OnDrawGizmos()
    {
        if (_Parriable)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, parryRange);
        }
/*
        // Draw the last OverlapSphere check
        if (Application.isPlaying)
        {
            // Draw the sphere where we last checked
            Gizmos.color = wasLastCheckSuccessful ? Color.green : Color.red;
            Gizmos.DrawWireSphere(lastCheckPosition, parryRange);

            // Draw lines to all detected colliders
            if (lastHitColliders != null)
            {
                foreach (var hitCollider in lastHitColliders)
                {
                    if (hitCollider != null && hitCollider.gameObject != gameObject)
                    {
                        // Draw yellow line to parriable enemies
                        if (hitCollider.GetComponent<CombatManager>()._Parriable)
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawLine(lastCheckPosition, hitCollider.transform.position);
                        }
                        // Draw white line to non-parriable enemies
                        else
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawLine(lastCheckPosition, hitCollider.transform.position);
                        }
                    }
                }
            }
        }*/
    }

    private bool TryParry(CombatManager attacker)
    {/*
        if (Time.time >= _lastParryTime + parryCooldown)
        {
            _lastParryTime = Time.time;
            return true;
        }
        return false;*/
        if (attacker) 
            Debug.Log($"Trying to Parry with result: {attacker._Parriable} * {attacker != null && attacker._Parriable} against {attacker.gameObject}");
        return attacker != null && attacker._Parriable;
    }
    
    public bool SuccessfullParry() => this._parried;
    
    public void PerformAttackExt() => weapon.ToggleHitDetection();

    private void OnWeaponChanged(IEquipable newWeapon)
    {
        if (newWeapon.GetSlot() != EEquipSlot.Weapon) return;

        weaponItem = newWeapon as Weapon;
        if (weaponItem != null)
        {
            _primaryAttacks.Clear();
            _primaryAttacks = new(weaponItem.animations);
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
        /*
        var damageEffect = GameplayEffectFactory.CreateEffect(
            "damagable", EEffectDurationType.Instant, 0, 0,
            EModifierOperationType.Add, healthAttribute, new ConstantValueStrategy() { value = -incomingDamage });
        attributeComp.ApplyEffect(damageEffect);*/

        if (healthAttribute.CurrentValue() <= 0)
            owner.Die();
    }

    public bool IsDead() => owner.IsDead();
    public int GetLevel() => 1;
}