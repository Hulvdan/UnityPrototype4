using UnityEngine.AI;
using UnityEngine;
using UnityEditor;
using NavMeshPlus.Extensions;
using NavMeshPlus.Components;

namespace NavMeshPlus.Editors.Extensions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RootSources2d))]
    internal class RootSources2dEditor: Editor
    {
        SerializedProperty _rootSources;
        void OnEnable()
        {
            _rootSources = serializedObject.FindProperty("_rootSources");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var surf = target as RootSources2d;
            EditorGUILayout.HelpBox("Add GameObjects to create NavMesh form it and it's ancestors", MessageType.Info);

            EditorGUILayout.PropertyField(_rootSources);

            serializedObject.ApplyModifiedProperties();
        }
    }

}
