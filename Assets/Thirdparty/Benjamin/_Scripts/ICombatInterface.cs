// interface for combat methods, making sure every class or character that is viable for combat has the neccessary methods
// while making it possible to call the methods by interacting with the interface instead of the class

using UnityEngine;

public interface ICombatInterface
{
    void Hit(float incomingDamage, float knockbackForce, Vector3 knockbackDirection);
    void Die();
    void PerformAttack();
}