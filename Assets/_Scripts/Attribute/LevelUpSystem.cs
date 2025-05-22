using System;
using LM;
using UnityEngine;

public class LevelUpSystem : MonoBehaviour
{
    [SerializeField] private int level = 1;
    [SerializeField] private int points = 0;
    private int pointsPerLevel = 3;

    public event Action OnLevelUp;

    void Start()
    {
        
    }

    private void OnEnable()
    {
        OnLevelUp += LevelUp;
    }

    private void OnDisable()
    {
        OnLevelUp -= LevelUp;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            CallOnLevelUp();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            UpdateBindedAttributes();
        }
    }

    public bool CanIncreaseAttribute()
    {
        return this.points > 0;
    }

    public void CallOnLevelUp()
    {
        OnLevelUp?.Invoke();
    }

    public void UpdateBindedAttributes()
    {
        foreach (var attribute in GameManager.instance.player.GetAttributeContainer().attributes.Values)
        {
            GameManager.instance.player.GetAttributeContainer().ApplyNewMod(
                GameManager.instance.player.GetAttributeContainer().attributes["Max" + attribute.ToString()].CurrentValue() * 25, attribute.ToString(), 0f);
        }
    }

    private void LevelUp()
    {
        this.level++;
        this.points += this.pointsPerLevel;
        switch (this.level)
        {
            case 5:
                GameManager.instance.ModifyDeathEnergy(1);
                break;
            case 10:
                GameManager.instance.ModifyDeathEnergy(1);
                break;
            case 15:
                GameManager.instance.ModifyDeathEnergy(1);
                break;
            case 20:
                GameManager.instance.ModifyDeathEnergy(1);
                break;
            case 30:
                GameManager.instance.ModifyDeathEnergy(5);
                break;
            default:
                break;
        }
    }
}
