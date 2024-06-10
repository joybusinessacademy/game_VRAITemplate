using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Props;
using Props.PropInterfaces;
using UnityEditor;
using UnityEngine;


namespace Scripts.Props
{
	[CustomPropertyDrawer(typeof(PropGUID<>))]

	public class PropGuidEditor : PropertyDrawer
	{
		private const int padding = 1;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 2 + padding * 3;
		}
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			position.y += padding;
			position.height = EditorGUIUtility.singleLineHeight;

			List<string> pathsThing = property.propertyPath.Split(".").ToList();
			object targetObject = property.serializedObject.targetObject;


			for (int i = 0; i < pathsThing.Count; i++)
			{
				if (pathsThing[i].Contains("["))
				{
					i--;
					pathsThing.RemoveAt(i);
					pathsThing[i] = pathsThing[i].Substring(pathsThing[i].IndexOf("[", StringComparison.Ordinal));
				}
			}
			
			
			foreach (string fieldName in pathsThing)
			{
				if (fieldName.Contains("["))
				{
					int index = int.Parse(fieldName.Replace("]", "").Replace("[", ""));
					
					IEnumerable nEnumerable = (IEnumerable)targetObject;

					if (nEnumerable != null)
					{
						IEnumerator numbers = nEnumerable.GetEnumerator();
						numbers.MoveNext();
						for (int i = 0; i < index; i++)
						{
							numbers.MoveNext();
						}
					
						targetObject = numbers.Current;
					}
				}
				else
				{
					if (targetObject != null)
					{
						FieldInfo fieldInfo = targetObject.GetType().GetField(fieldName);
						targetObject = fieldInfo.GetValue(targetObject);
					}
				}
			}


			if (targetObject == null)
			{
				return;
			}
			
			
			string guid = targetObject.GetType().GetField("propGUID")?.GetValue(targetObject)?.ToString();
			
			Type propType = targetObject.GetType().GenericTypeArguments[0];

			string propName = PropManager.GetPropName(guid);
			if (propName.IsNullOrEmpty())
			{
				propName = "NO PROP FOUND";
			}
			
			if (EditorGUI.DropdownButton(position, new GUIContent(propName), FocusType.Keyboard))
			{
				OpenDropdown(propType, targetObject);
			}
			
			position.y += EditorGUIUtility.singleLineHeight;
			position.y += padding;

			string guidOutput = EditorGUI.TextField(position, "GUID " + targetObject.GetType().GenericTypeArguments[0].ToString().Split(".")[^1], guid);

			if (guidOutput != guid)
			{
				targetObject.GetType().GetField("propGUID").SetValue(targetObject, guidOutput);
			}
		}

		private static void OpenDropdown(Type propType, object targetObject)
		{
			GenericMenu genericMenu = new();
			
			List<PropReferences> allProps = new();

			MethodInfo[] things = typeof(PropManager).GetMethods(BindingFlags.Static | BindingFlags.Public);
			MethodInfo getAllPropsMethod = things.FirstOrDefault(t => t.Name == "GetAllProps" && t.IsGenericMethod);
			if (getAllPropsMethod != null)
			{
				getAllPropsMethod = getAllPropsMethod.MakeGenericMethod(propType);

				allProps = getAllPropsMethod.Invoke(null, new object[] { }) as List<PropReferences>;
			}

			if (allProps == null)
			{
				return;
			}

			foreach (PropReferences prop in allProps)
			{
				void OnItemClicked()
				{
					targetObject.GetType().GetField("propGUID").SetValue(targetObject, prop.GUID.propGUID);
				}

				genericMenu.AddItem(new GUIContent(prop.PropComponent.name), false, OnItemClicked);
			}

			genericMenu.ShowAsContext();
		}
	}
}