using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField] public Weapon weaponItem;
    [SerializeField] public GameObject weapon;
    [SerializeField] private BoxCollider weaponBox;

    public GameObject hitFX;

    public List<AnimationClip> combatAnimations;
    public Animator animator;

    [SerializeField] int comboIndex;
    [SerializeField] private bool isAttacking;
    [SerializeField] float comboTimer;

    private AnimatorOverrideController animOverrideController;

    void Start()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip += OnWeaponChanged;
        //animator = GameManager.instance.player.GetAnimator();

        animOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animOverrideController;
    }

    private void OnDisable()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip -= OnWeaponChanged;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PerformAttack();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
    }

    private void PerformHitDetectionMelee(Collider other)
    {
        weaponBox.enabled = !weaponBox.enabled;
    }

    private void PerformHitDetectionProjectile(Collider other)
    {

    }

    private void PerformAttack()
    {
        if (weaponItem == null || weaponItem.animations.Count == 0)
            return;

        if (comboIndex <= combatAnimations.Count && !isAttacking)
        {
            animOverrideController["TestAnim1"] = combatAnimations[comboIndex];

            animator.Play("Attack", 0, 0);
            isAttacking = true;
            comboIndex = (comboIndex + 1) % combatAnimations.Count;

            Invoke("ResetCombo", 1.5f);
        }
        /*
        Collider[] hitEnemies = Physics.OverlapBox(weapon.transform.position, new Vector3(0.1f, 0.18205f, 1.4517f), Quaternion.identity, LayerMask.NameToLayer("Combat"), QueryTriggerInteraction.Ignore);

        foreach (Collider enemy in hitEnemies)
        {
            ICombat damageable = enemy.GetComponent<ICombat>();
            damageable?.TakeDamage(Random.Range(weaponItem.damage * 0.85f, weaponItem.damage * 1.15f));
            Debug.Log($"Hit: {enemy.gameObject.name}");
        }*/

        comboTimer = 1.0f;
        /*
        comboIndex++;
        if (comboIndex >= equippedWeapon.animations.Count)
        {
            comboIndex = 0;
        }*/
    }

    void OnWeaponChanged(IEquipable newWeapon)
    {
        if (newWeapon.GetSlot() == EEquipSlot.Weapon)
        {
            weaponItem = newWeapon as Weapon;
            combatAnimations.Clear();
            combatAnimations = new(weaponItem.animations);
            weaponBox = weapon.GetComponent<BoxCollider>();
        }
        else
            return;
    }

    private void ResetCombo()
    {
        isAttacking = false;
    }

    void PrintEquippedWeapon()
    {
        Debug.Log($"{weaponItem.itemName}");
    }
}