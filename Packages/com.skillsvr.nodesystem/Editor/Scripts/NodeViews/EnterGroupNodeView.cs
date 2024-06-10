using GraphProcessor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
    [NodeCustomEditor(typeof(EnterGroupNode))]
    public class EnterGroupNodeView : BaseNodeView
    {

        public EnterGroupNode EnterGroupNode => AttachedNode<EnterGroupNode>();

        public override void Enable()
        {
            base.Enable();
        }

        public override void OnRemoved()
        {
            base.OnRemoved();
        }
    }
}
