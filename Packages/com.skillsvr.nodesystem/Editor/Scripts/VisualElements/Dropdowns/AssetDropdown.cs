using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillsVRNodes.Managers.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace Scripts.VisualElements
{
	public class AssetDropdown<TAsset> : DisposableVisualElement
	where TAsset : Object
	{
		private Dictionary<string, string> namePathPair;

		public const string DefaultLabel = "Null";
		
		public DropdownField dropdown;
		
		public string ImportOneString => "External/Import " + typeof(TAsset).Name;
		public string ImportBatchString => "External/Batch Import " + typeof(TAsset).Name;

		public string assetTags;

		private static string currentMainGraphName;

		public AssetDropdown(ChangeDropdown dropdownCallback, TAsset asset, string listLabel = null, string assetTags = "")
		{
			this.dropdownCallback = dropdownCallback;
			this.assetTags = assetTags;
			
			string assetPath = AssetDatabase.GetAssetPath(asset);
			dropdown = new DropdownField();
			ResetChoices();
			dropdown.value = asset == null ? DefaultLabel : PathToName(assetPath);
			dropdown.RegisterCallback<ChangeEvent<string>>(OnDropdownChange);
			
			listLabel ??= ObjectNames.NicifyVariableName(typeof(TAsset).Name);
			dropdown.label = listLabel;
			
			Add(dropdown);
			SkillsVRAssetImporter.OnAssetDatabaseChangedCallback += ResetChoices;
		}

		~AssetDropdown()
		{
			// This line never be called because the static event keep reference of this instance.
			// So GC skip this and never call this destructor.
			// Use Dispose/Destroy or similar and manually remove the callback (and ref).
			// For cck node view contents, inherit from DisposableVisualElement which could auto dispose from node views.
			SkillsVRAssetImporter.OnAssetDatabaseChangedCallback -= ResetChoices;
		}
		
		private void ResetChoices()
		{
			if (dropdown == null)
			{
				return;
			}

			//List<string> allGUIDs = AssetDatabase.FindAssets(assetTags == "" ? $"t:{typeof(TAsset).Name}" : $"t:{typeof(TAsset).Name} l:{assetTags}").ToList();
			namePathPair = new Dictionary<string, string>();
			//foreach (string guid in allGUIDs)
			//{
			//	string path = AssetDatabase.GUIDToAssetPath(guid);
			//string niceName = PathToName(path);
			//namePathPair.TryAdd(niceName, path);
			//}



            foreach (string pathFromGUID in AssetDatabaseFileCache.GetAssetPathsOfType<TAsset>(assetTags))
			{
				string niceName = PathToName(pathFromGUID);
				namePathPair.TryAdd(niceName, pathFromGUID);
			}

			dropdown.choices.Clear();
			dropdown.choices.Add(DefaultLabel);
			dropdown.choices.Add("");
			foreach (KeyValuePair<string, string> assetPath in namePathPair)
			{
				dropdown.choices.Add(assetPath.Key);
			}

			if (SkillsVRAssetImporter.SupportsType<TAsset>())
			{
				if (dropdown.choices.Last() != "")
				{
					dropdown.choices.Add("");
				}

				if (SkillsVRAssetImporter.SupportsType<TAsset>())
				{
					dropdown.choices.Add(ImportOneString);
					dropdown.choices.Add(ImportBatchString);
				}
			}
		}

		private static string PathToName(string path)
		{
			string folder = "Assets";
			if (path.Split("/").Length >= 3)
			{
				folder = path.Split("/")[^3];
			}
			
			string niceName = CleanName(path);
			if (folder == AssetDatabaseFileCache.GetCurrentMainGraphName())
			{
				return niceName;
			}
			
			return "Projects/" + folder + "/" + niceName;
		}

		private static string CleanName(string path)
		{
			string niceName = Path.GetFileNameWithoutExtension(path);
			niceName = niceName.Replace("_", " ");
			niceName = niceName.Replace("-", " ");
			niceName = ObjectNames.NicifyVariableName(niceName);
			return niceName;
		}

		public delegate void ChangeDropdown(TAsset element);
		ChangeDropdown dropdownCallback;

		protected virtual void OnDropdownChange(ChangeEvent<string> evt)
		{
			string path;

			// User Selects None
			if (dropdown.index == 0)
			{
				dropdownCallback.Invoke(null);
				return;
			}
			
			
			// User Imports One Asset
			if (evt.newValue == ImportOneString)
			{
                path = SkillsVRAssetImporter.ImportAssetType<TAsset>();
				if (path.IsNullOrWhitespace())
				{
					dropdown.SetValueWithoutNotify(evt.previousValue);
					return;
				}

                assetPathToGrab = path;
                GrabAssetAfterImport(assetPathToGrab);
                dropdown.SetValueWithoutNotify(Path.GetFileNameWithoutExtension(assetPathToGrab));			

            }
			// User Imports Multiple Assets
			else if (evt.newValue == ImportBatchString)
			{
				List<string> importedAssets = SkillsVRAssetImporter.BatchImportAssetType<TAsset>();
				if (importedAssets.IsNullOrEmpty())
				{
					dropdown.SetValueWithoutNotify(evt.previousValue);
					return;
				}
				
                assetPathToGrab = importedAssets[0];
                GrabAssetAfterImport(assetPathToGrab);
                dropdown.SetValueWithoutNotify(Path.GetFileNameWithoutExtension(assetPathToGrab));
				
			}
			else
			{
				path = evt.newValue;
				TAsset asset = AssetDatabase.LoadAssetAtPath<TAsset>(namePathPair.GetValueOrDefault(path));
				dropdownCallback.Invoke(asset);
			}
			
		}

        private void GrabAssetAfterImport(string path)
        {
			if (assetPathToGrab.IsNullOrWhitespace())
				assetPathToGrab = path;

            TAsset asset = AssetDatabase.LoadAssetAtPath<TAsset>(assetPathToGrab);
            dropdownCallback.Invoke(asset);
            dropdown.SetValueWithoutNotify(Path.GetFileNameWithoutExtension(assetPathToGrab));
            
            AssetDatabaseFileCache.AddRefrence(asset);

            SkillsVRAssetImporter.OnAssetDatabaseChanged();
        }

        private string assetPathToGrab;

		public void GrabAssetAfterImport()
		{
			TAsset asset = AssetDatabase.LoadAssetAtPath<TAsset>(namePathPair.GetValueOrDefault(assetPathToGrab));
			dropdownCallback.Invoke(asset);
			dropdown.SetValueWithoutNotify(Path.GetFileNameWithoutExtension(assetPathToGrab));
			SkillsVRAssetImporter.OnAssetDatabaseChangedCallback -= GrabAssetAfterImport;
		}	

		public override void Dispose()
		{
			SkillsVRAssetImporter.OnAssetDatabaseChangedCallback -= ResetChoices;
			SkillsVRAssetImporter.OnAssetDatabaseChangedCallback -= GrabAssetAfterImport;
		}
	}
}