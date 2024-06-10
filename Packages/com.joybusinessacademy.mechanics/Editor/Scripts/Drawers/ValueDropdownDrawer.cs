using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRMechanics.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(ValueDropdownAttribute))]
    internal sealed class ValueDropdownDrawer : PropertyDrawer
    {
        private string[] optionNameArray;
        private object[] optionValueArray;

        private int optionIndex;
        private ValueDropdownAttribute smartTarget => (ValueDropdownAttribute)attribute;
        private bool init;

        void Init(SerializedProperty property)
        {
            if (init)
            {
                return;
            }
            var parent = property.GetParent();
            if (null == parent)
            {
                return;
            }
            var optionListObject = parent.TryInvoke(smartTarget.methodName, null);
            if (null == optionListObject)
            {
                return;
            }
            var optionListObjectType = optionListObject.GetType();
            if (null == optionListObject as IEnumerable)
            {
                return;
            }
            var enumerableList = optionListObject as IEnumerable;
            List<string> nameList = new List<string>();
            List<object> valueList = new List<object>();

            if (IsSubclassOfRawGeneric(typeof(IEnumerable<ValueDropdownListItemBase>), optionListObjectType)
                || IsSubclassOfRawGeneric(typeof(ValueDropdownList<>), optionListObjectType))
            {
                foreach (var item in enumerableList)
                {
                    if (null == item)
                    {
                        continue;
                    }
                    ValueDropdownListItemBase baseItem = item as ValueDropdownListItemBase;
                    if (null == baseItem || string.IsNullOrWhiteSpace(baseItem.name) || null == baseItem.GetValue())
                    {
                        continue;
                    }

                    nameList.Add(baseItem.name);
                    valueList.Add(baseItem.GetValue());
                }
            }
            else
            {
                foreach (var item in enumerableList)
                {
                    if (null == item)
                    {
                        continue;
                    }
                    nameList.Add(item.ToString());
                    valueList.Add(item);
                }
            }

            optionNameArray = nameList.ToArray();
            optionValueArray = valueList.ToArray();


            InitIndex(property.GetValue());
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //No Longer Working with Odin Removed  - TODO
            if (optionNameArray == null)
                return;

            Init(property);
            EditorGUI.BeginChangeCheck();

            if (optionNameArray == null)
			{
                EditorGUI.EndChangeCheck();
                return;
			}

            optionIndex = EditorGUI.Popup(position, property.displayName, optionIndex, optionNameArray);
            if (EditorGUI.EndChangeCheck())
            {
                property.SetValue(optionValueArray[optionIndex]);
                property.serializedObject.ApplyModifiedProperties();
                property.GetParent()?.TryInvoke(smartTarget.onValueChangedCallback, null);
            }
        }

        private void InitIndex(object value)
        {
            optionIndex = -1;
            if (null == value)
            {
                return;
            }
            foreach(object item in optionValueArray)
            {
                ++optionIndex;
                IComparable cv = value as IComparable;
                if (null != cv && 0 == cv.CompareTo(item))
                {
                    return;
                }
                if (item == value)
                {
                    return;
                }
            }

            optionIndex = -1;
        }
        static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
