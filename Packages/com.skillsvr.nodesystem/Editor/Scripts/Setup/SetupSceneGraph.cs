using System.Collections;
using GraphProcessor;
using Props;
using SkillsVR.Messeneger;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Scripts;
using Unity.EditorCoroutines.Editor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SkillsVRNodes.Managers.Setup
{
	public static class SetupSceneGraph
	{
		//[MenuItem("Skills Node/Current Scene/Set Up Scene For Graph")]
		public static void SetUpScene()
		{
			SceneGraphExecutorComponent graphExecutorComponent = GraphSetupTools.ValidateOrCreateGameObjectWithComponent<SceneGraphExecutorComponent>("Graph Controller");
			ValidateSceneElements();
			GraphSetupTools.RemoveObjectOfType<Camera>("Main Camera");
			GraphSetupTools.RemoveObjectOfType<Light>("Directional Light");

			if (graphExecutorComponent.graph == null)
			{
				string path = SceneManager.GetActiveScene().path;
				path = System.IO.Path.GetDirectoryName(path);
				SceneGraph graph = ScriptableObjectManager.CreateScriptableObject<SceneGraph>(path, SceneManager.GetActiveScene().name + "Graph");
				graph.position = new UnityEngine.Vector3(580,200,0);
				graphExecutorComponent.graph = graph;
			}
		}

		public static void ValidateSceneElements()
		{
			GraphSetupTools.ValidateOrCreateGameObjectWithComponent<PropManager>("Prop Manager");
			GraphSetupTools.ValidateOrCreateGameObjectWithComponent<MechanicsManager>("Mechanics Manager");
			GraphSetupTools.ValidateOrCreateGameObjectWithComponent<GlobalMessenger>("Global Messenger");
			GraphSetupTools.ValidateOrCreateGameObjectWithComponent<XRUIInputModule>("Event System");
			
            PlayerSpawnPosition playerSpawn = GraphSetupTools.ValidateOrCreateGameObjectWithComponent<PlayerSpawnPosition>("Player Spawn");
            NewSceneWindow.MovePlayer(playerSpawn.transform);
		}

		static IEnumerator Delayed()
		{
			yield return new EditorWaitForSeconds(0.1f);
			SkillsVRGraphWindow.RefreshGraph();
		}

		//[MenuItem("Skills Node/New Scene/Create Scene Graph")]
		public static void NewSceneGraphSceneVoid()
		{
			NewSceneGraphScene();
		}
		
		public static string NewSceneGraphScene()
		{
			string scenePath = GraphSetupTools.AskForNameAndCreateSceneAndAddToBuild();
			if (string.IsNullOrEmpty(scenePath))
			{
				return null;
			}

			SetUpScene();
			EditorSceneManager.SaveOpenScenes();
			return scenePath;
		}
	}
}
