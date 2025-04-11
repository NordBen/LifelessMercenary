using UnityEngine;

public interface ICombat
{
    void TakeDamage(float incomingDamage);
    void Die();
}