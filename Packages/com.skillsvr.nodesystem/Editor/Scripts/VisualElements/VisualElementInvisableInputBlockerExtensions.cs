using SkillsVRNodes.Editor.NodeViews;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.VisualElements
{
    public static class VisualElementInvisableInputBlockerExtensions
    {
        const string INPUT_BLOCKER_CUSTOM_CLICK_EVENT_KEY = "ipblkCustomClickEventKey";
        const string INPUT_BLOCKER_ELEMENT_NAME = "input_blocker";
        public static VisualElement GetInputBlocker(this VisualElement source)
        {
            if (null == source)
            {
                return null;
            }
			var blocker = source.Q(INPUT_BLOCKER_ELEMENT_NAME);
            return blocker;
		}

        public static VisualElement AddInputBlocker(this VisualElement source)
        {
            if (null == source)
            {
                return null;
            }

            var blocker = source.Q(INPUT_BLOCKER_ELEMENT_NAME);
            if (null != blocker)
            {
                return blocker;
            }
            blocker = source.MakeInputBlockerElement();
            source.Add(blocker);

            source.ExecOnceOnRenderReady(() => {
				blocker?.BringToFront();
				blocker?.CopyPosAndSizeFrom(source);
			});

            blocker.Hide();

            return blocker;
        }

        public static void RemoveInputBlocker(this VisualElement source)
        {
            if (null == source)
            {
                return;
            }

            var blocker = source.Q(INPUT_BLOCKER_ELEMENT_NAME);
            if (null == blocker)
            {
                return;
            }
            var callback = blocker.userData as EventCallback<GeometryChangedEvent>;
            if (null != callback)
            {
                source.UnregisterCallback<GeometryChangedEvent>(callback);
            }
            source.Remove(blocker);
        }

        public static void EnableInputBlocker(this VisualElement source, bool enableInputBlocker)
        {
            if (null == source)
            {
                return;
            }

            var blocker = source.Q(INPUT_BLOCKER_ELEMENT_NAME);
            if (enableInputBlocker && null == blocker)
            {
                blocker = source.AddInputBlocker();
            }
            blocker?.CopyPosAndSizeFrom(source);
            blocker?.BringToFront();
            blocker?.SetDisplay(enableInputBlocker);
        }

        public static void SetInputBlockerClickEvent(this VisualElement source, Action action)
        {
			if (null == source)
			{
				return;
			}

			var blocker = source.Q(INPUT_BLOCKER_ELEMENT_NAME);
			if (null == blocker)
			{
				return;
			}
            if (null == action)
            {
                return;
            }
            blocker.SetUserData<Action>(INPUT_BLOCKER_CUSTOM_CLICK_EVENT_KEY, action);
		}
        private static VisualElement MakeInputBlockerElement(this VisualElement source)
        {
            VisualElement blocker = new VisualElement();
            blocker.name = INPUT_BLOCKER_ELEMENT_NAME;
            blocker.style.backgroundColor = new StyleColor(Color.clear);
            blocker.StopResponseKeyAndMouseEvents();
            blocker.style.position = Position.Absolute;
            return blocker;
        }

        public static void StopResponseKeyAndMouseEvents(this VisualElement visualElement)
        {
            if (null == visualElement)
            {
                return;
            }
			visualElement.RegisterCallback<MouseDownEvent>((evt) =>
            {
                visualElement.GetUserData<Action>(INPUT_BLOCKER_CUSTOM_CLICK_EVENT_KEY)?.Invoke();
                evt.StopPropagation(); 
            });
			visualElement.RegisterCallback<MouseUpEvent>((evt) => { evt.StopPropagation(); });
			visualElement.RegisterCallback<MouseEnterEvent>((evt) => { evt.StopPropagation(); });
			visualElement.RegisterCallback<MouseOverEvent>((evt) => { evt.StopPropagation(); });
			visualElement.RegisterCallback<MouseOutEvent>((evt) => { evt.StopPropagation(); });
			visualElement.RegisterCallback<KeyDownEvent>((evt) => { evt.StopPropagation(); });
			visualElement.RegisterCallback<KeyUpEvent>((evt) => { evt.StopPropagation(); });
		}
        
    }
}