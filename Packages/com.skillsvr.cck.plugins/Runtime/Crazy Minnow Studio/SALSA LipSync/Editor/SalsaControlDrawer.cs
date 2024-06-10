using UnityEditor;
using UnityEngine;

namespace CrazyMinnow.SALSA.Timeline
{
    [CustomPropertyDrawer(typeof(SalsaControlBehavior))]
    public class SalsaControlDrawer : PropertyDrawer
    {
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            // Find the clip this behaviour is associated with
            SalsaControl clip = property.serializedObject.targetObject as SalsaControl;
            if (!clip) return;
            var clipTemplate = clip.template;
            if (clipTemplate.trackBinding == null) return;

            if (!clipTemplate.overrideDynamics)
            {
                clipTemplate.advDynamicsBias = clipTemplate.trackBinding.advDynPrimaryBias;
                clipTemplate.globalDynamics = clipTemplate.trackBinding.globalFrac;
            }

            if (clipTemplate.audioClip == null)
                InspectorCommon.DrawBackgroundCondition(InspectorCommon.AlertType.Error);

            GUILayout.BeginVertical(EditorStyles.helpBox);
            clipTemplate.audioClip = (AudioClip) EditorGUILayout.ObjectField("Audio Clip", (UnityEngine.Object) clipTemplate.audioClip, typeof (AudioClip), true);
            GUILayout.EndVertical();
            InspectorCommon.DrawResetBg();

            if (clipTemplate.audioClip == null)
                return;

            GUILayout.Space(5f);
            GUILayout.BeginVertical(EditorStyles.helpBox);
            clipTemplate.overrideDynamics = GUILayout.Toggle(clipTemplate.overrideDynamics,
                new GUIContent("Override Dynamics", "Enable to override dynamics settings."));
            if (!clipTemplate.overrideDynamics)
                InspectorCommon.DrawBackgroundCondition(InspectorCommon.AlertType.Warning);
            GUILayout.BeginVertical(
                new GUIContent("", !clipTemplate.overrideDynamics ? "Enable Override to adjust settings." : ""),
                EditorStyles.helpBox);
            clipTemplate.advDynamicsBias =
                EditorGUILayout.Slider("Advanced Dynamics Bias", clipTemplate.advDynamicsBias, 0.0f, 1f);
            clipTemplate.globalDynamics =
                EditorGUILayout.Slider("Global Dynamics", clipTemplate.globalDynamics, 0.0f, 1f);
            GUILayout.EndVertical();
            GUILayout.EndVertical();

            GUILayout.Space(15f);

            if (clipTemplate.overrideDynamics)
            {
                InspectorCommon.DrawBackgroundCondition(InspectorCommon.AlertType.Warning);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Override fields will be restored at clip end.");
                EditorGUILayout.EndVertical();
            }

            InspectorCommon.DrawResetBg();

            GUILayout.Space(5f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            clipTemplate.isDebug = GUILayout.Toggle(clipTemplate.isDebug,
                new GUIContent("Debug Event Calls", ""));
            EditorGUILayout.EndHorizontal();



            var audioClip = clip.template.audioClip;
            if(audioClip)
            {
                // Assume that the currently selected object is the internal class UnityEditor.Timeline.EditorClip
                // this gives you access to the clip start, duration etc.
                SerializedObject editorGUI = new SerializedObject(Selection.activeObject);

                // Grab the duration, set new duration
                SerializedProperty duration = editorGUI.FindProperty("m_Clip.m_Duration");
                if (duration == null)
                    duration = editorGUI.FindProperty("m_Item.m_Duration");

                if (duration != null)
                {
                    duration.doubleValue = (double) audioClip.samples / audioClip.frequency;
                    duration.doubleValue -= .01; // safety net for clip display -- tries to avoid double firing of clips
                }

                // Grab the clip title, set new title
                SerializedProperty title = editorGUI.FindProperty("m_Clip.m_DisplayName");
                if (title == null)
                    title = editorGUI.FindProperty("m_Item.m_DisplayName");

                if (title != null)
                {
                    title.stringValue = audioClip.name;
                }



                editorGUI.ApplyModifiedProperties();
            }


        }
    }
}