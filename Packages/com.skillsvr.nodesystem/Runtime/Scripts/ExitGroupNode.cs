using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using GraphProcessor;

namespace SkillsVRNodes.Scripts.Nodes
{
    [Serializable, NodeMenuItem("Exit group - dont click", typeof(BaseGraph))]

    public class ExitGroupNode : ExecutableNode
    {
        public override string name => "Exit";
        public override Color color => NodeColours.End;
        public override bool isLocked => true;
        public override bool deletable => false;
        public override bool SolidColor => true;


        Group AttachedGroup => graph.groups.First(group => (group as LogicGroup)?.exitGroupNodeGUID == GUID);


        protected override void OnStart()
        {
            foreach (BaseNode baseNode in Group.GetInnerNodes(AttachedGroup, graph))
            {
                if (baseNode is ExecutableNode nodeToFinish)
                {
                    nodeToFinish.StopNode();
                }
            }

            CompleteNode();
        }
    }
}
