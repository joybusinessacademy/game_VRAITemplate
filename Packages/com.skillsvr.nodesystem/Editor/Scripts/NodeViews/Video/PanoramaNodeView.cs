using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Scripts.VisualElements;
using SkillsVR.VideoPackage;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(PanoramaVideoNode))]
	public class PanoramaNodeView : BaseNodeView
	{
#if PANORAMA_VIDEO
		public PanoramaVideoNode AttachedNode => nodeTarget as PanoramaVideoNode;

		public override VisualElement GetNodeVisualElement()
		{
			return null;
		}

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new();
			visualElement.RegisterCallback<ChangeEvent<float>>(_ => RefreshPorts());

			visualElement.Add(new VideoVisualElement(AttachedNode, owner, RefreshNode));
			visualElement.Add(BuildVideoEventUI());
			return visualElement;

		}

		private VisualElement BuildVideoEventUI()
		{
			ListDropdown<PanoramaVideoNode.PanoramaMechanicEventData> allAnswersContainer = new("Mechanic Events", AttachedNode.panoramaMechanicEventDatas, QuestionBox, 10);
			allAnswersContainer.onRefresh += RefreshNode;
			allAnswersContainer.onRefresh += UpdateAnswerPorts;
			EditorCoroutineUtility.StartCoroutineOwnerless(DelayedUpdatePorts());

			return allAnswersContainer;
		}
		
		IEnumerator DelayedUpdatePorts()
		{
			yield return new EditorWaitForSeconds(0.05f);
		}

		private ListDropdown<PanoramaVideoNode.PanoramaMechanicEventData> AllAnswersContainer => controlsContainer.Q<ListDropdown<PanoramaVideoNode.PanoramaMechanicEventData>>();

		private VisualElement QuestionBox(PanoramaVideoNode.PanoramaMechanicEventData question)
		{
			int questionIndex = AttachedNode.panoramaMechanicEventDatas.IndexOf(question);

			VisualElement questionContainer = new();

			FloatField numberLabel = new("Event " + (questionIndex + 1) + " Time")
			{
				style =
				{
					flexDirection = FlexDirection.Row,
					flexGrow = 1
				},
				value = AttachedNode.panoramaMechanicEventDatas[questionIndex].mechanicEventTime
			};

			numberLabel.RegisterValueChangedCallback(number =>
			{
				float eventTime = number.newValue < 0 ? 0 : number.newValue;
				AttachedNode.panoramaMechanicEventDatas[questionIndex].mechanicEventTime = eventTime;
			});

			questionContainer.Add(numberLabel);

			questionContainer.Add(new Divider());

			IconButton delete = new("Close", 16, 1, true)
			{
				tooltip = "Delete",
			};

			delete.clicked += () =>
			{
				bool requireRefresh = this.ResetEdgeFieldNameBeforeRemoveDynamicPortItem(questionIndex);
				DisconnectPort(AttachedNode.GetOutputPortNameByIndex(questionIndex));
				AttachedNode.panoramaMechanicEventDatas.Remove(question);
				RefreshNode();
				UpdateAnswerPorts();
				if (requireRefresh)
				{
					owner?.SaveGraphToDisk();
					owner?.Refresh();
				}
			};

			questionContainer.Add(delete);

			return questionContainer;
		}

		public override bool RefreshPorts()
		{
			bool returnValue = base.RefreshPorts();

			UpdateAnswerPorts();

			return returnValue;
		}

		private void UpdateAnswerPorts()
		{
			if(AttachedNode == null)
				return;

			List<PortView> allPorts = new();
			for (int i = 0; i < outputContainer.childCount; i++)
			{
				List<PortView> port = GetPortViewsFromFieldName(AttachedNode.GetOutputPortNameByIndex(i));
				if (port != null)
				{
					allPorts.AddRange(port);
				}
			}

			foreach (PortView port in allPorts)
			{
				port.portName = "";
			}

			for (int index = 0; index < AttachedNode?.panoramaMechanicEventDatas?.Count; index++)
			{
				int minutes = (int)(AttachedNode.panoramaMechanicEventDatas[index].mechanicEventTime / 60);
				int seconds = (int)(AttachedNode.panoramaMechanicEventDatas[index].mechanicEventTime % 60);

				allPorts[index].portName = minutes + ":" + seconds.ToString("00");
			}
			
			foreach (PortView port in allPorts)
			{
				port.UpdatePortSize();
			}
			outputContainer.MarkDirtyRepaint();
		}
#endif
	}
}
