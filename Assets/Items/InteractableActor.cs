using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class InteractableActor : MonoBehaviour
{
    [SerializeField] Item item;
    [SerializeField] public bool pressToInteract = false;

    void Awake()
    {
        this.GetComponent<MeshFilter>().mesh = item.mesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pressToInteract) return;
        else
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Interact();
            }
        }
    }

    public void Interact()
    {
        item.Interact();
        Destroy(this.gameObject);
    }
}