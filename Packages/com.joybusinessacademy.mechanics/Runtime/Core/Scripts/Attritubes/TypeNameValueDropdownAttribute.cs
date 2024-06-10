using System;
using System.Linq;
using UnityEngine;

namespace VRMechanics
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public sealed class TypeNameValueDropdownAttribute : PropertyAttribute
	{
        public Type baseType { get; set; }
        public bool includeAbstract { get; set; } = false;
        public bool includeInterface { get; set; } = false;
        public bool includeGenericType { get; set; } = false;
        public bool includeBaseType { get; set; } = false;

        public string onValueChangedCallback { get; set; }

        public TypeNameValueDropdownAttribute() : base()
        {
        }
	}
}
