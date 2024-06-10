using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Ioc.Data
{
    public class BindingMap
    {
        protected KeyNode root { get; } = new KeyNode();

        public bool SetValue<T>(T value, params object[] keys)
        {
            if (null == value)
            {
                return false;
            }

            KeyNode current = GetOrCreateNode(keys);
            if (null == current)
            {
                return false;
            }
            if (null != current.Value )
            {
                return false;
            }
            current.Value = value;
            return true;
        }

        public bool SetValueOverride<T>(T value, params object[] keys)
        {
            if (null == value)
            {
                return false;
            }

            KeyNode current = GetOrCreateNode(keys);
            if (null == current)
            {
                return false;
            }
            current.Value = value;
            return true;
        }

        public bool TryGetValue<T>(out T value, params object[] keys)
        {
            value = default(T);
            KeyNode current = GetNode(keys);
            if (null == current)
            {
                return false;
            }
            bool success = current.TryGetValue<T>(out value);
            return success;
        }

        public T GetValue<T>(params object[] keys)
        {
            T output = default(T);
            TryGetValue<T>(out output, keys);
            return output;
        }

        protected KeyNode GetNode(params object[] keys)
        {
            if (null == keys)
            {
                return null;
            }

            KeyNode current = root;
            foreach (var key in keys)
            {
                current = current.GetChildren(key);
                if (null == current)
                {
                    return null;
                }
            }
            return current;
        }

        protected KeyNode GetOrCreateNode(params object[] keys)
        {
            if (null == keys)
            {
                return null;
            }

            KeyNode current = root;
            foreach (var key in keys)
            {
                current = current.GetOrAddChildren(key);
            }
            return current;
        }
    }
}