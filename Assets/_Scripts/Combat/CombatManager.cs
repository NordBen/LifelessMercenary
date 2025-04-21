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

    [SerializeField] private AnimatorOverrideController animOverrideController;

    void Start()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip += OnWeaponChanged;

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
            if (transform.tag == "Player")
                PerformAttack();
        }
    }

    private void PerformHitDetectionMelee(Collider other)
    {
        weapon.GetComponent<WeaponObject>().ToggleHitDetection();
    }

    private void PerformAttack()
    {
        if (weaponItem == null || weaponItem.animations.Count == 0)
            return;

        if (TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.stamina) == 0) return;

        if (comboIndex <= combatAnimations.Count && !isAttacking)
        {
            animOverrideController["TestAnim1"] = combatAnimations[comboIndex];

            animator.Play("Attack", 0, 0);
            isAttacking = true;
            comboIndex = (comboIndex + 1) % combatAnimations.Count;

            if (transform.root.tag == "Player")
                TempPlayerAttributes.instance.ModifyStamina(-10);

            Invoke("ResetCombo", 1f);
        }
        comboTimer = 1.0f;
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
}