using GraphProcessor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
    [NodeCustomEditor(typeof(ExitGroupNode))]
    public class ExitGroupNodeView : BaseNodeView
    {
        public ExitGroupNode ExitGroupNode => AttachedNode<ExitGroupNode>();

        public override void Enable()
        {
            //inputPortViews
            base.Enable();
        }

        
        
    }
}
