using GraphProcessor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillsVRGraphModifyView
{
	float scrollSpeedScaler = 15f;
	Vector3 upScroll = new Vector3(0, 1, 0);
	Vector3 downScroll = new Vector3(0, -1, 0);
	Vector3 leftScroll = new Vector3(1, 0, 0);
	Vector3 rightScroll = new Vector3(-1, 0, 0);
	Vector3 scrollToApply = Vector3.zero;
	bool zooming;
	public void HandleEvent(EventBase evt, BaseGraphView view)
	{
		if (evt.GetType() == typeof(KeyDownEvent))
		{
			if (((KeyDownEvent)evt).keyCode == KeyCode.LeftArrow)
			{
				scrollToApply += (leftScroll * scrollSpeedScaler);
			}

			if (((KeyDownEvent)evt).keyCode == KeyCode.RightArrow)
			{
				scrollToApply += (rightScroll * scrollSpeedScaler);
			}

			if (((KeyDownEvent)evt).keyCode == KeyCode.UpArrow)
			{
				scrollToApply += (upScroll * scrollSpeedScaler);
			}

			if (((KeyDownEvent)evt).keyCode == KeyCode.DownArrow)
			{
				scrollToApply += (downScroll * scrollSpeedScaler);
			}

			if(view.graph)
				view.UpdateViewTransform(view.graph.position + scrollToApply, view.graph.scale);
		}

		if (evt.GetType() == typeof(WheelEvent))
		{
			if (((WheelEvent)evt).delta.y < 0)
			{
				scrollToApply += ((((WheelEvent)evt).shiftKey ? leftScroll : upScroll) * scrollSpeedScaler);

			}
			else
			{
				scrollToApply += ((((WheelEvent)evt).shiftKey ? rightScroll : downScroll) * scrollSpeedScaler);
			}

			if (((WheelEvent)evt).ctrlKey && !zooming)
			{
				view.SetupZoom(0.05f, 2f, 0.2f, view.scale);
				zooming = true;
				scrollToApply = Vector3.zero;
			}

			if (!((WheelEvent)evt).ctrlKey && zooming)
			{
				view.SetupZoom(0.05f, 2f, 0.0f, view.scale);
				zooming = false;
			}

			if(view.graph)
				view.UpdateViewTransform(view.graph.position + scrollToApply, view.graph.scale);
		}


		scrollToApply = Vector3.zero;

		//base.HandleEvent(evt);
	}
}
