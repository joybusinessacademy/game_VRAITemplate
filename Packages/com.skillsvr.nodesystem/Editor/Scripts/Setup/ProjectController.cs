using GraphProcessor;
using SkillsVRNodes.Managers.Setup;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ProjectController
{
	public void CheckAllProjectData()
	{
		if (EditorApplication.isPlaying)
			return;

		List<GraphProjectData> graphDatas = new List<GraphProjectData>();
		graphDatas = GraphFinder.GetAllGraphData();

		//No Project Graphs Found
		if (graphDatas.Count == 0)
		{
			//No MainGraphs Found As well
			if (GetMainGraphs().Count > 0)
				GetMainGraphs().ForEach(x => GenerateProjectFilesBasedOnMainGraphs(x));
		}
		else
		{
			//Main Graphs and Project Graphs Not Matching
			if (graphDatas.Count != GetMainGraphs().Count)
			{
				foreach (var item in GetMainGraphs())
				{
					if (!graphDatas.Exists(x => x.mainGraphData.graphGraph == item))
						GenerateProjectFilesBasedOnMainGraphs(item);
				}
			}

			ValidateProjectData(graphDatas);
		}
	}

	private void ValidateProjectData(List<GraphProjectData> graphDatas)
	{
		bool noProjectCurrentlyActive = true;

		foreach (GraphProjectData item in graphDatas)
		{
			if(item.mainGraphData.graphGraph == null)
			{
				Debug.LogError("Missing Main Graph from Project Data");
				return;
			}

			if (item.CurrentActiveProject)
				noProjectCurrentlyActive = false;

			if (item.mainGraphData.graphTitle == "")
				item.mainGraphData.graphTitle = item.mainGraphData.graphGraph.ToString().Replace("Graph", "");

			if (item.mainGraphData.scene == null)
				item.mainGraphData.scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(item.mainGraphData.graphGraph.GetDefaultGraphScenePath());

			if (item.sceneGraphs == null)
				continue;

			foreach (var subSceneData in item.sceneGraphs)
			{
				if (subSceneData.graphTitle == "")
				{
					string title = subSceneData.graphGraph.ToString().Replace("Graph", "");
					subSceneData.graphTitle = title;
				}

				if (subSceneData.scene == null)
				{
					string scenePath = subSceneData.graphGraph.GetDefaultGraphScenePath();
					subSceneData.scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

				}
			}
		}

		if (noProjectCurrentlyActive && null != graphDatas[0])
		{
			GraphFinder.CurrentActiveProject = graphDatas[0];
		}
	}

	private void GenerateProjectFilesBasedOnMainGraphs(MainGraph mainGraph)
	{
		string assetPath = "Assets" + GraphSetupTools.SceneFolder + "/";
		GraphProjectData graphProjectData = null;
		if (null != mainGraph)
		{
			graphProjectData = UnityEngine.ScriptableObject.CreateInstance<GraphProjectData>();
			graphProjectData.name = mainGraph.name;

			int projectID = 0;
			string projectPath = assetPath + "Project" + projectID + ".asset";

			while(File.Exists(projectPath))
			{
				projectID++;
				projectPath = "Assets" + GraphSetupTools.SceneFolder + "/" + "Project" + projectID + ".asset";
			}

			AssetDatabase.CreateAsset(graphProjectData, projectPath);

			graphProjectData.mainGraphData ??= new ProjectSceneData();

			graphProjectData.mainGraphData.graphGraph = mainGraph;
			graphProjectData.mainGraphData.scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(mainGraph.GetDefaultGraphScenePath());
			graphProjectData.mainGraphData.graphTitle = mainGraph.ToString();

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		IEnumerable<SceneNode> newSceneNodes = mainGraph.Nodes.OfType<SceneNode>();

		foreach (SceneNode sceneNode in newSceneNodes)
		{
			BaseGraph graph = GraphFinder.GetDefaultGraphByScenePath(sceneNode.scenePath);

			graphProjectData.sceneGraphs ??= new List<ProjectSceneData>();

			if (!graphProjectData.sceneGraphs.Exists(item => item.graphGraph == graph))
			{
				ProjectSceneData projectSceneData = new ProjectSceneData();
				projectSceneData.graphGraph = graph;
				projectSceneData.scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(mainGraph.GetDefaultGraphScenePath());
				projectSceneData.graphTitle = graph.ToString();
				graphProjectData.sceneGraphs.Add(projectSceneData);
			}
		}

	}

	public static void GenerateSceneProjectData(GraphProjectData graphProjectData)
	{
		IEnumerable<SceneNode> newSceneNodes = graphProjectData.mainGraphData.graphGraph.Nodes.OfType<SceneNode>();

		foreach (SceneNode sceneNode in newSceneNodes)
		{
			if (sceneNode.scenePath == string.Empty)
				continue;

			BaseGraph graph = GraphFinder.GetDefaultGraphByScenePath(sceneNode.scenePath);

			graphProjectData.sceneGraphs ??= new List<ProjectSceneData>();

			if (!graphProjectData.sceneGraphs.Exists(item => item.graphGraph == graph))
			{
				ProjectSceneData projectSceneData = new ProjectSceneData();
				projectSceneData.graphGraph = graph;
				projectSceneData.scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(graph.GetDefaultGraphScenePath());
				projectSceneData.graphTitle = graph.ToString();
				graphProjectData.sceneGraphs.Add(projectSceneData);
			}
		}
	}

	private List<MainGraph> GetMainGraphs()
	{
		string[] guids = AssetDatabase.FindAssets("t:MainGraph");

		List<MainGraph> mainGraphs = guids
			.Select(guid => AssetDatabase.LoadAssetAtPath<MainGraph>(AssetDatabase.GUIDToAssetPath(guid)))
			.Where(scriptableObject => scriptableObject != null)
			.ToList();

		return mainGraphs;
	}

	public void InitOrNewMainGraphAndScene(string mainGraphName)
	{
		if (string.IsNullOrWhiteSpace(mainGraphName))
		{
			mainGraphName = "MainGraphGraph.asset";
		}
		if (!mainGraphName.EndsWith(".asset"))
		{
			mainGraphName += ".asset";
		}

		string assetPath = "Assets" + GraphSetupTools.SceneFolder + "/" + mainGraphName;
		MainGraph graph = AssetDatabase.LoadAllAssetsAtPath(assetPath).FirstOrDefault() as MainGraph;
		GraphProjectData graphProjectData = null;
		if (null == graph)
		{
			graph.InitOrCreateMainGraphScene();
			return;
		}

		graph = UnityEngine.ScriptableObject.CreateInstance<MainGraph>();
		graphProjectData = UnityEngine.ScriptableObject.CreateInstance<GraphProjectData>();

		int projectID = 0;
		string projectPath = "Assets" + GraphSetupTools.SceneFolder + "/" + "Project" + projectID + ".asset";

		while (File.Exists(projectPath))
		{
			projectID++;
			projectPath = "Assets" + GraphSetupTools.SceneFolder + "/" + "Project" + projectID + ".asset";
		}

		AssetDatabase.CreateAsset(graph, assetPath);

		graphProjectData.mainGraphData ??= new ProjectSceneData();

		graphProjectData.mainGraphData.graphGraph = graph;
		graphProjectData.mainGraphData.scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(graph.GetDefaultGraphScenePath());
		graphProjectData.mainGraphData.graphTitle = graph.ToString();

		AssetDatabase.CreateAsset(graphProjectData, projectPath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		graph.InitOrCreateMainGraphScene();
	}
} 
