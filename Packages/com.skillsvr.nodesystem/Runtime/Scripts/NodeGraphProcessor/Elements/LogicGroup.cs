using System;
using System.Linq;
using UnityEngine;
using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;

namespace GraphProcessor
{
    public class ConnectToOuterNode
    {
        public List<NodePort> nodePortsInput = new List<NodePort>();
        public List<NodePort> nodePortsOutput = new List<NodePort>();
    }

    [Serializable]
    public class LogicGroup : Group
    {
        public string enterGroupNodeGUID;
        public string exitGroupNodeGUID;


        public ConnectToOuterNode entryOnCopy = new ConnectToOuterNode();
        public ConnectToOuterNode exitOnCopy = new ConnectToOuterNode();
        public EnterGroupNode GetEnterGroupNode(BaseGraph graph)
        {
            return graph.nodes.FirstOrDefault(node => node.GUID == enterGroupNodeGUID) as EnterGroupNode;
        }
        public ExitGroupNode GetExitGroupNode(BaseGraph graph)
        {
            return graph.nodes.FirstOrDefault(node => node.GUID == exitGroupNodeGUID) as ExitGroupNode;
        }
        
        public LogicGroup(string title, Vector2 position) : base(title, position)
        {
        }
        public LogicGroup() {
           
        }

    }
}
