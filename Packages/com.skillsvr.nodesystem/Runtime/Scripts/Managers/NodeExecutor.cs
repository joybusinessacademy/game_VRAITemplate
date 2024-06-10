using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine;

namespace SkillsVRNodes.Managers
{
	public class NodeExecutor
	{
		public BaseGraph graph;

		public Action onEndAction;

		private List<MainStartNode> mainStartNodeList;
		private List<StartNode> startNodeList;
		private List<GoToOutNode> goToStartNodeList;

		public List<GoToOutNode> GoToStartNodeList => goToStartNodeList;

		public NodeExecutor(BaseGraph baseGraph)
		{
			graph = baseGraph;
		}
		
		public void InitializeGraph()
		{
			mainStartNodeList = graph.Nodes.OfType<MainStartNode>().ToList();
			startNodeList = graph.Nodes.OfType<StartNode>().ToList();
			goToStartNodeList = graph.Nodes.OfType<GoToOutNode>().ToList();

			RunOnAwake();
		}


		private HashSet<BaseNode> successfullyInitializedNodes;
		private List<string> nodesInGroups;
		private void RunOnAwake()
		{
			successfullyInitializedNodes = new HashSet<BaseNode>();
			
			nodesInGroups = new List<string>();

			foreach (Group group in graph.groups)
			{
				if (group is LogicGroup logicGroup)
				{
					nodesInGroups.AddRange(logicGroup.innerNodeGUIDs);
				}
			}
			
			foreach (StartNode executableNode in graph.Nodes.OfType<StartNode>())
			{
				InitConnectedNodes(executableNode);
			}

			foreach (MainStartNode executableNode in graph.Nodes.OfType<MainStartNode>())
			{
				InitConnectedNodes(executableNode);
			}

			foreach (GoToOutNode executableNode in graph.Nodes.OfType<GoToOutNode>())
			{
				InitConnectedNodes(executableNode);
			}
		}

		private void InitConnectedNodes(BaseNode nodeFired)
		{
			if (successfullyInitializedNodes.Contains(nodeFired))
			{
				return;
			}
			successfullyInitializedNodes.Add(nodeFired);
			
			
			if (!nodesInGroups.Contains(nodeFired.GUID))
			{
				ExecutableNode exeNode = nodeFired as ExecutableNode;
				exeNode?.Initialise();
			}

			List<BaseNode> childNodes = (from outputPorts in nodeFired.outputPorts from node in outputPorts.GetEdges() select node.inputNode).ToList();
			foreach (BaseNode childNode in childNodes)
			{
				InitConnectedNodes(childNode);
			}
		}

		public void Start()
		{
			if(startNodeList.Count != 0)
				foreach (StartNode nodes in startNodeList)
				{
					nodes.OnStart(this);
				}
			else if(mainStartNodeList.Count != 0)
				foreach (MainStartNode nodes in mainStartNodeList)
				{
					nodes.OnStart(this);
				}

		}


		private IEnumerable<ExecutableNode> GetConnectedNodes(BaseNode nodeFired, string link)
		{
			// Return all the nodes connected to the executes port
			IEnumerable<ExecutableNode> allNodes = nodeFired.outputPorts.FirstOrDefault(n => n.fieldName == link)
				?.GetEdges().Select(e => e.inputNode as ExecutableNode);
			return allNodes;
		}

		private IEnumerable<SerializableEdge> GetConnectedEdges(BaseNode nodeFired, string link)
		{
			// Return all the nodes connected to the executes port
			IEnumerable<SerializableEdge> allEdges = nodeFired.outputPorts.FirstOrDefault(n => n.fieldName == link)
				?.GetEdges().Where(e => e.inputNode is ExecutableNode);
			return allEdges;
		}

		/// <summary>
		/// WORK AROUND FOR SCENE SKIPPER
		/// </summary>
		public bool isStopped = false;

		public void RunConnectedNodes(BaseNode nodeFired, string link)
		{
			if (isStopped)
			{
				return;
			}

            nodeFired.TriggerOutputPortActiveByName(link);
            IEnumerable<SerializableEdge> edges = GetConnectedEdges(nodeFired, link);

			if (edges == null)
			{
				return;
			}
			IEnumerable<SerializableEdge> conditionalNodes = edges.ToList();
			if (conditionalNodes.ToList().IsNullOrEmpty())
			{
				return;
			}

            foreach (SerializableEdge edge in conditionalNodes)
			{
				ExecutableNode conditionalNode = edge.inputNode as ExecutableNode;
				if (null == conditionalNode)
				{
					continue;
				}
                InitConnectedNodes(conditionalNode);
                try
                {
					bool successStarted = conditionalNode.StartNodeFrom(this, edge);
					if (successStarted)
					{
						conditionalNode.TriggerInputPortActiveByName(edge.inputFieldName);
					}
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
		}
	}
}