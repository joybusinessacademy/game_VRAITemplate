using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Scripts;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.Managers
{
	public static class ScriptableObjectManager
	{

		public static void RefreshList<TScriptableObject>() where TScriptableObject : ScriptableObject
		{
			// Adds the key if it is null
			if (AllScriptableObjectsDictionary.ContainsKey(typeof(TScriptableObject)) == false)
			{
				AllScriptableObjectsDictionary.Add(typeof(TScriptableObject), new List<ScriptableObject>());
			}
			
			AllScriptableObjectsDictionary[typeof(TScriptableObject)] = LoadAllInstancesFromDatabase<TScriptableObject>();
		}


		private static List<ScriptableObject> LoadAllInstancesFromDatabase<TScriptableObject>() where TScriptableObject : ScriptableObject
		{
			//FindAssets uses tags check documentation for more info
			string[] guids = AssetDatabase.FindAssets("t:"+ typeof(TScriptableObject).Name);
			List<ScriptableObject> allInstances = new();
			
			foreach (string t in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(t);
				var asset = AssetDatabase.LoadAssetAtPath<TScriptableObject>(path);
				if(asset)
					allInstances.Add(asset);
			}
			return allInstances;
		}


		private static readonly Dictionary<Type, List<ScriptableObject>> AllScriptableObjectsDictionary = new();

		public static List<TScriptableObject> GetAllInstances<TScriptableObject>() where TScriptableObject : ScriptableObject
		{
			if (AllScriptableObjectsDictionary.ContainsKey(typeof(TScriptableObject)) == false)
			{				
				RefreshList<TScriptableObject>();
			}

			// cleanup nulls
			if (AllScriptableObjectsDictionary.ContainsKey(typeof(TScriptableObject)) == false)
				AllScriptableObjectsDictionary[typeof(TScriptableObject)].RemoveAll(item => item == null);

			List<ScriptableObject> scriptableList = new List<ScriptableObject>();
			List<TScriptableObject> convertedList = new List<TScriptableObject>();

			scriptableList = AllScriptableObjectsDictionary[typeof(TScriptableObject)].ToList();

			foreach (ScriptableObject scriptableObject in scriptableList)
			{
				if(scriptableObject is TScriptableObject)
				{
					TScriptableObject newObject = (TScriptableObject)scriptableObject;
					convertedList.Add(newObject);
				}
			}

			return convertedList;
		}

		
		public static string Path<T>() => Path() + "/" + typeof(T).Name;
		public static string Path() => "Assets/Contexts/" + GraphName() + "/ScriptableObjects";
		public static string GraphName() => AssetDatabaseFileCache.GetCurrentMainGraphName();

		public static TScriptableObject CreateScriptableObject<TScriptableObject>(string scriptablePath = null, string assetName = "") where TScriptableObject : ScriptableObject
		{
			scriptablePath ??= Path<TScriptableObject>();
			
			TScriptableObject asset = ScriptableObject.CreateInstance<TScriptableObject>();

			Directory.CreateDirectory(scriptablePath);
			AssetDatabase.Refresh();
			if (assetName == "")
			{
				assetName = GetNameFromUser<TScriptableObject>();
				if (assetName == null)
				{
					return null;
				}
			}

			string assetPath = AssetDatabase.GenerateUniqueAssetPath(scriptablePath + "/" + assetName + ".asset");
			AssetDatabase.CreateAsset(asset, assetPath);
			
			AssetDatabase.SaveAssets();

			RefreshList<TScriptableObject>();
			return asset;
		}

		private static string GetNameFromUser<TScriptableObject>() where TScriptableObject : ScriptableObject
		{
			string assetName = AskUserForString.Show("New " + typeof(TScriptableObject).Name, "Please Insert a name for your new " + typeof(TScriptableObject).Name, "new" + typeof(TScriptableObject).Name);
			if (string.IsNullOrEmpty(assetName))
			{
				return null;
			}

			while (File.Exists(Application.dataPath + Path<TScriptableObject>() + "/" + assetName + ".asset"))
			{
				assetName = AskUserForString.Show("New " + typeof(TScriptableObject).Name, "Please Insert a name for your new " + typeof(TScriptableObject).Name + "\n ERROR: asset already exists", "new" + typeof(TScriptableObject).Name);
				if (string.IsNullOrEmpty(assetName))
				{
					return null;
				}
			}

			return assetName;
		}

		public static bool CheckIfAssetNameExists<TScriptableObject>(string name) where TScriptableObject :ScriptableObject
        {
			if (AllScriptableObjectsDictionary.ContainsKey(typeof(TScriptableObject)) == false)
				AllScriptableObjectsDictionary[typeof(TScriptableObject)].RemoveAll(item => item == null);

			for (int i = 0; i < AllScriptableObjectsDictionary[typeof(TScriptableObject)].Count; i++)
            {
				if (AllScriptableObjectsDictionary[typeof(TScriptableObject)][i].name.Equals(name))
				{
					return true;
				}
            }

			return false;
        }

		public static void RefreshAllLists()
		{
			List<Type> test = AllScriptableObjectsDictionary.Keys.ToList();

			foreach (Type type in test)
			{
				//FindAssets uses tags check documentation for more info
				string[] guids = AssetDatabase.FindAssets("t:"+ type.Name);
				AllScriptableObjectsDictionary[type] = new List<ScriptableObject>();
			
				foreach (string t in guids)
				{
					string path = AssetDatabase.GUIDToAssetPath(t);
					AllScriptableObjectsDictionary[type].Add(AssetDatabase.LoadAssetAtPath<ScriptableObject>(path));
				}
			}
		}

		public static void ForceSerialization(ScriptableObject so)
        {
			Undo.RecordObject(so, "ForcedSave");
			EditorUtility.SetDirty(so);
		}
	}
}

