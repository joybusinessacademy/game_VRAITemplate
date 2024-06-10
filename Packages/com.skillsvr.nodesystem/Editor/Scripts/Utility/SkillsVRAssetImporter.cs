using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using SFB;
using SkillsVRNodes.Editor.Graph;
using UnityEngine;
using UnityEngine.Video;

namespace SkillsVRNodes.Managers.Utility
{
	public class SkillsVRAssetImporter : AssetPostprocessor
	{
		
		public static Action OnAssetDatabaseChangedCallback = () => { };

		public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			AssetDatabaseFileCache.ClearAllProjectData();
			OnAssetDatabaseChangedCallback?.Invoke();
		}

		public static void OnAssetDatabaseChanged()
		{
			OnAssetDatabaseChangedCallback?.Invoke();
		}


		public class ImportType
		{
			public ImportType(string assetName, string importPath, string extentions)
			{
				this.assetName = assetName;
				this.importPath = importPath;
				this.extentions = extentions;
			} 
			
			public string assetName;
			private string importPath;
			public string GetImportPath() => $"Assets/Contexts/{AssetDatabaseFileCache.GetCurrentMainGraphName()}/{importPath}";
			public string extentions;
			
			public string[] GetExtensionList()
			{
				return extentions.Split(';');
			}
			
			public ExtensionFilter GetExtensionFilters()
			{
				return new ExtensionFilter(assetName, extentions.Split(';'));
			}
		}
		
		
		public static Dictionary<Type, ImportType> importTypes = new()
		{
			{ typeof(VideoClip), new ImportType("Video Clip", "Videos", "mp4;avi;mov;wmv;mkv;flv") },
			{ typeof(AudioClip), new ImportType("Audio Clip", "Audio", "mp3;wav;ogg;flac") },
			{ typeof(Texture2D), new ImportType("Image", "Images", "png;jpg;jpeg;gif;bmp;psd;tga;dds;exr") },
			{ typeof(UnityEngine.Object), new ImportType("Model", "Assets/Models", "fbx;obj;dae;3ds;blend;max;c4d;ma;mb") },
		};
		

		public static string GetAllImportTypes()
		{
			return importTypes.Aggregate("", (current, type) => current + (type.Value.extentions + ";"));
		}
		
		public static List<ExtensionFilter> GetAllExtensionFilters()
		{
			List<ExtensionFilter> list = new() { new ExtensionFilter("All Files", GetAllImportTypes().Split(";")) };
			foreach (ImportType importType in importTypes.Values)
			{
				list.Add(importType.GetExtensionFilters());
			}

			return list;
		}

		public static ImportType GetImporterFromAssetPath(string assetPath)
		{
			foreach (ImportType importType in importTypes.Values)
			{
				string assetExtension = assetPath.Split('.').Last();
				
				if (importType.GetExtensionList().Contains(assetExtension))
				{
					return importType;
				}
			}

			return null;
		}
		
		public static void ImportAssetsButton()
		{
			var paths = BatchImportAssets();

            for (int i = paths.Count - 1; i >= 0; i--)
            {
				ImportType type = GetImporterFromAssetPath(paths[i]);

				if (type.assetName != importTypes[typeof(UnityEngine.Object)].assetName)
					paths.RemoveAt(i);
			}

			if(paths.Count>0)
				ImportDirectorWindow.LoadAssetEdit(paths, null);
		}
		
		public static List<string> BatchImportAssetType<TImportType>()
		{
			if (!SupportsType<TImportType>())
			{
				return null;
			}
			
			ExtensionFilter[] assets = { importTypes[typeof(TImportType)].GetExtensionFilters() };
			
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Assets", "", assets, true);

			List<string> allPaths = ImportPaths(paths);

			return allPaths;
		}


		public static string ImportAssetType<TImportType>()
		{
			if (!SupportsType<TImportType>())
			{
				return null;
			}
			
			ExtensionFilter[] assets = { importTypes[typeof(TImportType)].GetExtensionFilters() };
			
			string[] path = StandaloneFileBrowser.OpenFilePanel("Select Assets", "", assets, false);

			if (path.IsNullOrEmpty())
			{
				return null;
			}
			
			string importedPath = ImportAsset(path[0]);

            return importedPath;
		}

		public static bool SupportsType<TImportType>()
		{
			return importTypes.ContainsKey(typeof(TImportType));
		}
		
		public static List<string> BatchImportAssets()
		{
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Select Assets", "", GetAllExtensionFilters().ToArray(), true);

			List<string> allPaths = ImportPaths(paths);

			return allPaths;
		}

		private static List<string> ImportPaths(string[] paths)
		{
			List<string> allPaths = new();

			foreach (string path in paths)
			{
				string newAssetPath = ImportAsset(path);
				if (newAssetPath.IsNullOrWhitespace())
				{
					continue;
				}

				allPaths.Add(newAssetPath);
			}
			
			return allPaths;
		}

		private static string ImportAsset(string pathToImport)
		{
			ImportType importType = GetImporterFromAssetPath(pathToImport);
			
			pathToImport = pathToImport.Replace('\\', '/');
			
			string newAssetPath = importType.GetImportPath();
			string fileName = pathToImport.Split('/')[^1];

			if (!Directory.Exists(newAssetPath))
			{
				Directory.CreateDirectory(newAssetPath);
			}

			newAssetPath += "/" + fileName;

			if (fileName.IsNullOrWhitespace())
			{
				return null;
			}
			
			FileUtil.ReplaceFile(pathToImport, newAssetPath);
			AssetDatabase.ImportAsset(newAssetPath);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
            return newAssetPath;
		}
	}
}