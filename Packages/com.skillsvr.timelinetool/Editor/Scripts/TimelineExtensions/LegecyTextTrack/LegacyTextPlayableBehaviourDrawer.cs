using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool.Editor.TimelineExtensions
{
    [CustomPropertyDrawer(typeof(LegacyTextPlayableBehaviour))]
    public class LegacyTextPlayableBehaviourDrawer : PropertyDrawer
    {
        protected SerializedProperty colorProperty;
        protected SerializedProperty textProperty;
        private void OnEnable()
        {
        }
        string t;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            EditorGUI.BeginProperty(position,  label, property);
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property.FindPropertyRelative(nameof(LegacyTextPlayableBehaviour.color)));

            position.y += position.height;
            

            position = EditorGUI.PrefixLabel(position, new GUIContent(nameof(LegacyTextPlayableBehaviour.text)));

            position.x -= 10;
            position.width -= 10;
            position.height = EditorGUIUtility.singleLineHeight * 6;
            EditorGUI.BeginChangeCheck();

            var textProp = property.FindPropertyRelative(nameof(LegacyTextPlayableBehaviour.text));
            textProp.stringValue = EditorGUI.TextArea(position, textProp.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 7;
        }
    }
}
