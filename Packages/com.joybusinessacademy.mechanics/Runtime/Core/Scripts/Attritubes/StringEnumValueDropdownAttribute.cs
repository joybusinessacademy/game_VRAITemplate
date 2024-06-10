using System;
using UnityEngine;

namespace VRMechanics
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class StringEnumValueDropdownAttribute : PropertyAttribute
    {
        public Type enumType { get; set; }
        public bool enableCustomValue { get; set; }
        public string onValueChangedCallback { get; set; }
    }
}
