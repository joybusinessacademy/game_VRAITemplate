using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
    [CustomDataValidation(typeof(BaseGraphView))]
    public class BaseGraphViewDataValidation : AbstractGraphElementDataValidation<BaseGraphView>
    {
        public override string GetTargetId()
        {
            return  null == TargetVisual || null == TargetVisual.graph ? "" : TargetVisual.graph.UUID.ToString();
        }

        public override VisualElement OnGetVisualSourceFromPath(string path)
        {
            return TargetVisual.Q("heading");
        }

        public override void OnValidate()
        {
            string StartNodeCheckKey = "StartNode";
            string EndNodeCheckKey = "EndNode";
            string ConnectionCheckKey = "Connection";

            if (null == TargetVisual || null == TargetVisual.graph)
            {
                return;
            }
            var graph = TargetVisual.graph;

            ErrorIf(graph.nodes.Where(x => null != x && x is StartNode).Count() <= 0, StartNodeCheckKey, 
                "Graph should have at least 1 Start Node.\r\n" +
                "Right click on graph then select \"Create Node\" -> \"Flow\" -> \"Start\" to add new start node.");

            ErrorIf(graph.nodes.Where(x => null != x && x is EndNode).Count() <= 0, EndNodeCheckKey,
                "Graph should have at least 1 End Node.\r\n" +
                "Right click on graph then select \"Create Node\" -> \"Flow\" -> \"End\" to add new end node.");

            ErrorIf(!AnyStartNodeConnectToEndNode(graph), ConnectionCheckKey,
                "Graph cannot play from any start node to end node. Check graph connection.");
        }

        protected bool AnyStartNodeConnectToEndNode(BaseGraph graph)
        {
            return graph.nodes.Any(x => null != x && x is EndNode && null != GetConnectedStart(graph, x));
        }

        protected static BaseNode GetConnectedStart(BaseGraph g, BaseNode node)
        {
            if (null == g || null == node)
            {
                return null;

            }

            if (node is StartNode)
            {
                return null;
            }

            HashSet<BaseNode> visitedNodes = new HashSet<BaseNode>();
            Stack<BaseNode> stack = new Stack<BaseNode>();

            stack.Push(node);
            visitedNodes.Add(node);

            while (stack.Count > 0)
            {
                BaseNode currentNode = stack.Pop();

                if (currentNode is StartNode)
                {
                    return currentNode;
                }

                // Explore the neighboring nodes
                foreach (var edge in g.edges)
                {
                    if (edge.inputNode == currentNode && !visitedNodes.Contains(edge.outputNode))
                    {
                        visitedNodes.Add(edge.outputNode);
                        stack.Push(edge.outputNode);
                    }
                }
            }

            return null;
        }
    }
}

