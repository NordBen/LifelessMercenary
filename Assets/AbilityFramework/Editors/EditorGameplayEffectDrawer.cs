using UnityEditor;

[CustomEditor(typeof(GameplayEffect))]
public class GameplayEffectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameplayEffect effect = (GameplayEffect)target;
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("effectName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("targetAttribute"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("modifierType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("durationType"));

        if (effect.durationType != EEffectDurationType.Instant)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("period"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("valueStrategy"));

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("stackingType"));

        if (serializedObject.FindProperty("stackingType").enumValueIndex != (int)EEffectStackingType.None)
        {
//            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStacks"));
        }
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("requiredTags"));

        serializedObject.ApplyModifiedProperties();
    }
}