using System.Collections.Generic;
using GraphProcessor;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts.Nodes;

namespace SkillsVRNodes
{
	public class SceneNodeExecutor : NodeExecutor
	{
		private List<StartNode> startNodeList;
		private List<GoToOutNode> goToStartNodeList;

		public List<GoToOutNode> GoToStartNodeList => goToStartNodeList;

		
		public SceneNodeExecutor(MainGraph baseGraph) : base(baseGraph)
		{
			graph = baseGraph;
		}


		/// <summary>
		/// Used to initally start the scene node executor game object
		/// </summary>
		// [RuntimeInitializeOnLoadMethod]
		// public static void InitializeObject()
		// {
		// 	if (SceneGraph.GetSceneGraph() == null)
		// 	{
		// 		Debug.LogWarning("PROJECT NOT SET UP WITH SCENE GRAPH");
		// 		return;
		// 	}
		// 	if (!SceneGraph.GetSceneGraph().autoRunGraph)
		// 	{
		// 		return;
		// 	}
		// 	
		// 	SceneManager.LoadScene(0);
		//
		// 	GameObject sceneNodeExecutorGO = new()
		// 	{
		// 		name = "SceneNodeExecutor"
		// 	};
		// 	sceneNodeExecutorGO.AddComponent<SceneNodeComponent>();
		// }
	}
}