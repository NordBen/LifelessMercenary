using System;
using System.Collections.Generic;
using System.Collections;
using LM.AbilitySystem;
using UnityEngine;

namespace LM
{
    public class CombatManager : MonoBehaviour, ICombat
    {
        private static readonly int bIsBlocking = Animator.StringToHash("bIsBlocking");
        private static readonly int ParryHash = Animator.StringToHash("tParry");
        private static readonly int HeavyAttack = Animator.StringToHash("tHeavyAttack");
        private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");

        public float rootMotionMultiplier = 1;

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

        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip parrySound;
        [SerializeField] private AudioClip blockSound;
        [SerializeField] private AudioSource combatAudio;

        public event Action OnAttackStarted;
        public event Action OnAttackLanded;
        public event Action<Transform> OnParrySuccessful;

        public GameObject hitFX;

        public List<AnimationClip> _primaryAttacks;
        public List<AnimationClip> _optionalAttacks;
        public List<CombatAnimation> _LightAttacks;
        public List<CombatAnimation> _HeavyAttacks;

        public GameplayAttributeComponent attributeComp;

        [SerializeField] int comboIndex;
        [SerializeField] private bool isAttacking;
        [SerializeField] float comboTimer;

        [SerializeField] private AnimatorOverrideController animOverrideController;
        public Animator animator;

        public bool isBlocking = false;

        [SerializeField] private bool attackTagged = false;

        public bool IsAttacking() => attackTagged;

        bool wasParried;

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
                    PerformLightAttack();
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (transform.tag == "Player")
                    PerformHeavyAttack();
            }

            if (Input.GetKeyDown(KeyCode.X))
                if (transform.tag == "Player")
                    this.TestPerformParryCheck();

