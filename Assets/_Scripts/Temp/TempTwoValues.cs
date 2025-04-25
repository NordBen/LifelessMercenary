using UnityEngine;

public class TempTwoValues : TempFloatAttributeText
{
    public TempPlayerStats otherAttribute;
    protected override void UpdateStat()
    {
        this.statText.text = $"{TempPlayerAttributes.instance.GetFloatAttribute(attribute)} / {TempPlayerAttributes.instance.GetFloatAttribute(otherAttribute)}";
    }
}