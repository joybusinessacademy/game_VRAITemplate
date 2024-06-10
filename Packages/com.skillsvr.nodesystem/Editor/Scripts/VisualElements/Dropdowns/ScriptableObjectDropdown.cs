using System;
using System.Collections.Generic;
using System.Linq;
using Props;
using Scripts.VisualElements.Dropdowns;
using SkillsVRNodes;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace VisualElements
{
	/// <summary>
	/// Search assets and show in dropdown. MUST DISPOSE after use, otherwise will cause deadlock reference and memory leak.
	/// </summary>
	/// <typeparam name="TScriptableObject"></typeparam>
	public class ScriptableObjectDropdown<TScriptableObject> : BaseDropdown, INotifyValueChanged<string>
		where TScriptableObject : ScriptableObject
	{
		protected virtual string DefaultName => "Null";
		protected virtual string CreateNewText => "Create New";
		
		protected DropdownField DropdownField => Dropdown;
		public IconButton showScriptableObjectButton;

		public string value
		{
			get => DropdownField?.value;
			set
			{
				if (null == DropdownField)
				{
					return;
				}
				DropdownField.value = value;
			}
		}

		private readonly ChangeDropdown changeEvent;
		public ScriptableObjectDropdown(string label, TScriptableObject outputValue, ChangeDropdown changeEvent, Action showObjectInProject = null)
		{
			styleSheets.Add(Resources.Load<StyleSheet>("ScriptableObjectDropdown"));

			name = "scriptable-object-dropdown";
			// Display first dropdown field

			LabelText = label;
			
			DropdownField.name = "float-variable-dropdown";
			DropdownField.value = outputValue ? outputValue.name : DefaultName;

			RegisterRefreshEvents();

			
			showScriptableObjectButton = new IconButton(showObjectInProject, "target", 16)
			{
				tooltip = "Open Scriptable Object in Project",
				style = { display = DisplayStyle.None } // Remove this to enable the target button
			};

			
			RefreshChoices();
			
			this.changeEvent = changeEvent;
			//DropdownField.RegisterCallback<ChangeEvent<string>>(evt => { DropdownCallback(changeEvent, evt); });
			
			Add(DropdownField);
			Add(showScriptableObjectButton);
		}

		private void RegisterRefreshEvents()
		{
			GraphAssetPostprocessor.AddListener(RefreshChoices);
			PropManager.OnPropListChanged(RefreshChoices);
		}

		private void UnregisterRefreshEvents()
		{
			GraphAssetPostprocessor.RemoveListener(RefreshChoices);
			PropManager.RemovePropListChangedEvent(RefreshChoices);
		}

		public override void Dispose()
		{
			UnregisterRefreshEvents();
		}
		
		public bool nameAfterScene = false;

		public TScriptableObject CreateScriptableObject(string previousName = null)
		{
			previousName ??= DropdownField.value;

			TScriptableObject updatedSO;
				
			if (nameAfterScene)
			{
				updatedSO = ScriptableObjectManager.CreateScriptableObject<TScriptableObject>(ScriptableObjectManager.Path() + "/" + SkillsVRGraphWindow.GetWindow.graph.name);
			}
			else
			{
				updatedSO = ScriptableObjectManager.CreateScriptableObject<TScriptableObject>();
			}
			
			if (updatedSO == null)
			{
				SetValueWithoutNotify(previousName);
				return null;
			}
			SetValueWithoutNotify(updatedSO ? updatedSO.name : DefaultName);
			
			AssetDatabaseFileCache.AddRefrence(updatedSO);
			
			return updatedSO;
		}

		private void RefreshChoices()
		{
			
			NullChoice = new KeyValuePair<string, Action>(DefaultName, () => changeEvent?.Invoke(null));

			MainChoices.Clear();
			foreach (TScriptableObject scriptableObject in AssetDatabaseFileCache.GetAllObjects<TScriptableObject>())
			{
				string fileName = scriptableObject.name;
				string folderName = AssetDatabase.GetAssetPath(scriptableObject).Split('/')[^2];

				MainChoices.TryAdd(folderName + '/' + fileName, () =>
				{
					SetValueWithoutNotify(scriptableObject.name);
					changeEvent.Invoke(scriptableObject);
				}); 
			}

			AlternateChoices.Clear();
			AlternateChoices.TryAdd(CreateNewText /*+ ObjectNames.NicifyVariableName(typeof(TScriptableObject).FullName)*/, () => changeEvent.Invoke(CreateScriptableObject()));
		}
		
		public void SetValueWithoutNotify(string newValue)
		{
			DropdownField?.SetValueWithoutNotify(newValue);
		}

		public delegate void ChangeDropdown(TScriptableObject scriptableObject);
	}
}