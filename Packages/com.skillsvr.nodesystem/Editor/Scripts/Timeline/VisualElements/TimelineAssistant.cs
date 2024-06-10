using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Samples.Editor.General
{
    public class TimelineAssistant : EditorWindow
    {
        // This method opens two DragAndDropWindows when a user selects the specified menu item.
        [MenuItem("SkillsVR CCK/Timeline Assistant")]
        public static void OpenWindow()
        {
            GetWindow<TimelineAssistant>();
        }

        public static PlayableDirector currentDirector;
        public static TimelineAsset currentAsset;
        
        void CreateGUI()
        {
            name = "Timeline Assistant";
            titleContent = new GUIContent("Timeline Assistant");
            RefreshWindow();
            TryLockTimelineWindow();
        }   

        public void RefreshWindow()
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(new TimelineAssistantVisualElement());
            minSize = new Vector2(325, 200);
        }

        public static void Refresh()
        {
            GetWindow<TimelineAssistant>().RefreshWindow();
        }

        public static void TryLockTimelineWindow()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(DelayLockTimelineWindow());
        }

        private static IEnumerator DelayLockTimelineWindow(float timeout = 5.0f)
        {
            var startTime = Time.realtimeSinceStartup;
            while (null == TimelineEditor.GetWindow())
            {
                yield return null;
                if (Time.realtimeSinceStartup - startTime > timeout)
                {
                    yield break;
                }
            }

            TimelineEditor.GetWindow().locked = true;
        }
    }
}