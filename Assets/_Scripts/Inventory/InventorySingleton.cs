using UnityEngine;

[System.Serializable]
public enum EEquipSlot
{
    None,
    Head,
    Torso,
    Leggs,
    Boots,
    Amulet,
    Weapon,
    Quickslot
}

[System.Serializable]
public enum EItemType
{
    Basic,
    Weapon,
    Armor,
    Quest
}

[System.Serializable]
public enum EItemGrade
{
    Uncommon,
    Common,
    Great,
    Epic,
    Unique,
    Legendary
}