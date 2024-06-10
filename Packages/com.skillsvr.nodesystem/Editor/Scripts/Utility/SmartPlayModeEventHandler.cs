using System;
using UnityEngine;
using UnityEditor;

namespace SkillsVRNodes.Managers.Utility
{
    public class SmartPlayModeEventHandler : IPlayModeEventListener
    {
        private event Action internalEnterPlayModeEvents;
        public event Action OnEnterPlayMode
        {
            add
            {
                if (null != value)
                {
                    internalEnterPlayModeEvents += value;
                    if (EditorApplication.isPlayingOrWillChangePlaymode) { value.Invoke(); }
                }
            }
            remove
            {
                internalEnterPlayModeEvents -= value;
            }
        }
        private event Action internalExitPlayModeEvents;
        public event Action OnExitPlayMode
        {
            add
            {
                if (null != value)
                {
                    internalExitPlayModeEvents += value;
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) { value.Invoke(); }
                }
            }
            remove
            {
                internalExitPlayModeEvents -= value;
            }
        }
        private event Action internalEnterEditModeEvents;
        public event Action OnEnterEditMode
        {
            add
            {
                if (null != value)
                {
                    internalEnterEditModeEvents += value;
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) { value.Invoke(); }
                }
            }
            remove
            {
                internalEnterEditModeEvents -= value;
            }
        }

        private event Action internalExitEditModeEvents;
        public event Action OnExitEditMode
        {
            add
            {
                if (null != value)
                {
                    internalExitEditModeEvents += value;
                    if (EditorApplication.isPlayingOrWillChangePlaymode) { value.Invoke(); }
                }
            }
            remove
            {
                internalExitEditModeEvents -= value;
            }
        }

        private event Action<bool> internalPlayModeEvents;
        public event Action<bool> OnPlayMode
        {
            add
            {
                if (null != value)
                {
                    internalPlayModeEvents += value;
                    value?.Invoke(EditorApplication.isPlayingOrWillChangePlaymode);
                }
            }
            remove
            {
                internalPlayModeEvents -= value;
            }
        }

        private event Action<bool> internalEditModeEvents;
        public event Action<bool> OnEditMode
        {
            add
            {
                if (null != value)
                {
                    internalEditModeEvents += value;
                    value?.Invoke(!EditorApplication.isPlayingOrWillChangePlaymode);
                }
            }
            remove
            {
                internalEditModeEvents -= value;
            }
        }

        public SmartPlayModeEventHandler()
		{
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
		}

		~SmartPlayModeEventHandler()
		{
			EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
		}

        private void EditorApplication_playModeStateChanged(PlayModeStateChange state)
        {
            try
            {
                switch (state)
                {
                    case PlayModeStateChange.EnteredEditMode:
                        internalEnterEditModeEvents?.Invoke();
                        internalEditModeEvents?.Invoke(true);
                        break;
                    case PlayModeStateChange.ExitingEditMode:
                        internalExitEditModeEvents?.Invoke();
                        internalEditModeEvents?.Invoke(false);
                        break;
                    case PlayModeStateChange.EnteredPlayMode:
                        internalEnterPlayModeEvents?.Invoke();
                        internalPlayModeEvents?.Invoke(true);
                        break;
                    case PlayModeStateChange.ExitingPlayMode:
                        internalExitPlayModeEvents?.Invoke();
                        internalPlayModeEvents?.Invoke(false);
                        break;
                    default:
                        break;
                }
            }
            catch(Exception e)
            {
                Debug.LogException(e);
            }
        }

		public void Clear()
		{
            internalEditModeEvents = null;
            internalEnterEditModeEvents = null;
            internalEnterPlayModeEvents = null;
            internalExitEditModeEvents = null;
            internalExitPlayModeEvents = null;
            internalPlayModeEvents = null;
		}
	}
}
