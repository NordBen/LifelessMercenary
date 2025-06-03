using System.Collections.Generic;
using LM.AbilitySystem;
using UnityEngine;

namespace LM
{
    public class CombatManager : MonoBehaviour, ICombat
    {
        private static readonly int bIsBlocking = Animator.StringToHash("bIsBlocking");
        private static readonly int HeavyAttack = Animator.StringToHash("tHeavyAttack");
        private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");

        public float rootMotionMultiplier = 1;
        
        [SerializeField] public Weapon weaponItem;
        [SerializeField] public WeaponObject weapon;

        [SerializeField] private BaseCharacter owner;
        [SerializeField] private BaseCharacter target;

        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip blockSound;
        [SerializeField] private AudioSource combatAudio;

        public GameObject hitFX;
        public GameObject groundFX;
        public GameObject[] hitFXs;
        
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

        private void PerformHitDetectionMelee(Collider other)
        {
            weapon.ToggleHitDetection();
        }

        public void PerformLightAttack()
        {
            PerformAttack(false);
        }

        public void PerformHeavyAttack()
        {
            PerformAttack(true);
        }

        private void PerformAttack(bool isHeavy)
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

        private void ResetAttacking()
        {
            attackTagged = false;
        }

        public void PerformBlock()
        {
            this.isBlocking = true;
            animator.SetBool(bIsBlocking, this.isBlocking);
        }

        public void ResetBlocking()
        {
            this.isBlocking = false;
            animator.SetBool(bIsBlocking, this.isBlocking);
        }

        public void PerformAttackExt() => weapon.ToggleHitDetection();
        public void EndComboWindow() => isAttacking = false;

        private void OnWeaponChanged(IEquipable newWeapon)
        {
            if (newWeapon.GetSlot() != EEquipSlot.Weapon) return;

            weaponItem = newWeapon as Weapon;
            if (weaponItem != null)
            {
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
            if (TryGetComponent(out ParrySystem parryComponent))
            {
                if (parryComponent.TryParry())
                {
                    SpawnVFX(transform.position, hitFXs[2]);
                    return;
                }
            }
            float finalDamage = -incomingDamage;
            finalDamage = isBlocking ? finalDamage *= 0.1f : finalDamage;
            
            if (isBlocking)
                SpawnVFX(transform.position, hitFXs[1]);
            
            var healthAttribute = attributeComp.GetAttribute("Health");
            if (healthAttribute == null) return;

            var damageApplication = new GameplayEffectApplication(
                healthAttribute, EModifierOperationType.Add, new ConstantValueStrategy { value = finalDamage });
            
            List<GameplayEffectApplication> applications = new List<GameplayEffectApplication>();
            applications.Add(damageApplication);
            var damageEffect = EffectFactory.CreateEffect(
                "Damage", EEffectDurationType.Instant, 0f, applications);

            PlaySound(hitSound);
            attributeComp.ApplyEffect(damageEffect);

            if (healthAttribute.CurrentValue <= 0)
            {
                if (transform.root.tag == "Player")
                {
                    owner.Die();
                    Die();
                }
                else
                    owner.Die();
            }
        }

        public void HitObject(Collider other)
        {
            other.TryGetComponent(out CombatManager combatM);
            Debug.Log($"HitObject called. isBlocking={combatM.isBlocking}");
            
            if (combatM.isBlocking) {
                Debug.Log("Blocking so no vfx from HitObject");
                return; // || GetComponent<ParrySystem>().TryParry()) return;
}

            if (combatM.TryGetComponent(out ParrySystem parryComponent))
            {
                bool isParried = parryComponent.TryParry();
                Debug.Log($"TryParry returned {isParried}");
                if (parryComponent.TryParry())
                {
                    Debug.Log("Parried so no vfx from HitObject");
                    return;
                }
            }
            
            if (other.gameObject.layer == LayerMask.NameToLayer("Combat") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.Log("Spawning hit VFX!");
                SpawnVFX(other, hitFXs[0]);
                return;
            }
        }

        private void SpawnVFX(Collider collision, GameObject visualEffect)
        {
            Debug.Log("Spawning VFX");
            Vector3 hitPosition = collision.ClosestPoint(transform.position);
            Vector3 hitNormal = (hitPosition - collision.transform.position).normalized;

            if (hitNormal == Vector3.zero)
                hitNormal = -transform.forward;

            Quaternion rotation = Quaternion.LookRotation(hitNormal);

            if (hitFX != null)
            {
                GameObject vfx = Instantiate(visualEffect, hitPosition, rotation);

                var particle = vfx.GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    particle.Play();
                    Destroy(vfx, particle.main.duration);
                }
                else
                {
                    Destroy(vfx, 2f);
                }
            }
        }
        
        private void SpawnVFX(Vector3 location, GameObject visualEffect)
        {
            Debug.Log("Spawning VFX");
            Vector3 hitPosition = location;
            Vector3 hitNormal = (hitPosition - transform.position).normalized;

            if (hitNormal == Vector3.zero)
                hitNormal = -transform.forward;

            Quaternion rotation = Quaternion.LookRotation(hitNormal);

            if (hitFX != null)
            {
                GameObject vfx = Instantiate(visualEffect, hitPosition, rotation);

                var particle = vfx.GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    particle.Play();
                    Destroy(vfx, particle.main.duration);
                }
                else
                {
                    Destroy(vfx, 2f);
                }
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