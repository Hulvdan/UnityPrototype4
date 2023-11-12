using BFG.Runtime;
using UnityEditor;
using UnityEngine;

namespace Editor {
[CustomPropertyDrawer(typeof(GStringKey))]
public class GStringKeyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(
            position, GUIUtility.GetControlID(FocusType.Passive), label
        );

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var labelRect = new Rect(position.x, position.y, 240, position.height);
        var keyProperty = property.FindPropertyRelative(nameof(GStringKey.Key));
        EditorGUI.DelayedTextField(labelRect, keyProperty, GUIContent.none);

        if (GUILayout.Button("Change")) {
            var w = ScriptableWizard.DisplayWizard<GStringKeySelectorWindow>(
                "Choose Translation Key"
            );

            w.InitialKey = keyProperty.stringValue;
            w.OnKeyChanged += key => {
                keyProperty.stringValue = key;
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
            };
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
}
