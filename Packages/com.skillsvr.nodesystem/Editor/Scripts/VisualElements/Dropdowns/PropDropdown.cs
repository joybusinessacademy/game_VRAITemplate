using System;
using System.Collections.Generic;
using Props;
using Props.PropInterfaces;
using SkillsVRNodes.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace VisualElements
{
	public class PropDropdown<TPropType> : DisposableVisualElement
		where TPropType : class, IBaseProp
	{
		public virtual string DefaultName { get; private set; } = "None";
		private bool showCreateNew;
		protected readonly DropdownField dropdownField;
		
		public IconButton showPropButton;

		private PropGUID<TPropType> currentProp;
		
		private readonly Type basePropType;

		public PropDropdown(string label, PropGUID<TPropType> currentProp, ChangeDropdown changeEvent, bool showCreateNew = false, Type eventProp = null)
		{
			this.showCreateNew = showCreateNew;
			this.currentProp = currentProp;
			this.basePropType = eventProp;
			styleSheets.Add(Resources.Load<StyleSheet>("PropDropdown"));
			name = "prop-dropdown";
			// Display first dropdown field
			dropdownField = new DropdownField
			{
				label = label,
				name = "float-variable-dropdown",
				value = currentProp != null ? currentProp.GetPropName() : DefaultName
			};

			if (dropdownField.value.IsNullOrWhitespace())
			{
				dropdownField.value = DefaultName;
			}

			PropManager.OnPropListChanged(RefreshChoices);
			
			
			dropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				DropdownCallback(changeEvent, evt);
				RefreshChoices();
			});
			
			Add(dropdownField);

			showPropButton = new IconButton(ShowElementInScene, "target", 16)
			{
				tooltip = "Show Element in Scene"
			};

			Add(showPropButton);
			RefreshChoices();
			
			
			if (Application.isPlaying)
			{
				if (dropdownField.value == DefaultName)
				{
					dropdownField.SetValueWithoutNotify("Loading...");
				}
				SceneManager.sceneLoaded += OnSceneLoaded;
			}
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			RefreshChoices();
			SceneManager.sceneLoaded -= OnSceneLoaded;

		}

		public delegate void ChangeDropdown(PropGUID<TPropType> propGUID); 

		public override void Dispose()
		{
			PropManager.RemovePropListChangedEvent(RefreshChoices);
		}
		private void RefreshChoices()
		{
			string newValue =  currentProp != null ? currentProp.GetPropName() : DefaultName;
			if (newValue.IsNullOrWhitespace())
			{
				newValue = DefaultName;
			}
			dropdownField.value = newValue;
			
			dropdownField.choices.Clear();
			
			List<PropReferences> allPropsEditor = PropManager.GetAllProps<TPropType>();
			if (!allPropsEditor.IsNullOrEmpty())
			{
				foreach (PropReferences namedProp in allPropsEditor)
				{
					dropdownField.choices.Add(namedProp.PropComponent.PropName);
				}
			}
			dropdownField.choices.Sort();
			
			dropdownField.choices.Insert(0, DefaultName);
			dropdownField.choices.Insert(1, "");

			if (showCreateNew)
			{
				if (dropdownField.choices[^1] != "")
				{
					dropdownField.choices.Add("");
				}
				dropdownField.choices.Add("Create New");
			}
			
			if (dropdownField.value == DefaultName)
			{
				showPropButton.AddToClassList("hidden");
			}
			else
			{
				showPropButton.RemoveFromClassList("hidden");
			}
		}
		
		private void ShowElementInScene()
		{
			TPropType prop = currentProp.GetProp();

			if (prop == null)
			{
				return;
			}
			EditorGUIUtility.PingObject(prop.GetPropComponent());
			Selection.activeGameObject = prop.GetPropComponent().gameObject;
		}

		
		protected virtual void DropdownCallback(ChangeDropdown changeEvent, ChangeEvent<string> evt)
		{
			PropGUID<TPropType> updatedProp;
			if (evt.newValue == DefaultName
				|| string.IsNullOrWhiteSpace(evt.newValue))
			{
				updatedProp = new PropGUID<TPropType>();
				updatedProp.propGUID = System.Guid.Empty.ToString();
			}
			else if(evt.newValue == "Create New")
			{
				string propName = CreatePropName();

				if (propName == string.Empty)
					return;

				PropManager.Instance.CreateNewPropFromGraph(propName, basePropType);			

				updatedProp = new PropGUID<TPropType>
				{
					propGUID = PropManager.GetGUIDByName<TPropType>(propName)?.propGUID
				};
			}
			else
			{
				updatedProp = new PropGUID<TPropType>
				{
					propGUID = PropManager.GetGUIDByName<TPropType>(evt.newValue)?.propGUID
				};
			}

			currentProp = updatedProp;
			changeEvent.Invoke(currentProp);
			dropdownField.SetValueWithoutNotify(!updatedProp.IsNullOrEmpty() ? evt.newValue : DefaultName);
		}

		private string CreatePropName()
		{
			string elementName = AskUserForString.Show("New " + typeof(TPropType).Name,
				"Please Insert a name for your new " + typeof(TPropType).Name, "new" + typeof(TPropType).Name);

			return elementName;
		}
		
		public void SetCustomDefaultName(string name)
		{
			name = string.IsNullOrWhiteSpace(name) ? "Null" : name;
			if (DefaultName == dropdownField.value)
			{
				dropdownField.value = name;
			}
			for(int i =0;i < dropdownField.choices.Count; i++)
			{
				var value = dropdownField.choices[i];
				if (string.IsNullOrWhiteSpace(value) || value == DefaultName)
				{
					dropdownField.choices[i] = name;
				}
			}
			DefaultName = name;
		}
	}
}



