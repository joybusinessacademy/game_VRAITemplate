using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DialogExporter;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	public static class LocalisationVisualElementCreator
	{
		private static List<Type> allTypes;

		private static List<Type> GetAllTypes(Type type)
		{
			if (allTypes.IsNullOrEmpty())
			{
				var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s?.GetTypes())
					.Where(type.IsAssignableFrom).ToList();

				allTypes = new List<Type>();
				if (!types.Any())
				{
					return allTypes;
				}
				foreach (Type t in types.Where(ValidateType))
				{
					allTypes.Add(t);
				}
			}
			
			return allTypes;
		}

		public static bool ValidateType(Type type)
		{
			if (type == null)
			{
				return false;
			}

			if (type == typeof(ILocalisationSource))
			{
				return false;
			}

			MethodInfo method = type.GetMethod("VisualElement");
			if (method == null || method.ReturnType != typeof(VisualElement))
			{
				return false;
			}

			if (!method.GetParameters().Where(t => t.GetType() == typeof(LocalisedString)).IsNullOrEmpty())
			{
				return false;
			}
			
			if (type.GetConstructor(Type.EmptyTypes) == null)
			{
				return false;
			}
			
			return true;
		}
		
		public static VisualElement LocField(this LocalisedString localisedString, string labelText = null)
		{
			VisualElement container = new()
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					flexGrow = 1
				}
			};

			localisedString ??= new LocalisedString();
			localisedString.LocalisationSource ??= new DefaultLocalisationSource();

			
			// Creates a label visual element for the text
			if (labelText != null)
			{
				Label label = new(labelText)
				{
					style =
					{
						paddingLeft = 4,
						paddingTop = 3,
						flexShrink = 1,
						width = new StyleLength(Length.Percent(50)),
					}
				};

				container.Add(label);
			}

			
			//List<Type> allTypes = GetAllTypes(typeof(ILocalisationSource));
			List<Type> allTypes = new() { typeof(DefaultLocalisationSource) };

			if (allTypes.Count <= 1)
			{
				MethodInfo methodInfo = localisedString.LocalisationSource.GetType().GetMethod("VisualElement");

				if (methodInfo != null)
				{
					VisualElement locVisualElement = methodInfo.Invoke(localisedString, new object[] { localisedString }) as VisualElement;
					if (locVisualElement != null)
					{
						locVisualElement.style.flexShrink = 1;

						container.Add(locVisualElement);
					}
				}

				return container;
			}

			DropdownField dropdownField = new();

			foreach (Type variable in allTypes)
			{
				dropdownField.choices.Add(variable.Name.Replace("LocalisationSource", ""));
			}

			dropdownField.value = localisedString.LocalisationSource.GetType().Name.Replace("LocalisationSource", "");

			dropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				if (evt.previousValue == evt.newValue)
				{
					return;
				}

				int indexOfType = dropdownField.choices.FindIndex(s => s == evt.newValue);

				localisedString.LocalisationSource = Activator.CreateInstance(allTypes[indexOfType]) as ILocalisationSource;

				VisualElement locVisualElement = container[container.childCount - 1];
				container.Remove(locVisualElement);

				CreateGenericLocVisualElement(localisedString, container);
			});

			container.Add(dropdownField);

			CreateGenericLocVisualElement(localisedString, container);

			//Mainly Used for I2 - Needs latest Messenger package Pulled
			//localisedString.LocalisationSource.GetLocalisationItems();

			return container;


		}

		private static void CreateGenericLocVisualElement(LocalisedString localisedString, VisualElement container)
		{
			MethodInfo methodInfo = localisedString.LocalisationSource.GetType().GetMethod("VisualElement");

			if (methodInfo != null)
			{
				object locVisualElement = methodInfo.Invoke(localisedString, new object[] { localisedString });

				container.Add(locVisualElement as VisualElement);
			}
		}
	}
}