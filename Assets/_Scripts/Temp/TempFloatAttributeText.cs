using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TempFloatAttributeText : MonoBehaviour
{
    public TextMeshProUGUI statText;
    public TempPlayerStats attribute;
    public GameplayAttribute attributeToListen;

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
        var GAC = GameManager.instance.player.GetComponent<GameplayAttributeComponent>();
        if (GAC != null)
        {
            this.statText.text = GAC.GetRuntimeAttribute(attributeToListen).CurrentValue.ToString(); 
            //TempPlayerAttributes.instance.GetFloatAttribute(attribute).ToString();
        }
    }
}