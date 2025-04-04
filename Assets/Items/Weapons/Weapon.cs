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
    public List<AnimationClip> animations;
    public EEquipSlot slot = EEquipSlot.Weapon;
    public EWeaponType weaponType;
    public float attackSpeed = 1.5f;

    public EEquipSlot GetSlot() => slot;

    public override void Interact()
    {
        Equip();
    }

    public void Equip()
    {
        GameManager.instance.player.GetEquipmentManager().Equip(this);//.GetComponent<EquipmentManager>().Equip(this);
    }
}