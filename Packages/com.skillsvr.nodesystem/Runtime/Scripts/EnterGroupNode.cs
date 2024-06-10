using System;
using System.Linq;
using GraphProcessor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
    [Serializable, NodeMenuItem("Enter group - dont click", typeof(BaseGraph))]

    public class EnterGroupNode : ExecutableNode
    {
        public override string name => "Enter";
        public override Color color => NodeColours.Start;
        public override bool isLocked => true;
        public override bool deletable => false;
        public override bool SolidColor => true;

        Group AttachedGroup => graph.groups.First(group => (group as LogicGroup)?.enterGroupNodeGUID == GUID);
        
        
		protected override void OnStart()
		{
			foreach (BaseNode baseNode in Group.GetInnerNodes(AttachedGroup, graph))
			{
				if (baseNode is ExecutableNode nodeToStart)
				{
					nodeToStart.Initialise();
				}
			}
			
			CompleteNode();
		}


	}
}
