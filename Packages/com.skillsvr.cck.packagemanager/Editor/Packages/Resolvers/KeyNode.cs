using System.Collections.Generic;

namespace SkillsVR.CCK.PackageManager.Ioc.Data
{

    public class KeyNode
    {
        public Dictionary<object, KeyNode> Children { get; } = new Dictionary<object, KeyNode>();
        public object Value { get;  set; }

        public bool To(object newValue, bool forceOverride = false)
        {
            if (null != Value && !forceOverride)
            {
                return false;
            }
            Value = newValue;
            return true;
        }

        public KeyNode GetChildren(object key)
        {
            if (null == key)
            {
                return null;
            }
            KeyNode output = null;
            Children.TryGetValue(key, out output);
            return output;
        }

        public KeyNode GetOrAddChildren(object key)
        {
            if (null == key)
            {
                return null;
            }
            var existing = GetChildren(key);
            if (null != existing)
            {
                return existing;
            }
            KeyNode node = new KeyNode();
            Children.Add(key, node);
            return node;
        }

        public T GetValue<T>()
        {
            return GetValue<T>(default(T));
        }

        public T GetValue<T>(T defaultValue)
        {
            T output = default(T);
            if (TryGetValue<T>(out output))
            {
                return output;
            }
            return defaultValue;
        }

        public bool TryGetValue<T>(out T outputValue)
        {
            outputValue = default(T);
            if (null == Value)
            {
                return false;
            }
            if (Value is T)
            {
                outputValue = (T)Value;
                return true;
            }
            return false;
        }
    }
}