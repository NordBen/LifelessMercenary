using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.GridLayoutGroup;

public class HealthBarElement : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private BaseCharacter owner; // subject to monitor health for
    [SerializeField] private bool followTarget; // bool for if the health bar should follow an object in world space or is static on the HUD
    [SerializeField] private Vector3 targetOffset;

    // Private variables
    private TMP_Text healthText;
    private Slider healthSlider;
    private float value = 0;
    private string healthString = "";

    private void Start()
    {
        healthSlider = GetComponent<Slider>(); // gets Slider component
        healthText = transform.GetChild(2).GetComponent<TMP_Text>(); // gets text component
        Invoke("InitHealthUI", .05f); // initiates the HealthBar to Max hp of owning character, with a slight delay because of some weird thing
                                      // with npc's healthbars not initiating correctly for all of them normally by just calling the function
    }

    private void OnEnable()
    {
        owner.OnHPChanged += UpdateHealthUI; // subscribes to HPChange event of owner
    }

    private void OnDisable()
    {
        owner.OnHPChanged -= UpdateHealthUI; // unsubscribes from HPChange event of owner
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

    private void InitHealthUI()
    {
        UpdateHealthUI(owner.GetMaxHP());
    }

    private void UpdateHealthUI(float newValue)
    {
        // updates the local variable in the class to the new value from the OnHPChange event,
        // then sets the healthString to be correctly formatted, sets the healthText to healthString
        // and starts updating the slider to have a more animated look
        this.value = newValue;
        healthString = $"{value} / {owner.GetMaxHP()}";
        healthText.text = healthString;
        StartCoroutine(SmoothTransitionSlider());
    }

    private IEnumerator SmoothTransitionSlider()
    {
        // only calculates the percent value of current hp compared to max hp of owning character,
        // compares slider value to targetted value and changes the slider smoothly moving it towards the target value
        // with an error margin of 0.01 value difference
        float targetValue = value / owner.GetMaxHP();
        while (Mathf.Abs(healthSlider.value - targetValue) > 0.01f)
        {
            healthSlider.value = Mathf.MoveTowards(healthSlider.value, targetValue, Time.deltaTime * 0.5f);
            yield return null;
        }
        healthSlider.value = targetValue; // directly sets the slider's value to make sure it ends with the correct value at the end of the coroutine/enumerator
    }
}