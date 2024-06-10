using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace VRMechanics.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(TypeNameValueDropdownAttribute))]
    internal sealed class TypeNameDropdownDrawer : PropertyDrawer
    {
        private string[] optionArray;
        private int optionIndex;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            TypeNameValueDropdownAttribute typeNameDropdownAttribute = (TypeNameValueDropdownAttribute)attribute;
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use TypeNameDropdown with string type property or field.");
                return;
            }

            if (null == optionArray && null != typeNameDropdownAttribute.baseType)
            {
                InitTypeArray(typeNameDropdownAttribute);
                InitIndex(property.stringValue);
            }

            EditorGUI.BeginChangeCheck();
            optionIndex = EditorGUI.Popup(position, property.displayName, optionIndex, optionArray);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = optionArray[optionIndex];
                property.serializedObject.ApplyModifiedProperties();
                property.GetParent()?.TryInvoke(typeNameDropdownAttribute.onValueChangedCallback, null);
            }
        }

        private void InitIndex(string stringValue)
        {
            optionIndex = -1;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return;
            }
            foreach(string value in optionArray)
            {
                ++optionIndex;
                if (stringValue == value)
                {
                    return;
                }
                
            }
        }

        void InitTypeArray(TypeNameValueDropdownAttribute typeNameDropdownAttribute)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes());
            if (!typeNameDropdownAttribute.includeAbstract)
            {
                types = types.Where(t => !t.IsAbstract);
            }
            if (!typeNameDropdownAttribute.includeInterface)
            {
                types = types.Where(t => !t.IsInterface);
            }
            if (!typeNameDropdownAttribute.includeGenericType)
            {
                types = types.Where(t => !t.IsGenericType);
            }
            if (typeNameDropdownAttribute.includeBaseType)
            {
                types = types.Where(t => t == typeNameDropdownAttribute.baseType || typeNameDropdownAttribute.baseType.IsAssignableFrom(t));
            }
            else
            {
                types = types.Where(t => typeNameDropdownAttribute.baseType.IsAssignableFrom(t));
            }
            optionArray = types.Select(x => x.Name).ToArray();
        }
    }
}
