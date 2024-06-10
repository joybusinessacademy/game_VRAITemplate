using System;
using UnityEngine;

namespace SkillsVR.TimelineTool
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ClassPickerAttribute : PropertyAttribute
    {
        public Type[] extraBaseTypes;
        public bool hideLabel;
        public bool includeOriginFieldType = true;
        public ClassPickerAttribute(params Type[] extraTypes)
        {
            extraBaseTypes = extraTypes;
        }
    }
}
