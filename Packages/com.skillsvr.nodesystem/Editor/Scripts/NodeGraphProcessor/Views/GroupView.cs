using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using Scripts.VisualElements;
using SkillsVRNodes.Editor.NodeViews;
using System;

namespace GraphProcessor
{
    public class GroupView : UnityEditor.Experimental.GraphView.Group, IDisposable
	{
		public BaseGraphView	owner;
		public Group		    group;

        Label                   titleLabel;
        ColorField              colorField;

        readonly string         groupStyle = "GraphProcessorStyles/GroupView";

        public GroupView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));
		}
		
		private static void BuildContextualMenu(ContextualMenuPopulateEvent evt) {}
		
		public virtual void Initialize(BaseGraphView graphView, Group block)
		{
			if (block == null)
			{
				return;
			}
			
			group = block;
			owner = graphView;

            title = block.title;
            SetPosition(block.position);
			
			this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
			
            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
            titleLabel = headerContainer.Q<Label>();

            colorField = new ColorField{ value = group.color, name = "headerColorPicker" };
            colorField.RegisterValueChangedCallback(e =>
            {
                UpdateGroupColor(e.newValue);
            });
            UpdateGroupColor(group.color);

            headerContainer.Add(colorField);

            InitializeInnerNodes();
		}

        public virtual void Dispose()
        {
            this.owner = null;
            this.group = null;
        }

        public void SetInteractable(bool interactable)
        {
            this.EnableInputBlocker(!interactable);
        }

        void InitializeInnerNodes()
        {
	        List<BaseNodeView> baseNodeViews = new();
            foreach (string nodeGUID in group.innerNodeGUIDs.ToList())
            {
                if (!owner.graph.nodesPerGUID.ContainsKey(nodeGUID))
                {
                    Debug.LogWarning("Node GUID not found: " + nodeGUID);
                    group.innerNodeGUIDs.Remove(nodeGUID);
                    continue ;
                }
                BaseNode node = owner.graph.nodesPerGUID[nodeGUID];
                BaseNodeView nodeView = owner.nodeViewsPerNode[node];

                baseNodeViews.Add(nodeView);
            }
            AddElements(baseNodeViews);
        }

        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
	        if (element is BaseNodeView and (EnterGroupNodeView or ExitGroupNodeView))
	        {
		        reasonWhyNotAccepted = "Can't add the entry / exit nodes to itself";
		        return false;
	        }
	        return base.AcceptsElement(element, ref reasonWhyNotAccepted);
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            foreach (GraphElement element in elements)
            {
	            if (element is not BaseNodeView node)
                {
	                continue;
                }

                if (!group.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
                {
	                group.innerNodeGUIDs.Add(node.nodeTarget.GUID);
                }
            }
            
            
            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            // Only remove the nodes when the group exists in the hierarchy
            if (parent != null)
            {
                foreach (var elem in elements)
                {
                    if (elem is BaseNodeView nodeView)
                    {
                        group?.innerNodeGUIDs.Remove(nodeView.nodeTarget.GUID);
                    }
                }
            }

            base.OnElementsRemoved(elements);
        }

        public void UpdateGroupColor(Color newColor)
        {
            group.color = newColor;
            style.backgroundColor = newColor;
        }

        void TitleChangedCallback(ChangeEvent< string > e)
        {
            group.title = e.newValue;
        }

		public override void SetPosition(Rect newPos)
		{
			base.SetPosition(newPos);

			group.position = newPos;
		}
	}
}