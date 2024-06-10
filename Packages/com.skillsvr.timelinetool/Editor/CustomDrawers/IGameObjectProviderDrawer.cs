using SkillsVR.TimelineTool.AnimatorTimeline;
using SkillsVR.TimelineTool.Bindings;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.TimelineTool.Editor
{
    [CustomPropertyDrawer(typeof(IGameObjectProvider), true)]
    public class IGameObjectProviderDrawer : PropertyDrawer
    {
        public bool showProperties;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw a foldout area for the property
            Rect foldoutPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            showProperties = EditorGUI.Foldout(foldoutPosition, showProperties, label);
            position.y += EditorGUIUtility.singleLineHeight;
            position.x += 20;
            if (showProperties)
            {
                // Draw all public and serializable fields of the scriptable object
                var scriptableObject = property.GetValue();
                if (scriptableObject != null)
                {
                    var fields = scriptableObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    fields.Where(f => f.IsPublic || null != f.GetCustomAttribute<SerializeField>())
                        .ToList()
                        .ForEach(field =>
                        {
                            var fieldProperty = property.FindPropertyRelative(field.Name);
                            if (null != fieldProperty)
                            {
                                float propertyHeight = EditorGUI.GetPropertyHeight(fieldProperty, true);
                                position.height = propertyHeight;
                                EditorGUI.PropertyField(position, fieldProperty, true);
                                position.y += propertyHeight;
                            }
                        });

                    position.height = EditorGUIUtility.singleLineHeight;
                    Rect buttonPos = position;
                    buttonPos.x += 20;
                    if (GUI.Button(buttonPos, "Test"))
                    {
                        var provider = property.GetValue() as IGameObjectProvider;
                        var obj = provider.GetTypedUnityObject();
                        if (null == obj)
                        {
                            Debug.LogError("Null");
                        }
                        else
                        {
                            Debug.Log(obj);
                        }
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (!showProperties)
            {
                return height;
            }
            var scriptableObject = property.GetValue();
            if (scriptableObject != null)
            {
                var fields = scriptableObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                fields.Where(f => f.IsPublic || null != f.GetCustomAttribute<SerializeField>()).ToList().ForEach(field =>
                {

                    var fieldProperty = property.FindPropertyRelative(field.Name);
                    if (null != fieldProperty)
                    {
                        float propertyHeight = EditorGUI.GetPropertyHeight(fieldProperty, true);
                        height += propertyHeight;
                    }
                });
            }

            height += EditorGUIUtility.singleLineHeight;
            return height;
        }
    }
}