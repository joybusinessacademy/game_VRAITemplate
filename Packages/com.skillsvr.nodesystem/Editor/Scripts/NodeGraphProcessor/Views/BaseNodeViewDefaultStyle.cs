using UnityEngine;
using Scripts.VisualElements;
using System.Collections.Generic;
using UnityEngine.UIElements;
using SkillsVRNodes.Editor.NodeViews;
using System;

namespace GraphProcessor
{
    public static class BaseNodeViewDefaultStyles
	{
		public static Color DEFAULT_NODE_DONE_BORDER_COLOR { get; set; } = new Color(0.4f, 0.78f, 0.37f);
		public static int DEFAULT_NODE_DONE_BORDER_WIDTH { get; set; } = 5;

        const string BORDER_ELEMENT_NAME = "runtime_active_border";
        public static void SetDefaultBorderStyleByActive(this VisualElement nodeView, bool active)
        {
            var borderView = nodeView.Q(BORDER_ELEMENT_NAME);
            if (active && null == borderView)
            {
                borderView = new VisualElement();
                borderView.name = BORDER_ELEMENT_NAME;
                borderView.style.position = Position.Absolute;
                borderView.StopResponseKeyAndMouseEvents();
                nodeView.Add(borderView);

                Action callback = () =>{
					borderView.SendToBack();
					borderView.CopySizeFrom(nodeView);
					int top = DEFAULT_NODE_DONE_BORDER_WIDTH * 4;
					borderView.SetBorderStyleAsDone(DEFAULT_NODE_DONE_BORDER_COLOR, top, 0, true);
					borderView.style.top = borderView.style.top.value.value - top + 2;
					borderView.style.height = borderView.style.height.value.value + top;
				};

                if (!nodeView.IsRenderReady())
                {
					nodeView.ExecOnceOnRenderReady(() => {
                        // Node view sometimes init property view later than first geometry change, so update border on 2nd change as well.
                        nodeView.ExecOnceOnEvent<GeometryChangedEvent>((evt) => { callback.Invoke(); });
					});
				}
                else
                {
					callback();
				}
                
			}
            borderView?.SetDisplay(active);
		}

        public static void SetBorderStyleAsDone(this VisualElement nodeView, Color color, int widthTop, int widthOther, bool roundBorder = true)
        {
            nodeView.style.SetBorderColor(color);
            nodeView.style.SetBorderWidth(widthOther, widthTop, widthOther, widthOther, roundBorder);
        }

        public static void ClearBorder(this VisualElement nodeView)
        {
            nodeView.style.SetBorderColor(Color.clear);
            nodeView.style.SetBorderWidth(0, 0, 0, 0, false);
        }

        public static void CleanAllBorders(this IEnumerable<VisualElement> baseNodeViews)
        {
            baseNodeViews.ForEach(ClearBorder);
        }
    }
}