using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    public Weapon weaponData;
    public Transform hitPoint;
    private BoxCollider weaponBox;
    private Transform owner;
    [SerializeField] private LayerMask collisionLayer = 999;

    private void Start()
    {
        owner = this.transform.root;
        weaponData = owner.GetComponent<CombatManager>().weaponItem;
        weaponBox = GetComponent<BoxCollider>();
    }

    public void SetWeaponData(Weapon newData)
    {
        this.GetComponent<MeshFilter>().mesh = newData.mesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((collisionLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.gameObject.transform == owner || other.CompareTag(owner.gameObject.tag)) return;

        Debug.Log($"Hit: {other.gameObject}");
        ICombat target = other.GetComponent<ICombat>();
        if (target != null)
        {
            float finalDamage = weaponData.damage;
            if (owner.tag == "Player")
                finalDamage += TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.damage);
            else
                finalDamage += 10;

            target.TakeDamage(finalDamage, 5, this.transform.root.transform.forward);
        }
    }

    public void ToggleHitDetection()
    {
        weaponBox.enabled = !weaponBox.enabled;
    }
}