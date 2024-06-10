using System;
using UnityEngine;

namespace SkillsVR.TimelineTool
{
	[Serializable]
    public class SerializableType
    {
        [SerializeField]
        [HideInInspector]
        protected string typeInfo;

        protected Type cachedType;

        public SerializableType()
        {
        }
        public SerializableType(Type type)
        {
            SetTypeInternal(type);
        }
        
        public static implicit operator Type(SerializableType wrapper)
        {
            return null == wrapper ? null : wrapper.GetCachedTypeInternal();
        }

        public static implicit operator SerializableType(Type type)
        {
            return new SerializableType(type);
        }
       

        protected virtual void SetTypeInternal(Type targetType)
        {
            typeInfo = null == targetType ? null : targetType.AssemblyQualifiedName;
            cachedType = targetType;
        }

        protected Type GetCachedTypeInternal()
        {
            if (null != cachedType && cachedType.AssemblyQualifiedName == typeInfo)
            {
                return cachedType;
            }
            cachedType = null;
            if (string.IsNullOrWhiteSpace(typeInfo))
            {
                return null;
            }

            try
            {
                cachedType = Type.GetType(typeInfo);
            }
            catch { }
            return cachedType;
        }

        public override string ToString()
        {
            Type t = GetCachedTypeInternal();
            return null == t ? "<null type>" : t.ToString();
        }

        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }
            if (obj.GetType() == typeof(Type) || obj.GetType() == this.GetType())
            {
                return (Type)obj == (Type)this;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ((Type)this).GetHashCode();
        }
    }

}