using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI
{
    public static class VisualElementSendCustomEventExtensions
    {
        public static void SendValueChangedEvent<T>(this VisualElement visual, T oldValue, T newValue)
        {
            if (null == visual)
            {
                return;
            }
            using (ChangeEvent<T> valueChangeEvt = ChangeEvent<T>.GetPooled(oldValue, newValue))
            {
                valueChangeEvt.target = visual;
                visual.SendEvent(valueChangeEvt);
            }
        }

        public static void SendCustomEvent(this VisualElement visual, EventBase eventObj)
        {
            if (null == visual || null == eventObj)
            {
                return;
            }
            eventObj.target = visual;
            visual.SendEvent(eventObj);
            eventObj.Dispose();
        }
    }
}