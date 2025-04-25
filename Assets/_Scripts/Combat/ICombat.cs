using UnityEngine;

public interface ICombat
{
    void TakeDamage(float incomingDamage, float knockbackForce, Vector3 knockbackDirection);
    void Die();
    bool IsDead();
    int Level();
}