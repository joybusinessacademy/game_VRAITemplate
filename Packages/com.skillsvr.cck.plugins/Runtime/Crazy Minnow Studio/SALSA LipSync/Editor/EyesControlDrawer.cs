using UnityEditor;
using UnityEngine;

namespace CrazyMinnow.SALSA.Timeline
{
    [CustomPropertyDrawer(typeof(EyesControlBehavior))]
    public class EyesControlDrawer : PropertyDrawer
    {
        private float excludeButtonHeight = 15f;
        private InspectorCommon.AlertType excludeButtonColor = InspectorCommon.AlertType.Warning;

        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            // Find the clip this behaviour is associated with
            EyesControl clip = property.serializedObject.targetObject as EyesControl;
            if (!clip) return;
            var clipTemplate = clip.template;
            if (clipTemplate.trackBinding == null) return;

            clipTemplate.clipName = EditorGUILayout.TextField(new GUIContent("Clip Name", "Apply a unique name to the Timeline clip."),
                clipTemplate.clipName);

            GUILayout.Space(15f);

            #region +++++ Blink Settings +++++

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            clipTemplate.manualBlink = GUILayout.Toggle(clipTemplate.manualBlink,
                new GUIContent("Trigger Blink", "Enable to produce a blink."));
            if (clipTemplate.manualBlink)
            {
                clipTemplate.blinkCustomTiming =
                    EditorGUILayout.Toggle("Custom Timing", clipTemplate.blinkCustomTiming);

                if (clipTemplate.blinkCustomTiming)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(15f);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    clipTemplate.blinkON = EditorGUILayout.FloatField("On", clipTemplate.blinkON);
                    clipTemplate.blinkHOLD = EditorGUILayout.FloatField("Hold", clipTemplate.blinkHOLD);
                    clipTemplate.blinkOFF = EditorGUILayout.FloatField("Off", clipTemplate.blinkOFF);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(15f);

            #region +++++ Exclude Button Shortcuts +++++
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(new GUIContent("Exclusion Settings:", "Exclusions prevent a particular setting from being processed. To enable an individual setting exclusion, click the box right-aligned with the desired setting. When a box is yellow, the setting is excluded."));
            if (InspectorCommon.DrawItemButton("All", "Enables all exclusion options.", 40f))
            {
                clipTemplate.excludeLookTarget = true;
                clipTemplate.excludeUseAffinity = true;
                clipTemplate.excludeEnableHead = true;
                clipTemplate.excludeEnableEyes = true;
                clipTemplate.excludeEnableBlink = true;
                clipTemplate.excludeEnableTrack = true;
                clipTemplate.excludeSetHeadRandom = true;
                clipTemplate.excludeSetEyesRandom = true;
                clipTemplate.excludeSetBlinkRandom = true;
            }
            GUILayout.Space(5f);
            if (InspectorCommon.DrawItemButton("None", "Disables all exclusion options.", 40f))
            {
                clipTemplate.excludeLookTarget = false;
                clipTemplate.excludeUseAffinity = false;
                clipTemplate.excludeEnableHead = false;
                clipTemplate.excludeEnableEyes = false;
                clipTemplate.excludeEnableBlink = false;
                clipTemplate.excludeEnableTrack = false;
                clipTemplate.excludeSetHeadRandom = false;
                clipTemplate.excludeSetEyesRandom = false;
                clipTemplate.excludeSetBlinkRandom = false;
            }
            EditorGUILayout.EndHorizontal();
            #endregion -- exclude button shortcuts

            #region +++++ Enable/Disable Settings +++++

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Enable/Disable Modules");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // --------------
                EditorGUILayout.BeginHorizontal();
                clipTemplate.enableHead = GUILayout.Toggle(clipTemplate.enableHead,
                    new GUIContent("Head" + (clipTemplate.excludeEnableHead ? "\t (Excluded)":""), "Enable/disable head module."));

                if (clipTemplate.excludeEnableHead)
                    InspectorCommon.DrawBackgroundCondition(excludeButtonColor);
                Rect rHead = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20f));
                if (GUI.Button(rHead, GUIContent.none))
                    clipTemplate.excludeEnableHead = !clipTemplate.excludeEnableHead;
                GUILayout.Space(excludeButtonHeight);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                InspectorCommon.DrawResetBg();
                // --------------
                EditorGUILayout.BeginHorizontal();
                clipTemplate.enableEyes = GUILayout.Toggle(clipTemplate.enableEyes,
                    new GUIContent("Eyes" + (clipTemplate.excludeEnableEyes ? "\t (Excluded)":""), "Enable/disable eyes module."));

