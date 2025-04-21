using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour
{
    [Header("Refrences")]
    public CharacterController characterController;
    [SerializeField] private Transform mainCamera;

    [Header("Dodge settings")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 1f;
    
    private float lastDodgeTime = -Mathf.Infinity;
    private bool isDodging = false;
    private Vector3 dodgeDirection;

    public InteractableActor currentInteractable;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isDodging && Time.time >= lastDodgeTime + dodgeCooldown)
        {
            StartCoroutine(Dodge());
        }

        CheckForInteractable();
        if (currentInteractable != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Call Interact");
                CallInteract();
            }
        }
    }

    private void CheckForInteractable()
    {
        Vector3 center = new Vector3(Screen.width * .5f, Screen.height * .5f, 0);
        Ray ray = mainCamera.gameObject.GetComponent<Camera>().ScreenPointToRay(center);
        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            if (hit.collider.TryGetComponent<InteractableActor>(out InteractableActor hitActor))
            {
                currentInteractable = hitActor;
            }
            else
                currentInteractable = null;
        }
    }

    private void CallInteract()
    {
        if (currentInteractable.pressToInteract)
        {
            currentInteractable.Interact();
        } else
            Debug.Log("touch object instead");
    }

    private IEnumerator Dodge()
    {
        isDodging = true;
        lastDodgeTime = Time.time;

        float startTime = Time.time;
        dodgeDirection = -transform.forward;

        while (Time.time < startTime + dodgeDuration)
        {
            characterController.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);
            yield return null;
        }

        isDodging = false;
    }
}