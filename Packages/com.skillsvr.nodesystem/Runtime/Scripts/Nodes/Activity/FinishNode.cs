using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable, NodeMenuItem("Functions/Finish Node", typeof(SceneGraph)), NodeMenuItem("Functions/Finish Node", typeof(SubGraph))]
public class FinishNode : ExecutableNode
{
	public override string name => "Finish Node";
	public override string icon => "";
	public override Color color => NodeColours.Other;
	public override int Width => MEDIUM_WIDTH;

	[Input(name = "Node to End")]
	public ConditionalLink nodeToEnd = new();

	protected override void OnStart()
	{
		IEnumerable<ExecutableNode> nodes = GetConnectedNodes(this, "nodeToEnd");

		if (nodes == null)
		{
			CompleteNode();
			return;
		}

		foreach (var node in nodes)
		{
			node.SetNodeActiveState(false);
		}

		CompleteNode();
	}

	private IEnumerable<ExecutableNode> GetConnectedNodes(BaseNode nodeFired, string link)
	{
		// Return all the nodes connected to the executes port
		IEnumerable<ExecutableNode> allNodes = nodeFired.inputPorts.FirstOrDefault(n => n.fieldName == link)
			?.GetEdges().Select(e => e.outputNode as ExecutableNode);
		return allNodes;
	}
}