using System;
using System.Collections.Generic;
using SceneNavigation;
using SkillsVRNodes;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace VisualElements
{
	[Obsolete("All Scene elements and hotspots are now Props. Use Prop instead of SceneElement.")]
	public class SceneElementDropdown<TSceneElement> : DisposableVisualElement, INotifyValueChanged<string>
		where TSceneElement : SceneElement
	{
		public virtual string DefaultName { get; private set; } = "Null";

		public string value { 
			get => null == DropdownField ? null : DropdownField.value; 
			set
			{
				if (null == DropdownField)
				{
					return;
				}
				DropdownField.value = value;
			}
		}

		protected readonly DropdownField DropdownField;
		public IconButton showSceneElementButton;
		private bool showCreateNew;
		
		public SceneElementDropdown(string label, string outputValue, ChangeDropdown changeEvent, bool showCreateNew = true)
		{
			this.showCreateNew = showCreateNew;
			
			styleSheets.Add(Resources.Load<StyleSheet>("SceneElementDropdown"));
			name = "scene-element-dropdown";
			// Display first dropdown field
			DropdownField = new DropdownField
			{
				label = label,
				name = "float-variable-dropdown",
				value = !string.IsNullOrWhiteSpace(outputValue) ? outputValue : DefaultName
			};

			GraphAssetPostprocessor.AddListener(RefreshChoices);
			
			
			DropdownField.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				DropdownCallback(changeEvent, evt);
				RefreshChoices();
			});
			
			Add(DropdownField);

			showSceneElementButton = new IconButton(ShowElementInScene, "target", 16)
			{
				tooltip = "Show Element in Scene"
			};

			Add(showSceneElementButton);
			RefreshChoices();
		}

		public override void Dispose()
		{
			GraphAssetPostprocessor.RemoveListener(RefreshChoices);
		}

		private void ShowElementInScene()
		{
			TSceneElement sceneElement = SceneElementManager.GetSceneElement<TSceneElement>(DropdownField.value);

			if (sceneElement == null)
			{
				return;
			}
			EditorGUIUtility.PingObject(sceneElement);
			Selection.objects = new Object[] { sceneElement.gameObject };
			SceneCameraAssistant.LookAt(sceneElement.transform);
			
		}

		public delegate void ChangeDropdown(string elementName);

		
		private void RefreshChoices()
		{
			DropdownField.choices.Clear();
			
			List<TSceneElement> allSceneElementsEditor = SceneElementManager.GetAllSceneElements<TSceneElement>();
			if (!allSceneElementsEditor.IsNullOrEmpty())
			{
				foreach (TSceneElement sceneElement in allSceneElementsEditor)
				{
					DropdownField.choices.Add(sceneElement.elementName);
				}
			}
			DropdownField.choices.Sort();
			
			DropdownField.choices.Insert(0, DefaultName);
			DropdownField.choices.Insert(1, "");

			if (showCreateNew)
			{
				if (DropdownField.choices[^1] != "")
				{
					DropdownField.choices.Add("");
				}
				DropdownField.choices.Add("Create New");
			}
			
			if (DropdownField.value == DefaultName)
			{
				showSceneElementButton.AddToClassList("hidden");
			}
			else
			{
				showSceneElementButton.RemoveFromClassList("hidden");
			}
		}
		
		protected virtual void DropdownCallback(ChangeDropdown changeEvent, ChangeEvent<string> evt)
		{
			TSceneElement updatedSceneElement;
			if (evt.newValue == DefaultName)
			{
				updatedSceneElement = null;
			}
			else if (evt.newValue == "Create New")
			{
				updatedSceneElement = CreateNewElement();
				if (updatedSceneElement == null)
				{
					DropdownField.value = evt.previousValue;
					return;
				}
			}
			else
			{
				TSceneElement findScriptableObject = SceneElementManager.GetSceneElement<TSceneElement>(evt.newValue);
				updatedSceneElement = findScriptableObject;
			}

			changeEvent.Invoke(updatedSceneElement ? updatedSceneElement.elementName : "");
			DropdownField.value = updatedSceneElement ? updatedSceneElement.elementName : DefaultName;
		}

		private TSceneElement CreateNewElement()
		{
			string elementName = AskUserForString.Show("New " + typeof(TSceneElement).Name,
				"Please Insert a name for your new " + typeof(TSceneElement).Name, "new" + typeof(TSceneElement).Name);

			if (elementName == null)
			{
				return null;
			}

			while (SceneElementManager.GetSceneElement<TSceneElement>(elementName))
			{
				elementName = AskUserForString.Show("New " + typeof(TSceneElement).Name,
					"Please Insert a name for your new " + typeof(TSceneElement).Name,
					"new" + typeof(TSceneElement).Name);

				if (elementName == null)
				{
					return null;
				}
			}
			
			return SceneElementManager.AddElementEditor<TSceneElement>(elementName);
		}

		public void SetCustomDefaultName(string name)
		{
			name = string.IsNullOrWhiteSpace(name) ? "Null" : name;
			if (DefaultName == DropdownField.value)
			{
				DropdownField.value = name;
			}
			for(int i =0;i < DropdownField.choices.Count; i++)
			{
				var value = DropdownField.choices[i];
				if (string.IsNullOrWhiteSpace(value) || value == DefaultName)
				{
					DropdownField.choices[i] = name;
				}
			}
			DefaultName = name;
		}

		public void SetValueWithoutNotify(string newValue)
		{
			DropdownField?.SetValueWithoutNotify(newValue);
		}
	}
}