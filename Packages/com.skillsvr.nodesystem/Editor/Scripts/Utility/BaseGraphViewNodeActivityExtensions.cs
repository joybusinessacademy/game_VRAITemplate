using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;

namespace SkillsVRNodes.Managers.Utility
{
	public static class BaseGraphViewNodeActivityExtensions
	{
		public static void StartTrackingNodeActivity(this BaseGraphView graphView)
		{
			if (null == graphView)
			{
				return;
			}
			graphView.RefreshNodeActivityEffectFromRunningGraph();
			BaseNode.customNodeEvent += graphView.OnReceiveNodeEvent;
			graphView.onDispose += graphView.StopTrackingNodeActivity;
		}

		public static void StopTrackingNodeActivity(this BaseGraphView graphView)
		{
			if (null == graphView)
			{
				return;
			}
			BaseNode.customNodeEvent -= graphView.OnReceiveNodeEvent;
			graphView.ClearAllNodeActivityEffects();
			graphView.onDispose -= graphView.StopTrackingNodeActivity;
		}

		private static void OnReceiveNodeEvent(this BaseGraphView baseGraphView, BaseNode node, object eventKey, object data)
		{
			if (null == baseGraphView
				|| null == node
				|| !(data is bool)
				|| !(eventKey is string))
			{
				return;
			}
			if (eventKey as string != nameof(ExecutableNode.NodeActive))
			{
				return;
			}

			bool isOn = (bool)data;
			baseGraphView.SetupNodeActivityEffect(node, isOn);
		}

		public static void RefreshNodeActivityEffectFromRunningGraph(this BaseGraphView graphView)
		{
			if (!EditorApplication.isPlayingOrWillChangePlaymode)
			{
				return;
			}
			var runningGraph = graphView?.graph?.FindRunningInstance();
			var activeNodes = runningGraph?.GetAllActiveNodes();
			activeNodes?.ForEach(x => SetupNodeActivityEffect(graphView, x, true));
		}

		public static void SetupNodeActivityEffect(this BaseGraphView graphView, BaseNode node, bool isOn)
		{
			if (null == graphView
				|| null == node
				|| string.IsNullOrWhiteSpace(node.GUID))
			{
				return;
			}

			BaseNodeView nodeView = null;
			graphView.nodeViewsPerGUID.TryGetValue(node.GUID, out nodeView);
			nodeView?.SetDefaultBorderStyleByActive(isOn);
		}

		public static void ClearAllNodeActivityEffects(this BaseGraphView graphView)
		{
			if (null == graphView)
			{
				return;
			}
			graphView?.nodeViews?.ForEach(x => x.SetDefaultBorderStyleByActive(false));
		}
	}
}