            if (Input.GetKey(KeyCode.X))
            {
                //Debug.Log("Blocking");
                if (transform.tag == "Player")
                    PerformBlock();
            }
            else
            {
                this.isBlocking = false;
                animator.SetBool(bIsBlocking, this.isBlocking);
            }
        }
        
        private void TryParryEnemyAttack()
        {
            var player = GameManager.instance.player;
            bool wasParried = player.GetComponent<ParrySystem>().TryParryAttack();
            if (wasParried)
            {
                // Cancel the attack, stagger the enemy, etc.
            }
            else
            {
                // Apply damage or hit reaction
            }
        }

        public void ParryWindwow()
        {
            StartCoroutine(ParryWindowCoroutine());
        }

        private IEnumerator ParryWindowCoroutine()
        {
            /*
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

        private void PerformLightAttack()
        {
            PerformAttackV2(false);
        }

        private void PerformHeavyAttack()
        {
            PerformAttackV2(true);
        }

        private void PerformAttack(bool isHeavy)
        {
            List<AnimationClip> _attackAnims = new();
            var animSpeed = 1f;

            if (isHeavy)
            {
                _attackAnims = _optionalAttacks;
                animSpeed = 0.86f;
            }
            else
            {
                _attackAnims = _primaryAttacks;
                animSpeed = 1.33f;
            }

            if (weaponItem == null || _attackAnims.Count == 0)
                return;

            // return if no stamina

            if (comboIndex <= _attackAnims.Count && !isAttacking)
            {
                animOverrideController["TestAnim1"] = _attackAnims[comboIndex];
                animator.SetFloat(AttackSpeed, animSpeed);
                attackTagged = true;

                if (isHeavy)
                {
                    animator.Play("HeavyAttack", 0, 0);
                }
                else
                {
                    animator.Play("LightAttack", 0, 0);
                }

                isAttacking = true;
                comboIndex = (comboIndex + 1) % _attackAnims.Count;

                if (transform.root.tag == "Player")
                    // Reduce Stamina

                Invoke("ResetCombo", 1f);
                Invoke("ResetAttacking", 6f);
            }

            comboTimer = 1.0f;
        }

        private void PerformAttackV2(bool isHeavy)
        {
            if (weaponItem == null || (_LightAttacks.Count == 0 || _HeavyAttacks.Count == 0)) return;

            if (transform.root.name == "Player")
                if (GetComponent<GameplayAttributeComponent>().GetAttribute("Stamina").CurrentValue == 0)
                    return;

            List<CombatAnimation> attackAnimations = isHeavy ? _HeavyAttacks : _LightAttacks;

            if (comboIndex < attackAnimations.Count && !isAttacking)
            {
                var animSpeed = isHeavy ? 0.85f : 1.25f;
                animSpeed *= attackAnimations[comboIndex].speed * 1;
                isAttacking = true;
                animator.SetFloat(AttackSpeed, animSpeed);
                animOverrideController["TestAnim1"] = attackAnimations[comboIndex].clip;
                attackTagged = true;

                if (isHeavy)
                {
                    animator.Play("HeavyAttack", 1, 0);
                }
                else
                {
                    animator.Play("LightAttack", 1, 0);
                }

                comboIndex = (comboIndex + 1) % attackAnimations.Count;
                //GetComponent<PlayerControllerV2>()._mc.ApplyForce(transform.forward, testAttacks[comboIndex].forwardMomentum * rootMotionMultiplier);

                //if (transform.root.tag == "Player")
                    // reduce stamina
            }
        }

        private void PerformAttack()
        {
            if (weaponItem == null) // || weaponItem.animations.Count == 0)
                return;

            // return if no stamina

            if (comboIndex <= _primaryAttacks.Count && !isAttacking)
            {
                animOverrideController["TestAnim1"] = _primaryAttacks[comboIndex];
                attackTagged = true;

                animator.Play("Attack", 0, 0);
                isAttacking = true;
                comboIndex = (comboIndex + 1) % _primaryAttacks.Count;

                //if (transform.root.tag == "Player")
                    // reduce stamina
            }
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
                Debug.LogWarning(
                    $"CombatManager on {gameObject.name} is on a layer included in its enemyLayer mask. This might cause self-detection issues.");
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
        {
            /*
                if (Time.time >= _lastParryTime + parryCooldown)
                {
                    _lastParryTime = Time.time;
                    return true;
                }
                return false;*/
            if (attacker)
                Debug.Log(
                    $"Trying to Parry with result: {attacker._Parriable} * {attacker != null && attacker._Parriable} against {attacker.gameObject}");
            return attacker != null && attacker._Parriable;
        }

        public bool SuccessfullParry() => this._parried;

        public void PerformAttackExt() => weapon.ToggleHitDetection();
        public void EndComboWindow() => isAttacking = false;

        private void OnWeaponChanged(IEquipable newWeapon)
        {
            if (newWeapon.GetSlot() != EEquipSlot.Weapon) return;

            weaponItem = newWeapon as Weapon;
            if (weaponItem != null)
            {
                _primaryAttacks.Clear();
                //_primaryAttacks = new(weaponItem.animations);
                _optionalAttacks.Clear();
                //_optionalAttacks = new(weaponItem.optionalAnimations);
                _LightAttacks.Clear();
                _LightAttacks = new(weaponItem.lightAttackAnimations);
                _HeavyAttacks.Clear();
                _HeavyAttacks = new(weaponItem.heavyAttackAnimations);
            }
        }

        private void ResetCombo()
        {
            isAttacking = false;
        }

        public void TakeDamage(float incomingDamage, float knockbackForce, Vector3 knockbackDirection)
        {
            if (owner.IsDead()) return;
                
            Debug.Log($"Take Damaged from {this} of {transform.root} for {incomingDamage}");
            var healthAttribute = attributeComp.GetAttribute("Health");
            if (healthAttribute == null) return;

            var damageApplication = new GameplayEffectApplication(
                healthAttribute, 
                EModifierOperationType.Add, 
                new ConstantValueStrategy
                {
                    value = -incomingDamage
                });
            List<GameplayEffectApplication> applications = new List<GameplayEffectApplication>();
            applications.Add(damageApplication);
            var damageEffect = EffectFactory.CreateEffect(
                "Damage",
                EEffectDurationType.Instant,
                0f,
                applications
            );

            attributeComp.ApplyEffect(damageEffect);
            PlaySound(hitSound);

            if (healthAttribute.CurrentValue <= 0)
            {
                if (transform.root.tag == "Player")
                    Die();
                else
                    owner.Die();
            }
        }

        public bool IsDead() => owner.IsDead();
        public int GetLevel() => 1;

        public void Die()
        {
            Debug.Log($"{this.gameObject.name} Died");
            if (this.transform.root.name == "Player")
            {
                GameManager.instance.KillPlayer();
                // level up
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
                combatAudio.PlayOneShot(clip);
        }
    }
}