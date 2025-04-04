using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    public Weapon weaponData;
    public Transform hitPoint;

    private void Start()
    {
        weaponData = GameManager.instance.player.GetCombatManager().weaponItem;
        
    }

    public void SetWeaponData(Weapon newData)
    {
        this.GetComponent<MeshFilter>().mesh = newData.mesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        ICombat target = other.GetComponent<ICombat>();
        if (target != null)
        {
            target.TakeDamage(weaponData.damage);
            SpawnHitVFX(other, other.ClosestPoint(transform.position));
        }
    }

    private void SpawnHitVFX(Collider other, Vector3 hitLocation)
    {
        if (GameManager.instance.player.GetCombatManager().hitFX != null)// weaponData.hitVFX != null)
        {
            //Vector3 hitNormal = other.ClosestPoint(hitPoint.position);
            Instantiate(GameManager.instance.player.GetCombatManager().hitFX, hitLocation, Quaternion.identity);
        }
    }
}