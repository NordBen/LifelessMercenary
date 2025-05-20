using UnityEngine;

public class TempTwoValues : TempFloatAttributeText
{
    public TempPlayerStats otherAttribute;
    public GameplayAttribute otherAttributeToListen;
    protected override void UpdateStat()
    {
        var GAC = GameManager.instance.player.GetComponent<GameplayAttributeComponent>();
        if (GAC != null)
        {
            var statValueString = $"{GAC.GetRuntimeAttribute(attributeToListen).CurrentValue} / {GAC.GetRuntimeAttribute(otherAttributeToListen).CurrentValue}"; 
            //$"{TempPlayerAttributes.instance.GetFloatAttribute(attribute)} / {TempPlayerAttributes.instance.GetFloatAttribute(otherAttribute)}"
            this.statText.text = statValueString;
        }
    }
}