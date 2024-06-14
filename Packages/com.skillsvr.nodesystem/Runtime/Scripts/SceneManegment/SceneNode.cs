using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;


namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Scene", typeof(MainGraph))]
	public class SceneNode : ExecutableNode
	{
		public override string name => "Scene";
		public override string icon => "Unity";
		public override string layoutStyle => "SceneNode";
		public override Color color => NodeColours.Other;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/base-nodes#scene-node";
		public override int Width => MEDIUM_WIDTH;

		[Output(name = "Complete", allowMultiple = false)]
		public ConditionalLink Complete = new(); 
		
		public string scenePath = "";
		public List<string> additiveScenePaths = new List<string>();
		public string lightingScene
		{
			get
			{
				if (additiveScenePaths.IsNullOrEmpty() || additiveScenePaths.FirstOrDefault().IsNullOrEmpty())
				{
					return scenePath;
				}
				return additiveScenePaths.FirstOrDefault();
			}
		}

		public AudioClip audioClip;
		
		private SceneGraphExecutorComponent sceneNodeFromGraphExecutorComponent;
		public static string activeLightingScene = "";

		public string templateId;

		public string assistantId;
		public string assistantInstruction;
		public string assitantFiles;
		public string vectorStoreId = "NULL_ID";

		protected override void OnStart()
		{
			MainGraphExecutorComponent.SceneAudioSource.loop = true;
			MainGraphExecutorComponent.SceneAudioSource.clip = audioClip;
			MainGraphExecutorComponent.SceneAudioSource.Play();
			LoadSceneList();
		}
		
		/// <summary>
		/// Call this to end the scene
		/// </summary>
		public void EndScene()
		{
			MainGraphExecutorComponent.SceneAudioSource.clip = null;
			MainGraphExecutorComponent.SceneAudioSource.Stop();
			CompleteNode();
		}

		/// <summary>
		/// This is called each time a scene is loaded. will only run actions if all scenes in "singleAsyncOperation" are loaded
		/// </summary>
		/// <param name="asyncOperation"></param>
		public void OnceSceneIsLoaded(AsyncOperation asyncOperation)
		{
			if (allAsyncOperations.Any(singleAsyncOperation => !singleAsyncOperation.isDone))
			{
				return;
			}
			
			SceneManager.SetActiveScene(SceneManager.GetSceneByPath(lightingScene));
			activeLightingScene = lightingScene;

            LightProbes.Tetrahedralize();
			
			// Adds the callback to the node executor
			sceneNodeFromGraphExecutorComponent = Object.FindObjectOfType<SceneGraphExecutorComponent>();
			sceneNodeFromGraphExecutorComponent.AddListener(EndScene);
		}
		
		public void OnceSceneIsUnLoaded(AsyncOperation asyncOperation)
		{
			CompleteNode();
		}

		private List<AsyncOperation> allAsyncOperations;
		
		public MainGraphExecutorComponent MainGraphExecutorComponent
		{
			get
			{
				if (null == mainGraphCache)
				{
					mainGraphCache = UnityEngine.Object.FindObjectOfType<MainGraphExecutorComponent>();
				}
				return mainGraphCache;
			}
		}

		private MainGraphExecutorComponent mainGraphCache;

		public void LoadSceneList()
		{
			// Sets up the lighting
			if (!lightingScene.Equals(activeLightingScene))
			{
				LightmapSettings.lightProbes = null;
				LightmapSettings.lightmaps = null;
			}
			
			// gets the base scene
			string baseScenePath = MainGraphExecutorComponent.gameObject.scene.path;

			List<string> allLoadedScenes = GetAllLoadedScenes();

			allAsyncOperations = new List<AsyncOperation>();

			// this will unload all the scene that are not in the list of scenes to load
			foreach (string sceneToUnload in allLoadedScenes)
			{
				if (sceneToUnload == baseScenePath || additiveScenePaths.Contains(sceneToUnload))
				{
					continue;
				}
				allAsyncOperations.Add(SceneManager.UnloadSceneAsync(SceneManager.GetSceneByPath(sceneToUnload)));
			}
			
			allAsyncOperations.Add(SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive));

			//Loads the scene
			foreach (string sceneToLoad in additiveScenePaths)
			{
				if (!allLoadedScenes.Contains(sceneToLoad))
				{
					allAsyncOperations.Add(SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive));
				}
			}

			foreach (AsyncOperation asyncOperation in allAsyncOperations)
			{
				asyncOperation.completed += OnceSceneIsLoaded;
			}
		}

		/// <summary>
		/// Returns list of scene paths of all loaded scenes 
		/// </summary>
		/// <returns></returns>
		public static List<string> GetAllLoadedScenes()
		{
			// this will get all the scenes that are loaded
			List<string> allLoadedScenes = new();
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				allLoadedScenes.Add(SceneManager.GetSceneAt(i).path);
			}

			return allLoadedScenes;
		}
	}
}
