using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AttributeUpgradable : AttributeText
{
    [SerializeField] private string text = string.Empty;
    private List<Button> buttons;

    protected void Start()
    {
        base.Start();
        if (this.transform.childCount > 1)
        {
            for (int i = 1; i < this.transform.childCount; i++)
            {
                buttons.Add(transform.GetChild(i).GetComponent<Button>());
            }
        }
    }

    private void OnClick(int value)
    {
        if (GameManager.instance.player.GetComponent<LevelUpSystem>().CanIncreaseAttribute())
            this.refAttri.SetBaseValue(Mathf.Max(0, this.refAttri.BaseValue() + value));
        else
            Debug.Log("do not have enough points to increase attribute");
        SetText(text);
    }

    private void SetText(string newString)
    {
        this.attributeText.text = newString;
    }
}