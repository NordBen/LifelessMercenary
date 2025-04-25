using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    public Weapon weaponData;
    public BoxCollider weaponBox;
    public Transform hitPoint;

    private void Start()
    {
        weaponData = GameManager.instance.player.GetCombatManager().weaponItem;
        this.weaponBox = GetComponent<BoxCollider>();
    }

    public void SetWeaponData(Weapon newData)
    {
        this.GetComponent<MeshFilter>().mesh = newData.mesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform == this.transform.root) return;

        Debug.Log($"Hit: {other.gameObject}");
        ICombat target = other.GetComponent<ICombat>();
        if (target != null)
        {
            target.TakeDamage(weaponData.damage, 0, Vector3.zero);
            SpawnHitVFX(other, other.ClosestPoint(transform.position));
        }
    }

    public void ToggleHitBox()
    {
        this.weaponBox.enabled = !this.weaponBox.enabled;
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