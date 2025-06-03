using LM.AbilitySystem;
using UnityEngine;

namespace LM
{
    public class WeaponObject : MonoBehaviour
    {
        public Weapon weaponData;
        public BoxCollider weaponBox;
        [SerializeField] private Transform owner;
        [SerializeField] private LayerMask collisionLayer = 999;
        private CombatManager m_OwingCombatManager;

        private void Start()
        {
            owner = this.transform.root;
            weaponBox = GetComponent<BoxCollider>();
            m_OwingCombatManager = owner.GetComponent<CombatManager>();
            if (weaponData == null)
                weaponData = m_OwingCombatManager.weaponItem;
        }

        public void SetWeaponData(Weapon newData)
        {
            this.weaponData = newData;
            this.GetComponent<MeshFilter>().mesh = newData.mesh;
        }

        public void ToggleHitDetection()
        {
            weaponBox.enabled = !weaponBox.enabled;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("Hit other " + other.gameObject);
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
                if (owner.TryGetComponent(out GameplayAttributeComponent owningChararacterAttributes))
                {
                    if (owningChararacterAttributes != null)
                    {
                        finalDamage += owningChararacterAttributes.GetAttribute("Damage").CurrentValue;
                    }
                }
                if (owner.tag == "Player")
                    finalDamage += 5;
                else
                {
                    
                    // finalDamage += 10;
                }
                Debug.Log($"final damage: {finalDamage} from {this.gameObject.name} owned by {owner.gameObject.name}");
                
                Debug.Log("Target trying to take damage is: " + target);
                target.TakeDamage(finalDamage, 5, this.transform.root.transform.forward);
                m_OwingCombatManager.HitObject(other);
            }
        }
    }
}

/*
private void SpawnVFX(Collision collision)
{
    ContactPoint contact = collision.contacts[0];
    Vector3 hitPosition = contact.point;
    Vector3 hitNormal = contact.normal;

    if (GameManager.instance.player.GetCombatManager().hitFX != null)
    {
        Quaternion rotation = Quaternion.LookRotation(hitNormal);
        GameObject vfx = Instantiate(GameManager.instance.player.GetCombatManager().hitFX, hitPosition,
            rotation);

        var particle = vfx.GetComponent<ParticleSystem>();
        if (particle != null)
        {
            Destroy(vfx, particle.main.duration);
        }
        else
        {
            Destroy(vfx, 2f);
        }
    }
}*/