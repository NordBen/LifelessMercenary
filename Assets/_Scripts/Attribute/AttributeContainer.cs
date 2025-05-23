using System.Collections.Generic;
using LM;
using UnityEngine;

public class AttributeContainer : MonoBehaviour
{
    public Dictionary<string, Attribute> attributes;
    [SerializeField] private List<Attribute> bindedAttributes;
    public List<AttributeModifier> activeModifiers;
    public List<AttributeToInit> attributeinits;

    private void Awake()
    {
        attributes = new();
        bindedAttributes = new();
        activeModifiers = new();

        foreach (var attri in attributeinits)
        {
            attributes.Add(attri.name, new Attribute(attri.value));
        }

        SearchForAttribute("Max");
        /*
        foreach (var attri in attributeinits)
        {
            if (attri.bind)
            {
                Attribute primary = attributes[attri.bindTo];
                Attribute binding = attributes[attri.name];

                if (!bindedAttributes.ContainsKey(primary))
                    bindedAttributes[primary] = new List<Attribute>();

                bindedAttributes[primary].Add(binding);
            }
        }*/
        /*
        foreach (var pair in bindedAttributes)
        {
            float primaryValue = pair.Key.CurrentValue();
            float multiplier = 25f;
            float newValue = primaryValue * multiplier;

            foreach (var dependentAttribute in pair.Value)
                dependentAttribute.AddToModifiedValue(newValue);
        }*/
    }

    private void SearchForAttribute(string keyword)
    {
        foreach (var entry in attributes.Keys)
        {
            if (entry.StartsWith(keyword))
            {
                string withoutPrefix = entry.Substring(keyword.Length);

                foreach (var equivilant in attributes.Keys)
                {
                    if (equivilant == withoutPrefix)
                    {
                        bindedAttributes.Add(attributes[equivilant]);
                        Debug.Log($"{equivilant}");
                    }
                    //break;
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            PrintAllAttributes();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            ApplyNewMod(Random.Range(1, 81), "Health", Random.Range(3, 11));
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            ApplyNewMod(Random.Range(1, 81), "Health", 0);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            TakeDamage(10);
        }
    }

    public void AddMod(AttributeModifier mod)
    {
        if (attributes.ContainsKey(mod.attributeToModify))
        {
            attributes[mod.attributeToModify].AddModifier(mod);
            activeModifiers.Add(mod);
        }
    }

    public void ApplyBuff(string attributeName, AttributeModifier buff)
    {
        if (!attributes.ContainsKey(attributeName)) return;

        Attribute attribute = attributes[attributeName];
        activeModifiers.Add(buff);

        if (buff.duration > 0)
        {
            StartCoroutine(buff.ApplyBuff(attribute, RemoveBuff));
        }
        else
        {
            attribute.AddModifier(buff);
        }
    }

    private void RemoveBuff(AttributeModifier buff)
    {
        activeModifiers.Remove(buff);
    }

    void PrintAllAttributes()
    {
        string heldAttributes = "Held Attributes: ";
        foreach (var pair in attributes)
        {
            heldAttributes += $"{pair.Key} - {pair.Value.BaseValue()}/{pair.Value.CurrentValue()},";
        }
        Debug.Log(heldAttributes);
        /*
        string bindedAttributesString = "Binded Attributes: ";
        foreach (var pair in bindedAttributes)
        {
            string primaryAttribute = $"{pair.Key} - {pair.Key.BaseValue()}/{pair.Key.CurrentValue()}";

            string dependentAttribute = "";
            foreach (var attr in pair.Value)
            {
                dependentAttribute += $"   {attr} - {attr.BaseValue()}/{attr.CurrentValue()}";
            }

            bindedAttributesString += $"{primaryAttribute} - {dependentAttribute}\n";
        }
        Debug.Log(bindedAttributesString);*/
    }

    public void ApplyNewMod(float value, string attribute, float dur)
    {
        ApplyBuff(attribute, new AttributeModifier(value, attribute, EModifierType.Add, EModifierDuration.Instant, dur));
        Debug.Log("applied new modifier");
    }

    public void TakeDamage(float incomingDamage) 
    {
        attributes["Health"].SetBaseValue(Mathf.Max(0, attributes["Health"].BaseValue() - incomingDamage));

        if (attributes["Health"].CurrentValue() == 0)
            Die();
    }

    public void Die()
    {
        if (this.transform.root.name == "Player")
            GameManager.instance.player.GetComponent<LevelUpSystem>().CallOnLevelUp();
    }
}

[System.Serializable]
public class AttributeToInit
{
    public string name;
    public float value;
    public bool bind;
    public string bindTo;
}