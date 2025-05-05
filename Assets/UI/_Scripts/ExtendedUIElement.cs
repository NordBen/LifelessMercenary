using UnityEngine;

public class ExtendedUIElement : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] protected BaseCharacter owner;
    [SerializeField] protected GameObject _followTarget;
    [SerializeField] private bool followTarget;
    [SerializeField] private bool dirtyFollow;
    [SerializeField] private Vector3 targetOffset;

    private void Update()
    {
        // makes UI follow the camera
        if (this.followTarget)
        {
            transform.rotation = Camera.main.transform.rotation;
            if (!dirtyFollow)
                transform.position = owner.transform.position + targetOffset;
            else
                transform.position = _followTarget.transform.position + targetOffset;
        }
    }
}