using UnityEngine;

public class WeaponObject : MonoBehaviour
{
    public Weapon weaponData;
    public BoxCollider weaponBox;
    public Transform hitPoint;
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
        if ((collisionLayer.value & (1 << other.gameObject.layer)) == 0)
        {
            Debug.Log($"collisionlayer val: {collisionLayer.value} and other layer {other.gameObject.layer}");
            return;
        }
        if (other.gameObject.transform == owner || other.CompareTag(owner.gameObject.tag))
        {
            Debug.Log($"hit trans: {other.gameObject.transform} own = {owner}. comparing tags - other: {other.tag} : {owner.gameObject.tag}");
            return;
        }

        Debug.Log($"Hit: {other.gameObject}");
        ICombat target = other.GetComponent<ICombat>();
        if (target != null)
        {
            Debug.Log("Helhit");
            float finalDamage = weaponData.damage;
            if (owner.tag == "Player")
                finalDamage += TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.damage);
            else
                finalDamage += 10;

            target.TakeDamage(finalDamage, 5, this.transform.root.transform.forward);
            SpawnHitVFX(other, other.ClosestPoint(transform.position));
        }
    }

    public void ToggleHitDetection()
    {
        weaponBox.enabled = !weaponBox.enabled;
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