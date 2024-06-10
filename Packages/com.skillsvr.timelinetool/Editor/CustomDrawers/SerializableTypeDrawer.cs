using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.TimelineTool.Editor
{
    [CustomPropertyDrawer(typeof(SerializableType), true)]
    public class SerializableTypeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializableType value = property.GetValue() as SerializableType;
            Type type = value;
            string content = null == type ? "<null>" : type.Name;
            EditorGUI.LabelField(position, label, new GUIContent(content));
        }


        public Attribute FindPropertyAttributeByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            return fieldInfo.GetCustomAttributes().Where(a => null != a && a.GetType().Name.ToLower().Contains(name.ToLower())).FirstOrDefault();
        }
    }
}