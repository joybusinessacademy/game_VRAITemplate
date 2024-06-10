using UnityEngine;

namespace SkillsVR.TimelineTool.Bindings
{


    [System.Serializable]
    public class UnityObjectBinding : IUnityObjectProvider
    {
        public string id;

        [SerializeReference]
        [ClassPicker]
        public IUnityObjectProvider provider;


        public bool Test(object caller = null)
        {
            bool pass = true;
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError("Binding id cannot be null or white space");
                pass = false;
            }
            string idStr = string.IsNullOrWhiteSpace(id) ? "null" : id;
            if (null == provider)
            {
                Debug.LogError("Binding " + id + ": Provider cannot be null.");
                pass = false;
            }

            if (null != provider )
            {
                var obj = provider.GetUnityObject(caller);
                if (null == obj)
                {
                    Debug.LogError("Binding " + id + ": Null Value.");
                    pass = false;

                }
            }
            return pass;
        }

        public bool IsValid()
        {
            return null != provider && !string.IsNullOrWhiteSpace(id);
        }

        public Object GetUnityObject(object caller = null)
        {
            if (null == provider)
            {
                return null;
            }
            return provider.GetUnityObject(caller);
        }

        public object Clone()
        {
            var clone = new UnityObjectBinding();
            clone.provider = null == this.provider ? null : this.provider.Clone() as IUnityObjectProvider;
            return clone;
        }
    }
}