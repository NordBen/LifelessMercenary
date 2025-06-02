using UnityEngine;

namespace LM
{
    public class ParrySystem : MonoBehaviour
    {
        private static readonly int ParryHash = Animator.StringToHash("tParry");
        private static readonly int ParriedHash = Animator.StringToHash("tParried");
        
        [SerializeField] private float parryRange = 2;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private AudioClip parrySound;

        [SerializeField] private Collider[] lastHitColliders;
        private Animator m_animator;
        private bool m_canParry;

        private void Start()
        {
            m_animator = GetComponent<Animator>();
        }

        public void StartParry()
        {
            m_canParry = true;
        }

        public void EndParry()
        {
            m_canParry = false;
        }

        public bool TryParry()
        {
            if (!m_canParry)
            {
                Debug.Log("cannot parry");
                return false;
            }
            
            lastHitColliders = Physics.OverlapSphere(transform.position, parryRange, enemyLayer);
            foreach (var hitCollider in lastHitColliders)
            {
                if (hitCollider.gameObject == gameObject) continue;
                
                var enemyAnimator = hitCollider.GetComponent<Animator>();
                if (enemyAnimator) enemyAnimator.SetTrigger(ParriedHash);
            }
            
            EndParry();
            return true;
        }

        public void PerformParry()
        {
            m_animator.SetTrigger(ParryHash);
        }
        
        private void OnValidate()
        {
            // Ensure we're not on the enemy layer ourselves
            if (((1 << gameObject.layer) & enemyLayer.value) != 0)
            {
                Debug.LogWarning(
                    $"CombatManager on {gameObject.name} is on a layer included in its enemyLayer mask. This might cause self-detection issues.");
            }
        }

        private void OnDrawGizmos()
        {
            if (m_canParry)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, parryRange);
            }
        }
    }
}