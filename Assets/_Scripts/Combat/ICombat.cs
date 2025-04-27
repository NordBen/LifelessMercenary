using UnityEngine;

public interface ICombat
{
    void TakeDamage(float incomingDamage, float knockbackForce, Vector3 knockbackDirection);
    bool IsDead();
    int GetLevel();
}