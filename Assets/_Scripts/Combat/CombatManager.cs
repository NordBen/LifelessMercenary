using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    //private static readonly int bIsBlocking = Animator.StringToHash("bIsBlocking");

    [SerializeField] public Weapon weaponItem;
    [SerializeField] public WeaponObject weapon;

    public GameObject hitFX;

    public List<AnimationClip> combatAnimations;
    public Animator animator;

    [SerializeField] int comboIndex;
    [SerializeField] private bool isAttacking;
    [SerializeField] float comboTimer;

    [SerializeField] private AnimatorOverrideController animOverrideController;

    public bool isBlocking = false;

    void Start()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip += OnWeaponChanged;
        GameManager.instance.player.GetEquipmentManager().OnEquip += TestAddit;

        animOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animOverrideController;
    }

    private void OnEnable()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip += OnWeaponChanged;
        GameManager.instance.player.GetEquipmentManager().OnEquip += TestAddit;
    }

    private void OnDisable()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip -= OnWeaponChanged;
        GameManager.instance.player.GetEquipmentManager().OnEquip -= TestAddit;
    }

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

    void TestAddit(IEquipable newWeapon)
    {
        Debug.Log($"Called Equip and listened from Combat Manager for {newWeapon} with slot {newWeapon.GetSlot()}");
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

    void PrintEquippedWeapon()
    {
        Debug.Log($"{weaponItem.itemName}");
    }
}