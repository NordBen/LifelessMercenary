using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EWeaponType
{
    Sword,
    Axe,
    Hammer,
    Dagger,
    Box
}

[System.Serializable]
[CreateAssetMenu(fileName = "Weapon", menuName = "Item/Weapon")]
public class Weapon : Item, IEquipable
{
    public float damage;
    public float attackSpeed = 1.5f;
    
    public List<CombatAnimation> lightAttackAnimations;
    public List<CombatAnimation> heavyAttackAnimations;
    public EEquipSlot slot = EEquipSlot.Weapon;
    public EWeaponType weaponType;

    public EEquipSlot GetSlot() => slot;
    /*
    public override void Interact()
    {
        Equip();
    }*/

    public void Equip()
    {
        GameManager.instance.player.GetComponent<EquipmentManager>().TryEquip(this);
    }
}