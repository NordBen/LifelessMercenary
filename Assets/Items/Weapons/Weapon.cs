using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Weapon", menuName = "Item/Weapon")]
public class Weapon : Item, IEquipable
{
    public float damage;
    public List<AnimationClip> animations;
    public EEquipSlot slot = EEquipSlot.Weapon;
    public float attackSpeed = 1.5f;

    public EEquipSlot GetSlot() => slot;

    public override void Interact()
    {
        Equip();
    }

    public void Equip()
    {
        GameManager.instance.player.GetComponent<EquipmentManager>().Equip(this);
    }
}