using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public interface IBindingVisualElement
    {
        SerializedObject BindingSerializer { get; }
    }
    public abstract class AutoBindingVisualElement<T> : VisualElement, IBindingVisualElement
    {
        public abstract ScriptableObject<T> GetWrapper();

        public SerializedObject BindingSerializer { get; protected set; }

        protected ScriptableObject<T> wrapper;

        private T myBindingData;

        public AutoBindingVisualElement()
        {
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        public T BindingData
        {
            get
            {
                return myBindingData;
            }
            set
            {
                Dispose();

                myBindingData = value;
                if (null != myBindingData)
                {
                    wrapper = GetWrapper();
                    wrapper.data = myBindingData;
                    BindingSerializer = new SerializedObject(wrapper);
                    this.Bind(BindingSerializer);
                    this.BindButtons();
                }
            }
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            Dispose();
        }

        void Dispose()
        {
            this.Unbind();
            this.UnBindButtons();

            if (null != BindingSerializer)
            {
                BindingSerializer?.Dispose();
                BindingSerializer = null;
            }
            
            if (null != wrapper)
            {
                wrapper.data = default(T);
                GameObject.DestroyImmediate(wrapper);
                wrapper = null;
            }
        }

    }
}