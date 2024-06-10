using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillsVR.UnityExtenstion;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers.GraphNavigator;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SkillsVRNodes.Managers.Setup
{
	public static class GraphSetupTools
	{
		public static COMPONENT_TYPE CreateGameObjectWithComponent<COMPONENT_TYPE>(string objectName = "")
			where COMPONENT_TYPE : Component
		{
			GameObject gameObject = new GameObject
			{
				name = objectName == "" ? ObjectNames.NicifyVariableName(typeof(COMPONENT_TYPE).Name) : objectName
			};
			COMPONENT_TYPE component = gameObject.AddComponent<COMPONENT_TYPE>();

			return component;
		}

		public static COMPONENT_TYPE ValidateOrCreateGameObjectWithComponent<COMPONENT_TYPE>(string objectName = "") 
			where COMPONENT_TYPE : Component
		{
			COMPONENT_TYPE component = Object.FindObjectOfType<COMPONENT_TYPE>();
			if (component == null)
			{
				component = CreateGameObjectWithComponent<COMPONENT_TYPE>(objectName);
				EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
			}

			component.GetOrAddComponent<NonDeletable>();

			return component;
		}

		public static bool RemoveObjectOfType<COMPONENT_TYPE>(string objectName = "") where COMPONENT_TYPE : Component
		{
			bool foundAndRemoved = false;
			COMPONENT_TYPE component = Object.FindObjectOfType<COMPONENT_TYPE>();

			if (component != null)
			{
				Object.DestroyImmediate(component.gameObject);
				foundAndRemoved = true;
			}

			return foundAndRemoved;
		}

		public const string projectLocationFolder = "/Contexts";
		
		public static string CurrentProjName => GraphFinder.CurrentActiveProject == null ? "" : GraphFinder.CurrentActiveProject.GetProjectName;

		public static string SceneFolder => $"/Contexts/{CurrentProjName} /Scenes";

		public static string OpenSceneNameWindow()
		{
			string newSceneName = AskUserForString.Show( "New Scene", "Please enter a scene name", "NewScene" );
			if (newSceneName == null)
			{
				return "";
			}
	
			return newSceneName;
		}

		public static List<string> GetAllScenesInProject()
		{
			List<string> sceneNames = AssetDatabase.FindAssets("t:Scene")
				.Select(guid => AssetDatabase.GUIDToAssetPath(guid))
				.Select(path => System.IO.Path.GetFileNameWithoutExtension(path))
				.ToList();

			return sceneNames;
		}

		public static bool DoesSceneExist(List<string> cachedSceneList, string sceneName)
		{
			bool sceneExists = cachedSceneList.Any(x => x == sceneName);
			return sceneExists;
		}
		
		public static string AskForNameAndCreateSceneAndAddToBuild(bool includeDefaultGameObjects = true)
		{
			string newSceneName = OpenSceneNameWindow();

			if (newSceneName == "")
			{
				return null;
			}

			return CreateSceneAndAddToBuild(newSceneName, includeDefaultGameObjects);
		}

		public static string CreateSceneAndAddToBuild(string newSceneName, bool includeDefaultGameObjects = true, string projectName = "")
		{
			if (GetAllScenesInProject().Contains(newSceneName))
			{
				return null;
			}

			if(projectName == string.Empty)
				projectName = AssetDatabaseFileCache.GetCurrentMainGraphName();

			string currentProjectSceneLocation = "Assets/Contexts/" + projectName + "/Scenes";

			if (projectName == string.Empty)
			{
				Debug.LogError("Missing Project Name - Scene will be created in a non-context folder - Find Scene Here: " + SceneFolder);
				Directory.CreateDirectory("Assets" + SceneFolder);
			}
			else
				Directory.CreateDirectory(currentProjectSceneLocation);
				
			Scene newScene = EditorSceneManager.NewScene(includeDefaultGameObjects ? NewSceneSetup.DefaultGameObjects : NewSceneSetup.EmptyScene);

			if (projectName == string.Empty)
				EditorSceneManager.SaveScene(newScene, "Assets" + SceneFolder + "/" + newSceneName + ".unity");
			else
				EditorSceneManager.SaveScene(newScene, currentProjectSceneLocation + "/" + newSceneName + ".unity");

			AddOrEnableSceneInBuildSettings(newScene.path);
			return newScene.path;
		}

        public static void AddOrEnableScenesInBuildSettings(IEnumerable<string> scenePathCollection)
		{
			if (null == scenePathCollection)
			{
				return;
			}
			foreach(var path in scenePathCollection)
			{
				AddOrEnableSceneInBuildSettings(path);
			}

		}

        public static void AddOrEnableSceneInBuildSettings(string scenePath)
		{
			if (string.IsNullOrWhiteSpace(scenePath))
			{
				return;
			}
            EditorBuildSettingsScene buildSettingsScene = new EditorBuildSettingsScene
            {
                path = scenePath,
                enabled = true
            };

            List<EditorBuildSettingsScene> tempArray = EditorBuildSettings.scenes.Where(x=> null != x && !string.IsNullOrWhiteSpace(x.path)).ToList();
            var exist = tempArray.Find(x => x.path == scenePath);
            if (null != exist)
            {
                exist.enabled = true;
            }
            else
            {
                tempArray.Add(buildSettingsScene);
            }
            EditorBuildSettings.scenes = tempArray.ToArray();
        }
	}
}