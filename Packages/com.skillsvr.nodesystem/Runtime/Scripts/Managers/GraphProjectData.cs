using GraphProcessor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
[CreateAssetMenu(menuName = "SkillsVR/GraphProjectData")]
public class GraphProjectData : ScriptableObject
{
	[SerializeField]
	private bool currentActiveProject = false;
	[SerializeField]
	public ProjectSceneData mainGraphData;
	[SerializeField]
	public List<ProjectSceneData> sceneGraphs;
	[SerializeField]
	public PackageNameScriptable packageNameScriptable;
	[SerializeField]
	public BrandingScriptable brandingScriptable;

	public string GetProjectName => Path.GetFileNameWithoutExtension(mainGraphData?.GetGraphScenePath);

	public bool CurrentActiveProject
	{
		get
		{
			#if UNITY_EDITOR
			if (mainGraphData?.GetGraphScenePath == null)
			{
				currentActiveProject = false;
				return false;
			}

			currentActiveProject = EditorBuildSettings.scenes.FirstOrDefault(scene => scene.enabled)?.path == mainGraphData?.GetGraphScenePath;
			#endif
			return currentActiveProject;
		}
		set => currentActiveProject = value;
	}
}

[Serializable]
public class ProjectSceneData
{
	[SerializeField]
	public string graphTitle = "";
	
	public string GraphName => Path.GetFileNameWithoutExtension(GetGraphScenePath);
	
	public string GetGraphScenePath
	{
		get
		{
			#if UNITY_EDITOR
			graphScenePath = AssetDatabase.GetAssetPath(scene);
			#endif
			return graphScenePath;
		}
		set {
			graphScenePath = value;
		}
	}
	
	public string GetGraphAssetPath
	{
		get
		{
#if UNITY_EDITOR
			graphAssetPath = AssetDatabase.GetAssetPath(graphGraph);
#endif
			return graphAssetPath;
		}
	}

#if UNITY_EDITOR
	[SerializeField] 
	public SceneAsset scene;
#endif

	[SerializeField, HideInInspector]
	private string graphScenePath = "";
	[SerializeField]
	public BaseGraph graphGraph;

	[SerializeField, HideInInspector]
	private string graphAssetPath = "";
}
