using UnityEngine;

namespace LM.AbilitySystem
{
    public class ApplyEffectActor : MonoBehaviour
    {
        [SerializeField] private GameplayEffect effect;

        private void OnTriggerEnter(Collider other)
        {
            var attributeComponent = other.GetComponent<GameplayAttributeComponent>();
            if (attributeComponent != null)
            {
                attributeComponent.ApplyEffect(effect);
                Destroy(this.gameObject);
            }
        }
    }
}