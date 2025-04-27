using UnityEngine;

public class Trap : InteractableActor
{
    [SerializeField] private Mesh trapMesh;

    void Start()
    {
        this.SetMesh(trapMesh);
    }

    protected override void HandleInteract()
    {
        Debug.LogWarning($"interact was called from this object: {this.gameObject}");
        GameManager.instance.KillPlayer();
    }
}