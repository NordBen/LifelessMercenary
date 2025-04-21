using StarterAssets;
using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[System.Serializable]
public enum TempPlayerStats
{
    strength,
    agility,
    vitality,
    health,
    maxhealth,
    stamina,
    maxstamina,
    damage,
    attackspeed,
    movementspeed
}

public class TempPlayerAttributes : MonoBehaviour
{
    public static TempPlayerAttributes instance;

    public Dictionary<TempPlayerStats, int> tempPlayerIntAttributes;
    public Dictionary<TempPlayerStats, float> tempPlayerFloatAttributes;

    public int level = 1;
    public int pointsToUse = 5;

    public event Action<float> OnHealthChanged;
    public event Action<float> OnStaminaChanged;

    public ThirdPersonController playerController;

    void Awake()
    {
        SetPlayerController(GameObject.Find("Player").GetComponent<ThirdPersonController>());

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        tempPlayerIntAttributes = new Dictionary<TempPlayerStats, int>();
        tempPlayerFloatAttributes = new Dictionary<TempPlayerStats, float>();

        tempPlayerIntAttributes.Add(TempPlayerStats.strength, 5);
        tempPlayerIntAttributes.Add(TempPlayerStats.agility, 5);
        tempPlayerIntAttributes.Add(TempPlayerStats.vitality, 5);

        tempPlayerFloatAttributes.Add(TempPlayerStats.maxhealth, (GetIntAttribute(TempPlayerStats.vitality) * 25) - 25);
        tempPlayerFloatAttributes.Add(TempPlayerStats.health, GetFloatAttribute(TempPlayerStats.maxhealth));
        tempPlayerFloatAttributes.Add(TempPlayerStats.maxstamina, GetIntAttribute(TempPlayerStats.vitality) * 15);
        tempPlayerFloatAttributes.Add(TempPlayerStats.stamina, GetFloatAttribute(TempPlayerStats.maxstamina));
        tempPlayerFloatAttributes.Add(TempPlayerStats.damage, GetIntAttribute(TempPlayerStats.strength) * 2.5f);
        tempPlayerFloatAttributes.Add(TempPlayerStats.attackspeed, 1 + GetIntAttribute(TempPlayerStats.agility) * 0.045f);
        tempPlayerFloatAttributes.Add(TempPlayerStats.movementspeed, GetIntAttribute(TempPlayerStats.agility) * 1.067f);
        UpdateStats();
    }

    public void UpdateStats()
    {
        tempPlayerFloatAttributes[TempPlayerStats.maxhealth] = (GetIntAttribute(TempPlayerStats.vitality) * 25) - 25;
        ModifyHealth(GetFloatAttribute(TempPlayerStats.maxhealth));
        tempPlayerFloatAttributes[TempPlayerStats.maxstamina] = GetIntAttribute(TempPlayerStats.vitality) * 15;
        ModifyStamina(GetFloatAttribute(TempPlayerStats.maxstamina));
        tempPlayerFloatAttributes[TempPlayerStats.attackspeed] = 1 + GetIntAttribute(TempPlayerStats.agility) * 0.045f;
        tempPlayerFloatAttributes[TempPlayerStats.movementspeed] = GetIntAttribute(TempPlayerStats.agility) * 1.067f;
        playerController.MoveSpeed = GetFloatAttribute(TempPlayerStats.movementspeed) * 0.4f;
        playerController.SprintSpeed = GetFloatAttribute(TempPlayerStats.movementspeed);
        if (GameObject.Find("Player").GetComponent<CombatManager>().weaponItem != null)
        {
            tempPlayerFloatAttributes[TempPlayerStats.damage] = playerController.GetComponent<CombatManager>().weaponItem.damage + GetIntAttribute(TempPlayerStats.strength) * 2.5f;
        }
        else
            tempPlayerFloatAttributes[TempPlayerStats.damage] = GetIntAttribute(TempPlayerStats.strength) * 2.5f;
    }

    public void ModifyHealth(float changeValue)
    {
        tempPlayerFloatAttributes[TempPlayerStats.health] = Mathf.Clamp(GetFloatAttribute(TempPlayerStats.health) + changeValue, 0 , GetFloatAttribute(TempPlayerStats.maxhealth));
        OnHealthChanged?.Invoke(GetFloatAttribute(TempPlayerStats.health));
    }

    public void ModifyStamina(float changeValue)
    {
        tempPlayerFloatAttributes[TempPlayerStats.stamina] = Mathf.Clamp(GetFloatAttribute(TempPlayerStats.stamina) + changeValue, 0, GetFloatAttribute(TempPlayerStats.maxstamina));
        OnStaminaChanged?.Invoke(GetFloatAttribute(TempPlayerStats.stamina));
    }

    public void LevelUp(int levels)
    {
        this.level += levels;
        pointsToUse += 3 * levels;
    }

    public int GetLevel()
    {
        return level;
    }

    public int GetPointsToUse()
    {
        return pointsToUse;
    }

    public bool HasPointsToUse()
    {
        return pointsToUse > 0;
    }

    public float GetFloatAttribute(TempPlayerStats attribute)
    {
        return (float)this.tempPlayerFloatAttributes[attribute];
    }

    public float GetIntAttribute(TempPlayerStats attribute)
    {
        return (int)this.tempPlayerIntAttributes[attribute];
    }

    public void SetPlayerController(ThirdPersonController plCntrl)
    {
        if (this.playerController == null)
            this.playerController = plCntrl;
    }
}