                if (clipTemplate.excludeEnableEyes)
                    InspectorCommon.DrawBackgroundCondition(excludeButtonColor);
                Rect rEyes = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20f));
                if (GUI.Button(rEyes, GUIContent.none))
                    clipTemplate.excludeEnableEyes = !clipTemplate.excludeEnableEyes;
                GUILayout.Space(excludeButtonHeight);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                InspectorCommon.DrawResetBg();
                // --------------
                EditorGUILayout.BeginHorizontal();
                clipTemplate.enableBlink = GUILayout.Toggle(clipTemplate.enableBlink,
                    new GUIContent("Blink" + (clipTemplate.excludeEnableBlink ? "\t (Excluded)":""), "Enable/disable blink module."));

                if (clipTemplate.excludeEnableBlink)
                    InspectorCommon.DrawBackgroundCondition(excludeButtonColor);
                Rect rBlnk = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20f));
                if (GUI.Button(rBlnk, GUIContent.none))
                    clipTemplate.excludeEnableBlink = !clipTemplate.excludeEnableBlink;
                GUILayout.Space(excludeButtonHeight);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                InspectorCommon.DrawResetBg();
                // --------------
                EditorGUILayout.BeginHorizontal();
                clipTemplate.enableTrack = GUILayout.Toggle(clipTemplate.enableTrack,
                    new GUIContent("Lids" + (clipTemplate.excludeEnableTrack ? "\t (Excluded)":""), "Enable/disable lid tracking module."));

                if (clipTemplate.excludeEnableTrack)
                    InspectorCommon.DrawBackgroundCondition(excludeButtonColor);
                Rect rTrk = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20f));
                if (GUI.Button(rTrk, GUIContent.none))
                    clipTemplate.excludeEnableTrack = !clipTemplate.excludeEnableTrack;
                GUILayout.Space(excludeButtonHeight);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                InspectorCommon.DrawResetBg();
                // --------------

            GUILayout.Space(5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(15f);

            #region +++++ Random Settings +++++

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Enable/Disable Random");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // --------------
                EditorGUILayout.BeginHorizontal();
                clipTemplate.setHeadRandom = GUILayout.Toggle(clipTemplate.setHeadRandom,
                    new GUIContent("Head" + (clipTemplate.excludeSetHeadRandom ? "\t (Excluded)":""),   "Enable/disable head module."));

                if (clipTemplate.excludeSetHeadRandom)
                    InspectorCommon.DrawBackgroundCondition(excludeButtonColor);
                Rect rRhead = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20f));
                if (GUI.Button(rRhead, GUIContent.none))
                    clipTemplate.excludeSetHeadRandom = !clipTemplate.excludeSetHeadRandom;
                GUILayout.Space(excludeButtonHeight);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                InspectorCommon.DrawResetBg();
                // --------------
                EditorGUILayout.BeginHorizontal();
                clipTemplate.setEyesRandom = GUILayout.Toggle(clipTemplate.setEyesRandom,
                    new GUIContent("Eyes" + (clipTemplate.excludeSetEyesRandom ? "\t (Excluded)":""), "Enable/disable eyes module."));

                if (clipTemplate.excludeSetEyesRandom)
                    InspectorCommon.DrawBackgroundCondition(excludeButtonColor);
                Rect rReyes = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20f));
                if (GUI.Button(rReyes, GUIContent.none))
                    clipTemplate.excludeSetEyesRandom = !clipTemplate.excludeSetEyesRandom;
                GUILayout.Space(excludeButtonHeight);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                InspectorCommon.DrawResetBg();
                // --------------
                EditorGUILayout.BeginHorizontal();
                clipTemplate.setBlinkRandom = GUILayout.Toggle(clipTemplate.setBlinkRandom,
                    new GUIContent("Blink" + (clipTemplate.excludeSetBlinkRandom ? "\t (Excluded)":""),  "Enable/disable blink module."));

                if (clipTemplate.excludeSetBlinkRandom)
                    InspectorCommon.DrawBackgroundCondition(excludeButtonColor);
                Rect rRblk = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20f));
                if (GUI.Button(rRblk, GUIContent.none))
                    clipTemplate.excludeSetBlinkRandom = !clipTemplate.excludeSetBlinkRandom;
                GUILayout.Space(excludeButtonHeight);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                InspectorCommon.DrawResetBg();
                // --------------

            GUILayout.Space(5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(15f);

            #region +++++ Reset Clip End +++++

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            clipTemplate.resetAtClipEnd = GUILayout.Toggle(clipTemplate.resetAtClipEnd,
                new GUIContent("Reset on Clip End", "Reset settings after clip ends."));

            #region +++++ reset on clip end notification +++++
            InspectorCommon.DrawBackgroundCondition(InspectorCommon.AlertType.Normal);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            string message;
            if (clipTemplate.resetAtClipEnd)
                message = "All settings will be stored prior to implementing these changes and will be restored at clip end. Size of clip determines the reset point.";
            else
                message = "All settings will remain after clip end. Settings will NOT be reset; therefore, the size of the clip on the timeline does not matter.";
            EditorGUILayout.LabelField(message, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndVertical();
            GUILayout.Space(5f);
            EditorGUILayout.EndHorizontal();
            InspectorCommon.DrawResetBg();
            #endregion -- reset clip end notification

            GUILayout.Space(5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            #endregion -- reset clip end

            GUILayout.Space(5f);

            #region +++++ debug event calls +++++

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            clipTemplate.isDebug = GUILayout.Toggle(clipTemplate.isDebug,
                new GUIContent("Debug Event Calls", ""));
            EditorGUILayout.EndHorizontal();

            #endregion -- debug event calls

            GUILayout.Space(15f);

            #region +++++ Look Target Settings +++++
            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(15f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                        EditorGUI.indentLevel--;
                        EditorGUILayout.LabelField(new GUIContent("LookTarget\t" + (clipTemplate.excludeLookTarget ? "(Excluded)" : ""),
                            "LookTarget references are now managed by Unity's ExternalReference look-ups to ensure the value is stored between Unity and scene loads; therefore, the Inspector slot is also managed by that system and exists below."));
                        EditorGUI.indentLevel++;
                        if (clipTemplate.excludeLookTarget)
                            InspectorCommon.DrawBackgroundCondition(excludeButtonColor);
                        Rect rLook = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20f));
                            if (GUI.Button(rLook, GUIContent.none))
                                clipTemplate.excludeLookTarget = !clipTemplate.excludeLookTarget;
                            GUILayout.Space(excludeButtonHeight);
                        EditorGUILayout.EndVertical();
                        InspectorCommon.DrawResetBg();
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5f);

                    EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(15f);
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUILayout.Space(5f);
                            clipTemplate.useAffinity = GUILayout.Toggle(clipTemplate.useAffinity,
                                new GUIContent("Affinity\t" + (clipTemplate.excludeUseAffinity ? " (Excluded)":""),
                                    "Enable to use LookTarget Affinity. Affinity is now visible whether LookTarget is set or not; however, it is only applicable if LookTarget is configured on the Eyes component (here or externally)."));
                            if (clipTemplate.useAffinity && !clipTemplate.excludeUseAffinity)
                            {
                                clipTemplate.percentage = EditorGUILayout.FloatField("%", clipTemplate.percentage);
                                clipTemplate.timerMin = EditorGUILayout.FloatField("Min Timer", clipTemplate.timerMin);
                                clipTemplate.timerMax = EditorGUILayout.FloatField("Max Timer", clipTemplate.timerMax);
                            }
                            GUILayout.Space(5f);
                        EditorGUILayout.EndVertical();

                        if (clipTemplate.excludeUseAffinity)
                            InspectorCommon.DrawBackgroundCondition(excludeButtonColor);
                        Rect rAff = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(20f));
                            if (GUI.Button(rAff, GUIContent.none))
                                clipTemplate.excludeUseAffinity = !clipTemplate.excludeUseAffinity;
                            GUILayout.Space(excludeButtonHeight);
                        EditorGUILayout.EndVertical();
                        InspectorCommon.DrawResetBg();

                    EditorGUILayout.EndHorizontal();

                    GUILayout.Space(5f);
                    GUILayout.Space(5f);

                EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            #endregion -- look target settings


            // Assume that the currently selected object is the internal class UnityEditor.Timeline.EditorClip
            // this gives you access to the clip start, duration etc.
            SerializedObject editorGUI = new SerializedObject(Selection.activeObject);

            // Grab the clip title, set new title
            SerializedProperty title = editorGUI.FindProperty("m_Clip.m_DisplayName");
            if (title == null)
                title = editorGUI.FindProperty("m_Item.m_DisplayName");

            if (title != null)
            {
                title.stringValue = "> " + clipTemplate.clipName + (clipTemplate.resetAtClipEnd ? " <" : "");
            }

            editorGUI.ApplyModifiedProperties();
        }
    }
}