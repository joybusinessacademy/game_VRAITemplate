using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Toggle = UnityEngine.UIElements.Toggle;

namespace SkillsVRNodes.Editor.NodeViews
{
	public static class VisualElementCreator
	{
		public static VisualElement CustomTextField(this object fieldObject, string getFieldName, string label = null)
		{
			FieldInfo field = fieldObject.GetType().GetField(getFieldName);

			TextField title = new()
			{
				label = GetLabel(getFieldName, label),
				value = (string)field?.GetValue(fieldObject) == null ? "" : field.GetValue(fieldObject).ToString(),
				tooltip = ObjectNames.NicifyVariableName(getFieldName),
				multiline = true
			};
			title.TrySetNameFromFieldName(getFieldName);
			title.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				field?.SetValue(fieldObject, evt.newValue);
			});
			return title;
		}


		public static IntegerField CustomIntField(this object fieldObject, string getFieldName, string label = null)
		{
			FieldInfo field = fieldObject.GetType().GetField(getFieldName);

			IntegerField title = new()
			{
				label = GetLabel(getFieldName, label),
				value = (int)field?.GetValue(fieldObject)!,
				tooltip = ObjectNames.NicifyVariableName(getFieldName),
			};
			title.TrySetNameFromFieldName(getFieldName);
			title.RegisterCallback<ChangeEvent<int>>(evt =>
			{
				field.SetValue(fieldObject, evt.newValue);
			});
			return title;
		}
		
		public static IntegerField CustomUIntField(this object fieldObject, string getFieldName, string label = null)
		{
			FieldInfo field = fieldObject.GetType().GetField(getFieldName);

			IntegerField title = new()
			{
				label = GetLabel(getFieldName, label),
				value = (int)(uint)field?.GetValue(fieldObject)!,
				tooltip = ObjectNames.NicifyVariableName(getFieldName),
			};
			title.TrySetNameFromFieldName(getFieldName);
			title.RegisterCallback<ChangeEvent<int>>(evt =>
			{
				int value = evt.newValue;
				if (value < 0)
				{
					value = 0;
					title.SetValueWithoutNotify(value);
				}
				
				field.SetValue(fieldObject, (uint)value);
				
			});
			return title;
		}

		
		public static FloatField CustomFloatField(this object fieldObject, string getFieldName, string label = null)
		{
			FieldInfo field = fieldObject.GetType().GetField(getFieldName);

			FloatField title = new()
			{
				label = GetLabel(getFieldName, label),
				value = (float)field?.GetValue(fieldObject)!,
				tooltip = ObjectNames.NicifyVariableName(getFieldName),
			};
			title.TrySetNameFromFieldName(getFieldName);
			title.RegisterCallback<ChangeEvent<float>>(evt =>
			{
				field.SetValue(fieldObject, evt.newValue);
			});
			return title;
		}

		public static Vector3Field CustomVectorField(this object fieldObject, string getFieldName, string label =null)
        {
			FieldInfo field = fieldObject.GetType().GetField(getFieldName);

			Vector3Field vec = new()
			{
				label = GetLabel(getFieldName, label),
				value = (Vector3)field?.GetValue(fieldObject)!,
				tooltip = ObjectNames.NicifyVariableName(getFieldName),
			};
			vec.TrySetNameFromFieldName(getFieldName);
			vec.RegisterCallback<ChangeEvent<Vector3>>(evt =>
			{
				field.SetValue(fieldObject, evt.newValue);
			});

			return vec;
		}
		
		public static Toggle CustomToggle(this object fieldObject, string getFieldName, string label = null)
		{
			FieldInfo field = fieldObject.GetType().GetField(getFieldName);
			
			Toggle title = new()
			{
				label = GetLabel(getFieldName, label),
				value = (bool)field?.GetValue(fieldObject)!,
				tooltip = ObjectNames.NicifyVariableName(getFieldName),
			};

			title[0].style.flexGrow = 1;
			title.TrySetNameFromFieldName(getFieldName);
			title.RegisterCallback<ChangeEvent<bool>>(evt =>
			{
				field.SetValue(fieldObject, evt.newValue);
			});
			return title;
		}
		
		/// <summary>
		/// Creates a visual element of any type attached to anything that inherits from unity objects.
		/// </summary>
		/// <param name="objectProperty">The Unity Object which has the property</param>
		/// <param name="propertyName">The name of the Property</param>
		/// <param name="label">The label for the visual element</param>
		/// <returns>The Visual Element for the field</returns>
		public static VisualElement CustomField(this Object objectProperty, string propertyName, string label = null)
		{
			FieldInfo fieldInfo = objectProperty.GetType().GetField(propertyName);
			
			SerializedObject serializedObject = new SerializedObject(objectProperty);
			PropertyField element = new(serializedObject.FindProperty(fieldInfo.Name), GetLabel(propertyName, label));
			element.TrySetNameFromFieldName(propertyName);
			element.Bind(serializedObject);
			return element;
		}
		
		// public static VisualElement CustomField(this object objectProperty, string propertyName, string label = null)
		// {
		// 	FieldInfo fieldInfo = objectProperty.GetType().GetField(propertyName);
		// 	
		// 	SerializedObject serializedObject = new SerializedObject(objectProperty);
		// 	PropertyField element = new(serializedObject.FindProperty(fieldInfo.Name), GetLabel(propertyName, label));
		// 	
		// 	element.Bind(serializedObject);
		// 	return element;
		// }

		public static Toggle CustomToggle(this object fieldObject, string getFieldName, Action onChange, string label = null)
		{
			FieldInfo field = fieldObject.GetType().GetField(getFieldName);

			Toggle title = new()
			{
				label = GetLabel(getFieldName, label),
				value = (bool)field?.GetValue(fieldObject)!,
				tooltip = ObjectNames.NicifyVariableName(getFieldName),
			};
			title.TrySetNameFromFieldName(getFieldName);
			title[0].style.flexGrow = 1;

			title.RegisterCallback<ChangeEvent<bool>>(evt =>
			{
				onChange.Invoke();
				field.SetValue(fieldObject, evt.newValue);
			});
			return title;
		}

		private static string GetLabel(string fieldName, string label)
		{
			string newLabel = "";
			if (label == null)
			{
				newLabel = ObjectNames.NicifyVariableName(fieldName);
			}
			else if (label != "")
			{
				newLabel = label;
			}

			return newLabel;
		}

		public static void SetDisplay(this VisualElement visualElement, bool display)
		{
			if (null == visualElement)
			{
				return;
			}
			visualElement.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;
		}

		public static bool GetDisplay(this VisualElement visualElement)
		{
			if (null == visualElement)
			{
				return false;
			}
			return DisplayStyle.Flex == visualElement.style.display;
		}

		public static void Show(this VisualElement visualElement)
		{
			visualElement.SetDisplay(true);
		}
		public static void Hide(this VisualElement visualElement)
		{
			visualElement.SetDisplay(false);
		}

		public static string TrySetNameFromFieldName(this VisualElement visual, string fieldName, bool forceReplace = false)
		{
			if (string.IsNullOrWhiteSpace(visual.name) || forceReplace)
			{
				visual.name = (ObjectNames.NicifyVariableName(fieldName.Replace(":", "")) + " " + visual.GetType().Name).ToLower().Replace(" ", "-");
			}
			return visual.name;
		}

		public static string SetNameFromFieldName(this VisualElement visual, string fieldName)
		{
			return visual.TrySetNameFromFieldName(fieldName, true);
		}
	}
}