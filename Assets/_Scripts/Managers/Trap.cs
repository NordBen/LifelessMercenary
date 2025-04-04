using UnityEngine;

public class Trap : MonoBehaviour, IInteractable
{
    [SerializeField] private bool pressToInteract = false;
    [SerializeField] private Mesh trapMesh;

    void Start()
    {
        GetComponent<MeshFilter>().mesh = trapMesh;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning($"Interacted with {other.tag}");
        if (other.gameObject.CompareTag("Player"))
        {
            if (!this.pressToInteract)
            {
                Interact();
            }
            else
            {
                // TODO: add interact with key
                Debug.Log("Can press to interact now");
            }
        }
    }
    public void Interact()
    {
        Debug.LogWarning($"interact was called from this object: {this.gameObject}");
        GameManager.instance.KillPlayer();
    }
}