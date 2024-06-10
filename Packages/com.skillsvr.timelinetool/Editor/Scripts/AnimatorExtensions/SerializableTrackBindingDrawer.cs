using SkillsVR.TimelineTool.AnimatorTimeline;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace SkillsVR.TimelineTool.Editor.AnimatorTimeline
{
    [CustomPropertyDrawer(typeof(SerializableTrackBinding))]
    public class SerializableTrackBindingDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var assetProp = property.FindPropertyRelative(nameof(SerializableTrackBinding.timelineAsset));
            var trackProp = property.FindPropertyRelative(nameof(SerializableTrackBinding.trackAsset));
            var valueProviderProp = property.FindPropertyRelative(nameof(SerializableTrackBinding.valueProvider));


            bool prevEnabled = GUI.enabled;
            GUI.enabled = false;
            position.height = EditorGUIUtility.singleLineHeight;

            EditorGUI.PropertyField(position, assetProp, GUIContent.none, true);

            position.x += 20;
            position.y += position.height;
            string trackName = null == trackProp.objectReferenceValue ? "Null Track" : trackProp.objectReferenceValue.name;
            EditorGUI.PrefixLabel(position, new GUIContent(trackName));
            GUI.enabled = prevEnabled;

            EditorGUI.PropertyField(position, valueProviderProp, GUIContent.none, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;
            var valueProviderProp = property.FindPropertyRelative(nameof(SerializableTrackBinding.valueProvider));
            height += EditorGUI.GetPropertyHeight(valueProviderProp, true);
            return height;
        }

    }
}
