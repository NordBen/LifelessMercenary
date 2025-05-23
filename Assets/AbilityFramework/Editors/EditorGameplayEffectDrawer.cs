#if UNITY_EDITOR
using UnityEditor;

namespace LM.AbilitySystem
{
    [CustomEditor(typeof(GameplayEffect))]
    public class GameplayEffectEditor : Editor
    {
        private bool showModifiers = true;
        private bool showExecutions = true;

        public override void OnInspectorGUI()
        {
            GameplayEffect effect = (GameplayEffect)target;
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("durationType"));

            var durationType = (EEffectDurationType)serializedObject.FindProperty("durationType").enumValueIndex;

            if (durationType != EEffectDurationType.Instant)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("period"));
            }

            if (durationType == EEffectDurationType.Duration)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("stackingType"));

            var stackingType = (EEffectStackingType)serializedObject.FindProperty("stackingType").enumValueIndex;
            if (stackingType != EEffectStackingType.None)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStacks"));
            }

            showModifiers = EditorGUILayout.Foldout(showModifiers, "Modifiers", true);
            if (showModifiers)
            {
                EditorGUI.indentLevel++;

                SerializedProperty modifiersArray = serializedObject.FindProperty("applications");
                EditorGUILayout.PropertyField(modifiersArray, true);

                for (int i = 0; i < modifiersArray.arraySize; i++)
                {
                    var appProp = modifiersArray.GetArrayElementAtIndex(i);
                    var valueStrategyProp = appProp.FindPropertyRelative("valueStrategy");

                    // If we add a new element, create a new instance of the strategy
                    if (i > 0 && valueStrategyProp.managedReferenceValue != null)
                    {
                        var prevAppProp = modifiersArray.GetArrayElementAtIndex(i - 1);
                        var prevValueStrategyProp = prevAppProp.FindPropertyRelative("valueStrategy");

                        // If this element has the same reference as the previous one, clone it
                        if (valueStrategyProp.managedReferenceValue == prevValueStrategyProp.managedReferenceValue)
                        {
                            // We need to use reflection to clone the strategy properly
                            // This is a simplified example, you might need additional logic based on your strategy types
                            var strategy = valueStrategyProp.managedReferenceValue;
                            var clone = System.Activator.CreateInstance(strategy.GetType());

                            // Copy properties from strategy to clone
                            foreach (var field in strategy.GetType().GetFields())
                            {
                                field.SetValue(clone, field.GetValue(strategy));
                            }

                            // Assign the clone back to the property
                            effect.applications[i].valueStrategy = (IAttributeMagnitudeStrategy)clone;
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }

            showExecutions = EditorGUILayout.Foldout(showExecutions, "Executions", true);
            if (showExecutions)
            {
                EditorGUI.indentLevel++;

                SerializedProperty execsArray = serializedObject.FindProperty("executions");
                EditorGUILayout.PropertyField(execsArray, true);

                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif