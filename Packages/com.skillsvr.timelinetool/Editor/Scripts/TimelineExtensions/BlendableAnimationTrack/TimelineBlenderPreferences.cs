using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

namespace SkillsVR.TimelineTool.Editor.TimelineExtensions
{
    /// <summary>
    /// Store the editor preferences for Timeline.
    /// </summary>
    [FilePath("TimelineBlenderPreferences.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class TimelineBlenderPreferences : ScriptableSingleton<TimelineBlenderPreferences>
    {
        [SerializeField]
        public float defaultEaseInDuration = 0.2f;
        public float defaultEaseOurDuration = 0.2f;
        public AnimationPlayableAsset.LoopMode defaultAnimLoopMode = AnimationPlayableAsset.LoopMode.On;

        public TrackOffset defaultTrackOffset = TrackOffset.ApplySceneOffsets;

        void OnDisable()
        {
            Save();
        }

        /// <summary>
        /// Save the timeline preferences settings file.
        /// </summary>
        public void Save()
        {
            Save(true);
        }

        internal SerializedObject GetSerializedObject()
        {
            return new SerializedObject(this);
        }
    }

    class TimelineBlenderPreferencesProvider : SettingsProvider
    {
        SerializedObject m_SerializedObject;
        SerializedProperty m_defaultEaseInDuration;
        SerializedProperty m_defaultEaseOurDuration;
        SerializedProperty m_defaultTrackOffset;

        SerializedProperty m_defaultAnimLoopMode;

        internal class Styles
        {
            public static readonly GUIContent EaseInLabel = L10n.TextContent("Ease In Duration", "The default ease in duration (in seconds) applied to animation clip.");
            public static readonly GUIContent EaseOutLabel = L10n.TextContent("Ease Our Duration", "The default ease out duration (in seconds) applied to animation clip.");
            public static readonly GUIContent TrackOffsetLabel = L10n.TextContent("Track Offset", "The default track offset for blendable animation tracks.");
            public static readonly GUIContent AnimLoopModeLabel = L10n.TextContent("Animation clip loop mode", "The default loop mode for blendable animation clip.");
            public static readonly GUIContent EditorSettingLabel = L10n.TextContent("Timeline Blender Editor Settings", "");
#if TIMELINE_FRAMEACCURATE
        public static readonly GUIContent PlaybackLockedToFrame = L10n.TextContent("Playback Locked To Frame", "Enable Frame Accurate Preview.");
#endif
        }

        public TimelineBlenderPreferencesProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null)
            : base(path, scopes, keywords)
        {
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            TimelineBlenderPreferences.instance.Save();
            m_SerializedObject = TimelineBlenderPreferences.instance.GetSerializedObject();
            m_defaultEaseInDuration = m_SerializedObject.FindProperty(nameof(TimelineBlenderPreferences.defaultEaseInDuration));
            m_defaultEaseOurDuration = m_SerializedObject.FindProperty(nameof(TimelineBlenderPreferences.defaultEaseOurDuration));
            m_defaultTrackOffset = m_SerializedObject.FindProperty(nameof(TimelineBlenderPreferences.defaultTrackOffset));
            m_defaultAnimLoopMode = m_SerializedObject.FindProperty(nameof(TimelineBlenderPreferences.defaultAnimLoopMode));
#if TIMELINE_FRAMEACCURATE
        m_PlaybackLockedToFrame = m_SerializedObject.FindProperty("m_PlaybackLockedToFrame");
#endif
        }

        public override void OnGUI(string searchContext)
        {
            m_SerializedObject.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField(Styles.EditorSettingLabel, EditorStyles.boldLabel);
            m_defaultEaseInDuration.floatValue = EditorGUILayout.FloatField(Styles.EaseInLabel, m_defaultEaseInDuration.floatValue);
            m_defaultEaseOurDuration.floatValue = EditorGUILayout.FloatField(Styles.EaseOutLabel, m_defaultEaseOurDuration.floatValue);

            m_defaultAnimLoopMode.enumValueIndex = EditorGUILayout.Popup(Styles.AnimLoopModeLabel, m_defaultAnimLoopMode.enumValueIndex, m_defaultAnimLoopMode.enumNames);
            m_defaultTrackOffset.enumValueIndex = EditorGUILayout.Popup(Styles.TrackOffsetLabel, m_defaultTrackOffset.enumValueIndex, m_defaultTrackOffset.enumNames);

#if TIMELINE_FRAMEACCURATE
            m_PlaybackLockedToFrame.boolValue = EditorGUILayout.Toggle(Styles.PlaybackLockedToFrame, m_PlaybackLockedToFrame.boolValue);
#endif

            if (GUILayout.Button("Reset to Default"))
            {
                m_defaultEaseInDuration.floatValue = 0.2f;
                m_defaultEaseOurDuration.floatValue = 0.2f;
                m_defaultAnimLoopMode.enumValueIndex = (int)AnimationPlayableAsset.LoopMode.On;
                m_defaultTrackOffset.enumValueIndex = (int)TrackOffset.ApplySceneOffsets;
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_SerializedObject.ApplyModifiedProperties();
                TimelinePreferences.instance.Save();
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateTimelineProjectSettingProvider()
        {
            var provider = new TimelineBlenderPreferencesProvider("Preferences/TimelineBlender", SettingsScope.User, GetSearchKeywordsFromGUIContentProperties<Styles>());
            return provider;
        }
    }
}