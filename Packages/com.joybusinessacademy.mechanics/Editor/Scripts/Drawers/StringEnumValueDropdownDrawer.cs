using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace VRMechanics.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(StringEnumValueDropdownAttribute))]
    internal sealed class StringEnumValueDropdownDrawer : PropertyDrawer
    {
        private string[] optionArray;
        private int optionIndex;
        private bool isCustomValue;
        private StringEnumValueDropdownAttribute smartTarget;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            smartTarget = (StringEnumValueDropdownAttribute)attribute;
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use EnumValueDropdown with string type property or field.");
                return;
            }

            if (null == optionArray && null != smartTarget.enumType)
            {
                InitOptionValues(smartTarget);
                Init(property.stringValue);
            }

            EditorGUI.BeginChangeCheck();
            optionIndex = EditorGUI.Popup(position, property.displayName, optionIndex, optionArray);
            isCustomValue = smartTarget.enableCustomValue && optionIndex == optionArray.Length - 1;
            if (isCustomValue)
            {
                property.stringValue = EditorGUILayout.TextField(" ", property.stringValue);
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (!isCustomValue)
                {
                    property.stringValue = optionArray[optionIndex];
                }
                property.serializedObject.ApplyModifiedProperties();
                property.GetParent()?.TryInvoke(smartTarget.onValueChangedCallback, null);
            }
        }

        void InitOptionValues(StringEnumValueDropdownAttribute smartTarget)
        {
            List<string> optionList = new List<string>();
            try
            {
                optionList.AddRange(Enum.GetNames(smartTarget.enumType));
            }
            catch
            {
            }
            if (smartTarget.enableCustomValue)
            {
                optionList.Add("Custom Value");
            }
            optionArray = optionList.ToArray();
        }

        void Init(string initValue)
        {
            optionIndex = -1;
            if (null == initValue)
            {
                return;
            }
            foreach (string value in optionArray)
            {
                optionIndex++;
                if (value == initValue)
                {
                    return;
                }
            }
            if (!smartTarget.enableCustomValue)
            {
                optionIndex = 0;
            }
        }
    }
}
