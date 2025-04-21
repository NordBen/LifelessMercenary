using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ExtraUIElements : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private BaseCharacter owner;
    [SerializeField] private bool followTarget; // bool for if the health bar should follow an object in world space or is static on the HUD
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private string additionalText = "";

    // Private variables
    private TMP_Text extraText;
    private int value = 0;
    private string textString = "";

    void Start()
    {
        extraText = GetComponent<TMP_Text>(); // gets text component
        UpdateTextUI(owner.GetMaxHealingItems()); // initiates the UIText to Max allowed healing items of owning character
    }

    private void OnEnable()
    {
        owner.OnHealingUsed += UpdateTextUI; // subscribes to HPChange event of owner
    }

    private void OnDisable()
    {
        owner.OnHealingUsed -= UpdateTextUI; // unsubscribes from HPChange event of owner
    }

    private void Update()
    {
        // makes sure the healthbar is looking at the camera if in worldspace like for NPC's
        if (this.followTarget)
        {
            transform.rotation = Camera.main.transform.rotation;
            transform.position = owner.transform.position + targetOffset;
        }
    }

    private void UpdateTextUI(int newValue)
    {
        // updates the local variable in the class to the new value from the OnHealingUsed event,
        // then sets the textString to be correctly formatted, sets the extraText to textString
        this.value = newValue;
        textString = $"{additionalText} {value} / {owner.GetMaxHealingItems()}";
        extraText.text = textString;
    }
}