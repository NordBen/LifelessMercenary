using UnityEngine;

public class ExtendedUIElement : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] protected BaseCharacter owner;
    [SerializeField] private bool followTarget; // bool for if the health bar should follow an object in world space or is static on the HUD
    [SerializeField] private Vector3 targetOffset;

    private void Update()
    {
        // makes UI follow the camera
        if (this.followTarget)
        {
            transform.rotation = Camera.main.transform.rotation;
            transform.position = owner.transform.position + targetOffset;
        }
    }
}