using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IAttributeValueStrategy))]
public class EffectStrategyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var strategyType = property.managedReferenceValue?.GetType();
        if (strategyType == typeof(AttributeBasedValueStrategy))
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("sourceAttribute"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("Coefficient"));
        }
        else if (strategyType == typeof(ConstantValueStrategy))
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("value"));
        }
        else if (strategyType == typeof(CurveValueStrategy))
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("curve"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("timeScale"));
        }

        EditorGUI.EndProperty();
    }
}

/*
[CustomPropertyDrawer(typeof(IAttributeValueStrategy))]
public class ValueStrategyDrawer : PropertyDrawer
{
    private static readonly string[] StrategyTypeNames = {
        "None",
        "Constant Value",
        "Curve Value",
        "Attribute Based"
    };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Get the current strategy type
        var currentType = property.managedReferenceValue?.GetType();
        int currentIndex = GetStrategyTypeIndex(currentType);

        // Create dropdown for strategy type
        var dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        int newIndex = EditorGUI.Popup(dropdownRect, "Strategy Type", currentIndex, StrategyTypeNames);

        if (newIndex != currentIndex)
        {
            property.managedReferenceValue = CreateStrategyInstance(newIndex);
            property.serializedObject.ApplyModifiedProperties();
        }

        // Draw strategy-specific fields if we have a strategy selected
        if (property.managedReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            
            Rect propertyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);

            if (currentType == typeof(ConstantValueStrategy))
            {
                var valueProp = property.FindPropertyRelative("value");
                EditorGUI.PropertyField(propertyRect, valueProp);
            }
            else if (currentType == typeof(CurveValueStrategy))
            {
                var curveProp = property.FindPropertyRelative("curve");
                EditorGUI.PropertyField(propertyRect, curveProp);
                
                propertyRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var timeScaleProp = property.FindPropertyRelative("timeScale");
                EditorGUI.PropertyField(propertyRect, timeScaleProp);
            }
            else if (currentType == typeof(AttributeBasedValueStrategy))
            {
                var sourceProp = property.FindPropertyRelative("sourceAttribute");
                EditorGUI.PropertyField(propertyRect, sourceProp);
                
                propertyRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                var coeffProp = property.FindPropertyRelative("Coefficient");
                EditorGUI.PropertyField(propertyRect, coeffProp);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private int GetStrategyTypeIndex(Type type)
    {
        if (type == null) return 0;
        if (type == typeof(ConstantValueStrategy)) return 1;
        if (type == typeof(CurveValueStrategy)) return 2;
        if (type == typeof(AttributeBasedValueStrategy)) return 3;
        return 0;
    }

    private IAttributeValueStrategy CreateStrategyInstance(int index)
    {
        return index switch
        {
            1 => new ConstantValueStrategy(),
            2 => new CurveValueStrategy(),
            3 => new AttributeBasedValueStrategy(),
            _ => null
        };
    }
}*/