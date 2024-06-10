using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVRNodes.Managers.Utility
{
	public static class BaseGraphNodeActivityExtensions
	{
		public static IList<BaseNode> GetAllActiveNodes(this BaseGraph baseGraph)
		{
			if (null == baseGraph || null == baseGraph.Nodes)
			{
				return new List<BaseNode>();
			}

			return baseGraph.Nodes.Where(x => x is ExecutableNode && true == ((ExecutableNode)x).NodeActive).ToList();
		}

		public static MainGraph FindRunningMainGraph(this MainGraph baseGraph)
		{
			return GameObject.FindObjectOfType<MainGraphExecutorComponent>()?.graph;
		}

		public static IEnumerable<BaseGraph> FindAllRunningSceneGraphInstances(this BaseGraph graph)
		{
			return GameObject.FindObjectsOfType<SceneGraphExecutorComponent>().Select(x => x.graph);
		}

		public static BaseGraph FindRunningInstance(this BaseGraph baseGraph)
		{
			if (null == baseGraph)
			{
				return null;
			}
			switch(baseGraph)
			{
				case MainGraph asMainGraph:
					return GameObject.FindObjectsOfType<MainGraphExecutorComponent>().FirstOrDefault(x => asMainGraph == x.graph)?.graph;
				case SceneGraph asSceneGraph:
					return GameObject.FindObjectsOfType<SceneGraphExecutorComponent>().FirstOrDefault(x => asSceneGraph == x.graph)?.graph;
				default: return null;
			}
		}
	}
}
