using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.Managers.Utility
{
	/// <summary>
	/// For Indexing all the scenes and graphs
	/// Is used by the Graph Navigator and Scene Node
	/// </summary>
	public static class GraphFinder
	{
		private static GraphProjectData currProject;
        public static GraphProjectData CurrentActiveProject
		{
			get
			{
				if (null == currProject || currProject.CurrentActiveProject == false)
				{
					currProject =  GetAllGraphData()?.FirstOrDefault(x => x.CurrentActiveProject);
                }

				//Still null or no current project found
				if (null == currProject || currProject.CurrentActiveProject == false)
				{
					string buildZeroPath = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.enabled)?.path;

					if(!string.IsNullOrEmpty(buildZeroPath))
					{
						GraphProjectData searchViaScene = GetAllGraphData()?.FirstOrDefault(x => x.mainGraphData.GetGraphScenePath == buildZeroPath);

						if (searchViaScene != null)
						{
							currProject = searchViaScene;
							currProject.CurrentActiveProject = true;
						}
					}
				}
				return currProject;
			}
			set
			{
				if (value == currProject)
				{
					return;
				}
				if (null != currProject)
				{
					currProject.CurrentActiveProject = false;
					currProject.Save();
				}
				currProject = value;
				if (null != currProject)
				{
					currProject.CurrentActiveProject = true;
					currProject.Save();
				}
			}
		}

		public static List<string> EnvironmentScenes => AssetDatabase.FindAssets("l:environment t:scene").Select(AssetDatabase.GUIDToAssetPath).ToList();

		public static BaseGraph GetDefaultGraphByScenePath(string scenePath)
		{
			string graphPath = GraphScenePathExtensions.GetDefaultGraphAssetPathFromScenePath(scenePath);
			return LoadAssetAtPath<BaseGraph>(graphPath);
		}
		

		/// <summary>
		/// Finds all the graphs in the scene 
		/// </summary>
		/// <param name="scenePath"></param>
		/// <returns></returns>
		/// <remarks>WARNING: This can be expensive if ran on a large scene</remarks>
		public static List<string> GetGraphsInScene<GraphType>(string scenePath) where GraphType : BaseGraph
		{
			if (!File.Exists(scenePath))
			{
				return new List<string>();
			}

			List<string> guids = AssetDatabaseFileCache.GetAssetGUIDsOfType<GraphType>();
			List<string> graphs = new List<string>();
			
			// Loads scene Data to be read
			string contents = File.ReadAllText(scenePath);

            foreach (string guid in guids.Where(guid => contents.Contains(guid)))
			{
				graphs.Add(AssetDatabase.GUIDToAssetPath(guid));
			}

			//TODO: Duplicate type of code - Maybe change place of this (Could add to a listener)
			if(graphs.Count == 0)
			{
				AssetDatabaseFileCache.RegenCacheType<GraphType>();
				
				foreach (string guid in guids.Where(guid => contents.Contains(guid)))
				{
					graphs.Add(AssetDatabase.GUIDToAssetPath(guid));
				}
			}

			return graphs;
		}

		public static IEnumerable<string> FindAssetsPathOfType<T>()
		{
			string[] guids = AssetDatabase.FindAssets("t: " + typeof(T).Name);
			return guids.Select(g => AssetDatabase.GUIDToAssetPath(g));
		}

		public static string GetFirstAssetPathOfType<T>()
		{
			return FindAssetsPathOfType<T>().FirstOrDefault();
		}

		public static string PathToName(string path)
		{
			return Path.GetFileName(path)?.Replace(".unity", "");
		}

		public enum SceneGraphError
		{
			None,
			NoSceneSelected,
			ScenePathBroken,
			NoGraphInScene,
			TooManyGraphsInScene,
			SceneMissingComponents,
			WrongSceneType,
		}

		public static SceneGraphError CheckSceneForErrors(string scenePath, out List<string> graphPathList)
		{
			if (string.IsNullOrWhiteSpace(scenePath))
			{
				graphPathList = new List<string>();
				return SceneGraphError.NoSceneSelected;
			}
			if (!File.Exists(scenePath))
			{
				graphPathList = new List<string>();
				return SceneGraphError.ScenePathBroken;
			}
			graphPathList = GetGraphsInScene<SceneGraph>(scenePath);
			switch (graphPathList.Count)
			{
				case 0:
					return SceneGraphError.NoGraphInScene;
				case > 1:
					return SceneGraphError.TooManyGraphsInScene;
			}

			// if (graphs[0].GetType() != typeof(SceneGraph))
			// {
			// 	return SceneGraphError.WrongSceneType;
			// }

			return SceneGraphError.None;
		}

		private const string homeGraphKey = "NodeGraph-HomeGraph";
		private const string currentGraphKey = "NodeGraph-CurrentGraph";

		private const string mainGraphLocation = "Assets/Contexts/Scenes/MainGraphGraph.asset";

		private static Dictionary<string, Object> cachedLoadedAssets = new Dictionary<string, Object>();
		public static MainGraph DefaultMainGraph => LoadOrCreateDefaultMainGraph();

		public static BaseGraph HomeGraph
		{
			get => GetMainGraphFromProjectData();
			set => EditorPrefs.SetString(homeGraphKey, AssetDatabase.GetAssetPath(value));
		}

		private static MainGraph GetMainGraphFromProjectData()
		{
			if (null == CurrentActiveProject)
			{
				return null;
			}
			return (MainGraph)CurrentActiveProject.mainGraphData.graphGraph;
		}

		public static MainGraph LoadOrCreateDefaultMainGraph()
		{
			MainGraph asset = null;

			asset = GetMainGraphFromProjectData();

			if(asset == null)
				asset = LoadAssetAtPath<MainGraph>(mainGraphLocation);

			if (null == asset)
			{ 
				string dir = Path.GetDirectoryName(mainGraphLocation);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}
				asset = ScriptableObject.CreateInstance<MainGraph>();
				AssetDatabase.CreateAsset(asset, mainGraphLocation);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			return asset;
		}

		public static T LoadAssetAtPath<T>(string path, string info = "") where T : Object
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return default(T);
			}

			Object assetObj = null;
			if (cachedLoadedAssets.TryGetValue(path, out assetObj) && null != assetObj && null != assetObj as T)
			{
				return assetObj as T;
			}

			T asset = AssetDatabase.LoadAssetAtPath<T>(path);

			if (null == asset)
			{
				asset = Resources.Load<T>(path);
			}
            if (null == asset)
            {
                // Fallback that if LoadAllAssetsAtPath still return null.
                var all = Resources.FindObjectsOfTypeAll<T>();
                asset = all.LastOrDefault(x => AssetDatabase.GetAssetPath(x) == path);
            }
            if (null == asset)
			{
				// Due to the unity issue LoadAssetAtPath (and LoadMainAsset) may return null when called on play mode start.
				// Use load LoadAllAssetsAtPath instead.
				asset = AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault() as T;
			}

			cachedLoadedAssets.Remove(path);
			cachedLoadedAssets.Add(path, asset);
			
			return asset;
		}

        private static BaseGraph currentGraph;
        public static BaseGraph CurrentGraph
		{
			get
			{
				if (currentGraph != null)
				{
					return currentGraph;
				}

                string assetPath = EditorPrefs.GetString(currentGraphKey);

				if (string.IsNullOrWhiteSpace(assetPath))
				{
					return null;
				}
				BaseGraph assetExist = LoadAssetAtPath<BaseGraph>(assetPath);

				if (assetExist != null)
				{
					currentGraph = assetExist;
					EditorPrefs.SetString(homeGraphKey, AssetDatabase.GetAssetPath(assetExist));
				}
				 
				return assetExist;
			}
			set
			{
				currentGraph = value;
				EditorPrefs.SetString(currentGraphKey, AssetDatabase.GetAssetPath(value));
			}
		}

		public static IEnumerable<MainGraph> LoadAllMainGraphs(bool createDefaultIfEmpty = true)
		{
			List<MainGraph> list = new List<MainGraph>();
			var pathList = FindAssetsPathOfType<MainGraph>();
			foreach(var path in pathList)
			{
				var asset = LoadAssetAtPath<MainGraph>(path);
				if (null != asset)
				{
					list.Add(asset);
				}
			}

			if (createDefaultIfEmpty && 0 == list.Count)
			{
				list.Add(LoadOrCreateDefaultMainGraph());
			}
			return list;
		}

		internal static void OnDeleteAsset(string assetPath)
		{
			if (null == assetPath)
			{
				return;
			}

			cachedLoadedAssets.Remove(assetPath);
		}

		public static List<GraphProjectData> GetAllGraphData()
		{
			string[] guids = AssetDatabase.FindAssets("t:GraphProjectData");

			List<GraphProjectData> graphProjectDatas = guids
				.Select(guid => LoadAssetAtPath<GraphProjectData>(AssetDatabase.GUIDToAssetPath(guid)))
				.Where(scriptableObject => scriptableObject != null)
				.ToList();

			return graphProjectDatas;
		}
		
		public static List<string> GetAllProjectsSceneNames()
		{
			List<string> allSceneNames = new List<string>();

			foreach (var item in GetAllGraphData())
			{
				string mainGraphSceneName = Path.GetFileNameWithoutExtension(item.mainGraphData?.GetGraphScenePath);

				if (!allSceneNames.Contains(mainGraphSceneName))
					allSceneNames.Add(mainGraphSceneName);

				if (item.sceneGraphs.IsNullOrEmpty())
				{
					continue;
				}
				foreach (var sceneItem in item.sceneGraphs)
				{
					string sceneGraphName = Path.GetFileNameWithoutExtension(sceneItem?.GetGraphScenePath);

					if (!allSceneNames.Contains(sceneGraphName))
						allSceneNames.Add(sceneGraphName);
				}
			}

			return allSceneNames;
		}

		public static GraphProjectData GetGraphData(this BaseGraph graph)
		{
			return GetAllGraphData().FirstOrDefault(projectGraphs => projectGraphs.mainGraphData.graphGraph == graph);
		}
	}
}
