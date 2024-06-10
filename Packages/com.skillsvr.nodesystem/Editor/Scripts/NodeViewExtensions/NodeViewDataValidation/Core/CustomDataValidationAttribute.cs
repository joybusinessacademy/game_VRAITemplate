using System;
using System.Collections;
using UnityEngine;

namespace SkillsVRNodes.Editor.NodeViews.Validation
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class CustomDataValidationAttribute : Attribute
    {
        public Type targetType { get; set; }

        public CustomDataValidationAttribute(Type xType)
        {
            targetType = xType;
        }
    }
}