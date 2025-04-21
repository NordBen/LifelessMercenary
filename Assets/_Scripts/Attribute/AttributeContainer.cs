using System.Collections.Generic;
using UnityEngine;
//using CharlieMadeAThing.NeatoTags;

public class AttributeContainer : MonoBehaviour, ICombat
{
    private bool isDead = false;

    public void TakeDamage(float incomingDamage, float knockback, Vector3 knockbackDirection) 
    {
        if (isDead) return;
        TempPlayerAttributes tempPlayerAttributes = GameObject.Find("PlayerStats").GetComponent<TempPlayerAttributes>();
        tempPlayerAttributes.ModifyHealth(-incomingDamage);

        if (tempPlayerAttributes.GetFloatAttribute(TempPlayerStats.health) == 0)
            Die();
    }

    public void Die()
    {
        isDead = true;
        Debug.Log($"{this.gameObject.name} Died");
        if (this.transform.root.name == "Player")
        {
            GameManager.instance.KillPlayer();
            TempPlayerAttributes.instance.LevelUp(3);
        }
    }
}