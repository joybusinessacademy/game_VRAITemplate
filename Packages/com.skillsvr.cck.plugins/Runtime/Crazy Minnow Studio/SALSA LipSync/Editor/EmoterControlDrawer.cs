using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrazyMinnow.SALSA.Timeline
{
    [CustomPropertyDrawer(typeof(EmoterControlBehavior))]
    public class EmoterControlDrawer : PropertyDrawer
    {
        enum AnimateDirection {On, Off}
        List<string> emoteOptions = new List<string>();
        bool isListPopulated = false;


        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            // Find the clip this behaviour is associated with
            EmoterControl clip = property.serializedObject.targetObject as EmoterControl;
            if (!clip) return;
            var clipTemplate = clip.template;
            if (clipTemplate.trackBinding == null) return;

            if (clipTemplate.emoteName == "")
                InspectorCommon.DrawBackgroundCondition(InspectorCommon.AlertType.Error);

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(new GUIContent("Emote Name", "Select an emote from pre-configured emotes in EmoteR."));

                var isSelected = clipTemplate.emoteName != "";
                int optionIndex = 0;

                if (!isListPopulated)
                {
                    if (!isSelected)
                        emoteOptions.Add("select Emote...");
                    foreach (var emote in clipTemplate.trackBinding.emotes)
                        emoteOptions.Add(emote.expData.name);
                    isListPopulated = true;
                }

                if (isSelected)
                {
                    emoteOptions.Remove("select Emote...");
                    optionIndex = emoteOptions.FindIndex(n => n.Contains(clipTemplate.emoteName));
                }

                EditorGUI.BeginChangeCheck();
                int selection = EditorGUILayout.Popup(optionIndex, emoteOptions.ToArray());
                if (EditorGUI.EndChangeCheck())
                {
                    clipTemplate.emoteName = emoteOptions[selection];
                }

            EditorGUILayout.EndHorizontal();
            InspectorCommon.DrawResetBg();

            if (isSelected)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                clipTemplate.handler = (ExpressionComponent.ExpressionHandler) EditorGUILayout.EnumPopup(new GUIContent("Emote Handler", "Select round-trip or one-way handling."), (Enum) clipTemplate.handler);

                if (clipTemplate.handler == ExpressionComponent.ExpressionHandler.OneWay)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    EditorGUILayout.BeginVertical();
                    AnimateDirection animateDirection = clipTemplate.isAnimatingOn ? AnimateDirection.On : AnimateDirection.Off;
                    clipTemplate.isAnimatingOn = (AnimateDirection) EditorGUILayout.EnumPopup(
                        new GUIContent("Animate Direction", "Choose animation direction On/Off."),
                        (Enum) animateDirection) == AnimateDirection.On;

                    if (clipTemplate.isAnimatingOn)
                    {
                        GUILayout.Space(5f);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15f);
                        EditorGUILayout.BeginVertical();
                        clipTemplate.dynamics = GUILayout.Toggle(clipTemplate.dynamics,
                            new GUIContent("Apply Dynamics", "Enable to apply dynamics variations to the emote."));

                        if (clipTemplate.dynamics)
                        {
                            GUILayout.Space(5f);
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(15f);
                            EditorGUILayout.BeginVertical();
                            clipTemplate.dynamicsAmount =
                                EditorGUILayout.Slider("Dynamics Amount", clipTemplate.dynamicsAmount, 0.0f, 1.0f);
                            clipTemplate.dynamicsTiming =
                                EditorGUILayout.Slider("Dynamics Timing", clipTemplate.dynamicsTiming, 0.0f, 5.0f);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }


                GUILayout.Space(15f);
                InspectorCommon.DrawBackgroundCondition(InspectorCommon.AlertType.Warning);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                string message;
                if (clipTemplate.handler == ExpressionComponent.ExpressionHandler.RoundTrip)
                    message = "The emote will animate On then Off, based on the length of the emote clip.";
                else
                    message = "The emote will animate "
                              + (clipTemplate.isAnimatingOn ? "ON" : "OFF")
                              + ", triggering on the leading edge of the clip. \n\nNOTE: The size of the clip does not matter.";
                EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();
                InspectorCommon.DrawResetBg();

                GUILayout.Space(5f);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                clipTemplate.isDebug = GUILayout.Toggle(clipTemplate.isDebug,
                    new GUIContent("Debug Event Calls", ""));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            // Assume that the currently selected object is the internal class UnityEditor.Timeline.EditorClip
            // this gives you access to the clip start, duration etc.
            SerializedObject editorGUI = new SerializedObject(Selection.activeObject);

            // Grab the clip title, set new title
            SerializedProperty title = editorGUI.FindProperty("m_Clip.m_DisplayName");
            if (title == null)
                title = editorGUI.FindProperty("m_Item.m_DisplayName");

            if (title != null)
            {
                title.stringValue = (clipTemplate.handler == ExpressionComponent.ExpressionHandler.OneWay ?
                         (clipTemplate.isAnimatingOn ? "> " + clipTemplate.emoteName : "< " + clipTemplate.emoteName)
                         : "> " + clipTemplate.emoteName + " <");
            }

            editorGUI.ApplyModifiedProperties();
        }
    }
}