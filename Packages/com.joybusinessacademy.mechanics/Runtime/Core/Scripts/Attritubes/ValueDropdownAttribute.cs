using System;
using System.Collections.Generic;
using UnityEngine;

namespace VRMechanics
{

    public interface IValueDropdownItem
    {
        object GetValue();
    }
    public abstract class ValueDropdownListItemBase : IValueDropdownItem
    {
        public string name;

        public abstract object GetValue();
    }

    public sealed class ValueDropdownItem<T> : ValueDropdownListItemBase
    {
        public T value;

        public T Value => value;

        public ValueDropdownItem(string itemName, T itemValue)
        {
            name = itemName;
            value = itemValue;
        }

        public override object GetValue()
        {
            return value;
        }
    }

    public sealed class ValueDropdownList<T> : List<ValueDropdownItem<T>> { }

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class ValueDropdownAttribute : PropertyAttribute
    {
        public string methodName { get; set; }
        public string onValueChangedCallback { get; set; }

        public ValueDropdownAttribute() : base()
        {
        }

        public ValueDropdownAttribute(string optionMethodName, string callbackMethodName = null) : base()
        {
            methodName = optionMethodName;
            onValueChangedCallback = callbackMethodName;
        }
    }
}
