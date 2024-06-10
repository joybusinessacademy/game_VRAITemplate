using UnityEditor;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool.Editor.TimelineExtensions
{
    [CustomEditor(typeof(LegacyTextPlayableBehaviour))]
    public class LegacyTextPlayableBehaviourEditor : UnityEditor.Editor
    {
        protected SerializedProperty colorProperty;
        protected SerializedProperty textProperty;
        private void OnEnable()
        {
            colorProperty = serializedObject.FindProperty(nameof(LegacyTextPlayableBehaviour.color));
            textProperty = serializedObject.FindProperty(nameof(LegacyTextPlayableBehaviour.text));
        }
        public override void OnInspectorGUI()
        {

            EditorGUILayout.PropertyField(colorProperty);
            EditorGUILayout.PropertyField(textProperty);

        }
    }
}
