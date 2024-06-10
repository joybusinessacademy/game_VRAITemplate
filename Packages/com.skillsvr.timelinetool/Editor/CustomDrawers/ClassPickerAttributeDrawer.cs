using System;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.TimelineTool.Editor
{
    [CustomPropertyDrawer(typeof(ClassPickerAttribute))]
	public class ClassPickerAttributeDrawer : PropertyDrawer
	{
		bool show = false;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			var sourceAttr = this.attribute as ClassPickerAttribute;
			label = sourceAttr.hideLabel ? GUIContent.none : label;

			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(position, label);

			// Draw the subclass selector popup.
			Rect popupPosition = new Rect(position);
			popupPosition.height = EditorGUIUtility.singleLineHeight;
			if (!sourceAttr.hideLabel)
			{
				popupPosition.width -= EditorGUIUtility.labelWidth;
				popupPosition.x += EditorGUIUtility.labelWidth;
			}

			var objectValue = property.managedReferenceValue;
			if (null == objectValue)
			{
				objectValue = property.GetValue();
			}
			string typeName = null == objectValue ? "null" : objectValue.GetType().Name;


			if (EditorGUI.DropdownButton(popupPosition, new GUIContent(typeName), FocusType.Keyboard))
			{
				var fieldType = property.GetFieldType();

				var types = fieldType.GetAllSubClassTypesFromBaseTypes(sourceAttr.includeOriginFieldType, sourceAttr.extraBaseTypes);

				AdvancedDropdownT<Type> dropdown = new AdvancedDropdownT<Type>(types);
				dropdown.onItemSelected += (type, item) =>
				{
					if (null == type)
					{
						property.SetValue(null);
					}
					else
					{
						try
						{
							var obj = Activator.CreateInstance(type);
							property.managedReferenceValue = obj;
							property.isExpanded = null != obj;
							property.serializedObject.ApplyModifiedProperties();
							property.serializedObject.Update();
						}
						catch (Exception e)
						{
							Debug.LogException(e);
						}
					}
				};
				dropdown.GetLabel = () => { return null == fieldType ? "Select a Type" : "Select a " + fieldType.Name + " Type"; };

				dropdown.Show(popupPosition);
			}

			if (property.propertyType != SerializedPropertyType.ManagedReference)
			{
				EditorGUI.LabelField(position, label, "Not ManagedReference");
			}
			else
			{
				EditorGUI.PropertyField(position, property, GUIContent.none, true);
			}
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = 0;//EditorGUIUtility.singleLineHeight;
			height += EditorGUI.GetPropertyHeight(property, true);
			return height;
		}
	}
}