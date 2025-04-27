using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public abstract class InteractableActor : MonoBehaviour
{
    [SerializeField] public bool pressToInteract = false;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    void Awake()
    {
        this._meshFilter = GetComponent<MeshFilter>();
        this._meshRenderer = GetComponent<MeshRenderer>();
    }

    protected void SetMesh(Mesh mesh, Material material = null)
    {
        this._meshFilter.mesh = mesh;
        this._meshRenderer.material = material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pressToInteract) return;
        
        Interact();
    }

    public void Interact()
    {
        HandleInteract();
        Destroy(this.gameObject);
    }

    protected virtual void HandleInteract()
    {
        
    }
}