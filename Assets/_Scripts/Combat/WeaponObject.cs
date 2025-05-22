using UnityEngine;

namespace LM
{
    public class WeaponObject : MonoBehaviour
    {
        public Weapon weaponData;
        public BoxCollider weaponBox;
        public Transform hitPoint;
        [SerializeField] private Transform owner;
        [SerializeField] private LayerMask collisionLayer = 999;
        public GameObject vfx;

        private void Start()
        {
            owner = this.transform.root;
            weaponBox = GetComponent<BoxCollider>();
            if (weaponData == null)
                weaponData = owner.GetComponent<CombatManager>().weaponItem;
        }

        public void SetWeaponData(Weapon newData)
        {
            //Debug.Log($"incoming weapondata: {newData}");
            this.weaponData = newData;
            //Debug.Log($"weapon's data: {this.weaponData}");
            this.GetComponent<MeshFilter>().mesh = newData.mesh;
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
                Debug.Log(
                    $"hit trans: {other.gameObject.transform} own = {owner}. comparing tags - other: {other.tag} : {owner.gameObject.tag}");
                return;
            }

            Debug.Log($"Hit: {other.gameObject}");
            ICombat target = other.GetComponent<ICombat>();
            if (target != null)
            {
                Debug.Log("Helhit");
                float finalDamage = weaponData.damage;
                if (owner.tag == "Player")
                    finalDamage += 5;
                else
                {
                    Debug.Log(finalDamage);
                    // finalDamage += 10;
                }


                Debug.Log("Target trying to take damage is: " + target);
                target.TakeDamage(finalDamage, 5, this.transform.root.transform.forward);
                SpawnVFX(other);
                //SpawnHitVFX(other, other.ClosestPoint(transform.position));
            }
        }

        public void ToggleHitDetection()
        {
            weaponBox.enabled = !weaponBox.enabled;
        }

        private void SpawnHitVFX(Collider other, Vector3 hitLocation)
        {
            if (GameManager.instance.player.GetCombatManager().hitFX != null) // weaponData.hitVFX != null)
            {
                //Vector3 hitNormal = other.ClosestPoint(hitPoint.position);
                Instantiate(GameManager.instance.player.GetCombatManager().hitFX, hitLocation, Quaternion.identity);
            }
        }

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
        }

        private void SpawnVFX(Collider collision)
        {
            Debug.Log("SpawnVFX");
            Vector3 hitPosition = collision.ClosestPoint(transform.position);
            Vector3 hitNormal = (hitPosition - collision.transform.position).normalized;

            if (hitNormal == Vector3.zero)
                hitNormal = -transform.forward;

            Quaternion rotation = Quaternion.LookRotation(hitNormal);

            if (vfx != null) //owner.GetComponent<CombatManager>().hitFX != null)
            {
                Debug.Log("found hitVFX");
                GameObject
                    hitVfx = Instantiate(vfx, hitPosition,
                        rotation); // GameManager.instance.player.GetCombatManager().hitFX

                var particle = hitVfx.GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    particle.Play();
                    Destroy(hitVfx, particle.main.duration);
                }
                else
                {
                    Destroy(hitVfx, 2f);
                }
            }
        }
    }
}