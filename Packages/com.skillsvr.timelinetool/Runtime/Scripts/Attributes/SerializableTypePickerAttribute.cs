using System;
using UnityEngine;

namespace SkillsVR.TimelineTool
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SerializableTypePickerAttribute : PropertyAttribute
    {
        public Type baseType;
        public bool hideLabel;

        public SerializableTypePickerAttribute(Type filterBaseType = null)
        {
            baseType = filterBaseType;
        }
    }
}