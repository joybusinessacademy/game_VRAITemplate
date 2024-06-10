#if I2_LOCALIZATION

using I2.Loc;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.UIElements;

namespace DialogExporter
{
	public class I2LocalisationSource : ILocalisationSource
	{
		public LocalizedString localizedString;

		public bool CanBeEdited() => false;

		public string Translation(string term = "en")
		{
			return localizedString.ToString();
		}

		public void EditDialog(string newDialog)
		{
			
		}
		
		
		public static VisualElement VisualElement(LocalisedString localizedString)
		{
			DropdownField dropdown = new()
			{
				value = localizedString.ToString(),
				choices = LocalizationManager.GetTermsList()
			};


			
			return dropdown;
		}

		public void GetLocalisationItems()
		{
#if UNITY_EDITOR
			string[] guids = AssetDatabase.FindAssets("t:" + typeof(LanguageSourceAsset).Name);

			if (guids.Length != 0)
				LocalizationManager.Sources.Clear();

			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				LanguageSourceAsset asset = AssetDatabase.LoadAssetAtPath<LanguageSourceAsset>(assetPath);

				if (LocalizationManager.Sources == null)
					LocalizationManager.Sources = new List<LanguageSourceData> { asset.SourceData };
				else
				{
					LocalizationManager.Sources.Add(asset.SourceData);
					LocalizationManager.UpdateSources();
				}
			}
			if (LocalizationManager.CurrentLanguage == null)
				LocalizationManager.CurrentLanguage = "English";
#endif

		}
	}
}
#endif