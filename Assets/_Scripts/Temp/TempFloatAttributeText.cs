using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TempFloatAttributeText : MonoBehaviour
{
    public TextMeshProUGUI statText;
    public TempPlayerStats attribute;

    void Start()
    {
        UpdateStat();
    }

    private void Update()
    {
        UpdateStat();
    }

    protected virtual void UpdateStat()
    {
        this.statText.text = TempPlayerAttributes.instance.GetFloatAttribute(attribute).ToString();
    }
}