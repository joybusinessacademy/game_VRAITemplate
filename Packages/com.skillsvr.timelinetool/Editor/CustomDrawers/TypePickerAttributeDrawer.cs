using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

namespace SkillsVR.TimelineTool.Editor
{
    [CustomPropertyDrawer(typeof(SerializableTypePickerAttribute))]
    public class TypePickerAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            var sourceAttr = attribute as SerializableTypePickerAttribute;
            label = sourceAttr.hideLabel ? GUIContent.none : label;
            EditorGUI.BeginProperty(position, label, property);

            EditorGUI.LabelField(position, label);

            // Draw the subclass selector popup.
            Rect popupPosition = new Rect(position);
            popupPosition.height = EditorGUIUtility.singleLineHeight;
            if (!sourceAttr.hideLabel)
            {
                popupPosition.width -= EditorGUIUtility.labelWidth;
                popupPosition.x += EditorGUIUtility.labelWidth;
            }

            var objectValue = property.GetValue() as SerializableType;
            var valueType = (Type)objectValue;

            if (EditorGUI.DropdownButton(popupPosition, new GUIContent(null == valueType ? "null" : valueType.Name), FocusType.Keyboard))
            {
                var baseType = sourceAttr.baseType;
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());
                if (null != baseType)
                {
                    types = types.Where(t => t == baseType || baseType.IsAssignableFrom(t));
                }
                AdvancedDropdownT<Type> dropdown = new AdvancedDropdownT<Type>(types);
                dropdown.onItemSelected += (type, item) =>
                {
                    SerializableType st = type;
                    property.SetValue(st);
                };
                dropdown.GetItemNameFromData = (type) => { return null == type ? "null" : type.FullName.Replace(".", "/"); };
                dropdown.GetLabel = () => { return null == baseType ? "Select a Type" : "Select a " + baseType.Name + " Type"; };
                dropdown.Show(popupPosition);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}