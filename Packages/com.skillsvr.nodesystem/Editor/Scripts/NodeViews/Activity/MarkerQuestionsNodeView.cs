using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DialogExporter;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(MarkerQuestionsNode))]
	public class MarkerQuestionsNodeView : SpawnerNodeView<SpawnerMarkerQuestion, IMarkerQuestionSystem, MarkerQuestionsData>
	{
		public MarkerQuestionsNode AttachedNode => nodeTarget as MarkerQuestionsNode;

		private ListDropdown<MarkerData> Markers => controlsContainer.Q<ListDropdown<MarkerData>>();

		public override void Enable()
		{
			base.Enable();
			EditorCoroutineUtility.StartCoroutineOwnerless(DelayedUpdatePorts());
		}

		protected override void OnRefresh()
		{
			base.OnRefresh();
			UpdateAnswerPorts();
		}

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new();
			visualElement.RegisterCallback<ChangeEvent<string>>(_ => RefreshPorts());

			ListDropdown<MarkerData> markers = new("Answers", AttachedNode.MechanicData.markerDatas, MarkerBox, 10);
			markers.onRefresh += UpdateAnswerPorts;
			visualElement.Add(markers);

			visualElement.Add(NextButtonLocation());
			visualElement.Add(new Divider());

			EnumField feedbackTypeField = new("Select Feedback Type: ", AttachedNode.MechanicData.markerQuestionFeedback);

			feedbackTypeField.RegisterValueChangedCallback((evt) =>
			{
				AttachedNode.MechanicData.markerQuestionFeedback = (MarkerQuestionFeedback)evt.newValue;
				RefreshNode();
			});

			visualElement.Add(feedbackTypeField);
			
			EditorCoroutineUtility.StartCoroutineOwnerless(DelayedUpdatePorts());


			MarkerQuestionsNode spawnerNode = AttachedNode;
			
			visualElement.Add(spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.checkStraightAway)));
			visualElement.Add(spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.untoggleOthers)));


			if (spawnerNode.MechanicData.markerQuestionFeedback == MarkerQuestionFeedback.NOFEEDBACK)
			{
				visualElement.Add(spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.hideMarkerOnReset)));
			}
			else
			{
				spawnerNode.MechanicData.hideMarkerOnReset = false;
			}

			return visualElement;
		}

		public override VisualElement GetNodeVisualElement()
		{
			VisualElement visualElement = new();

			return visualElement;
		}

		IEnumerator DelayedUpdatePorts()
		{
			yield return new EditorWaitForSeconds(0.05f);
			UpdatePorts();
		}

		void UpdatePorts()
		{
			if (null == AttachedNode || null == AttachedNode.MechanicData)
			{
				return;
			}
			if (AttachedNode.MechanicData?.markerDatas == null)
				return;

			int anyMarkedCorrect = AttachedNode.MechanicData.markerDatas.FindAll(t => t.isCorrectMarker).Count;
			if (GetPortViewsFromFieldName("Correct") != null)
			{
				GetPortViewsFromFieldName("Correct").ForEach(k =>
				{
					k.SetEnabled(anyMarkedCorrect > 0);
					k.GetEdges().ForEach(edge => edge.style.display = anyMarkedCorrect == 0 ? DisplayStyle.None : DisplayStyle.Flex);
				});
			}
			if (GetPortViewsFromFieldName("Incorrect") != null)
			{
				GetPortViewsFromFieldName("Incorrect").ForEach(k =>
				{
					k.SetEnabled(anyMarkedCorrect > 0);
					k.GetEdges().ForEach(edge => edge.style.display = anyMarkedCorrect == 0 ? DisplayStyle.None : DisplayStyle.Flex);
				});
			}
		}


		private VisualElement NextButtonLocation()
		{
			VisualElement transformDropdowns = new();
			PropDropdown<IPropPanel> sceneElementDropdown = new ("Submit Button Position: ", AttachedNode.lockInButtonProp, newValue =>
			{
				if (newValue != null)
				{
					AttachedNode.MechanicData.lockInButtonLocation = PropManager.GetProp(newValue)?.GetTransform();
					AttachedNode.lockInButtonProp = newValue;
				}
			}, true, typeof(PanelProp));


			transformDropdowns.Add(sceneElementDropdown);

			transformDropdowns.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
			return transformDropdowns;
		}

		private VisualElement MarkerBox(MarkerData markerData)
		{

			VisualElement questionContainer = new ();

			Toggle toggle = new ()
			{
				label = "Correct: ",
				value = markerData.isCorrectMarker,
				tooltip = "Is correct answer"
			};
			toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
			{
				AttachedNode.MechanicData.markerDatas.Find(t => t == markerData).isCorrectMarker = evt.newValue;

				RefreshNode();
				
				UpdatePorts();
			});
			questionContainer.Add(toggle);

			int index = AttachedNode.MechanicData.markerDatas.IndexOf(markerData);
			while (AttachedNode.allGameObjects.Count <= index)
			{
				AttachedNode.allGameObjects.Add(new PropGUID<IPropPanel>());
			}

			PropGUID<IPropPanel> currentValue = AttachedNode.allGameObjects[index];
			PropDropdown<IPropPanel> sceneElementDropdown = new ("Position: ", currentValue, newValue =>
			{
				AttachedNode.allGameObjects[index] = newValue;
				// we use first index as our spawnposition to have a parent
				//This should not be the spawnPosition
				//if (index == 0)
				//	AttachedNode.spawnPosition = AttachedNode.allGameObjects[index];
			},true, typeof(PanelProp));

			questionContainer.Add(sceneElementDropdown);

			questionContainer.Add(markerData.customTextForMarker.LocField("Custom Text:"));

			Toggle customSpriteToggle = markerData.CustomToggle(nameof(markerData.useCustomSprite), "Use Custom Sprite");
			customSpriteToggle.RegisterCallback<ChangeEvent<bool>>(_ => RefreshNode());
			questionContainer.Add(customSpriteToggle);

			ObjectField spriteField = new("Custom Sprite: ")
			{
				objectType = typeof(Sprite),
				value = markerData.changeMarkerSprite
			};
			spriteField.RegisterValueChangedCallback(evt =>
			{
				markerData.changeMarkerSprite = evt.newValue as Sprite;
			});

			if (markerData.useCustomSprite)
				questionContainer.Add(spriteField);
			
			var space = new VisualElement();
			space.style.flexGrow = 1;
			questionContainer.Add(space);

			IconButton delete = new IconButton("Close")
			{
				tooltip = "Delete",
			};

			delete.clicked += () =>
			{
				bool requireRefresh = this.ResetEdgeFieldNameBeforeRemoveDynamicPortItem(index);
				DisconnectPort(AttachedNode.GetOutputPortNameByIndex(index));
				AttachedNode.MechanicData.markerDatas.RemoveAt(index);
				AttachedNode.allGameObjects.RemoveAt(index);
				
				if (requireRefresh)
				{
					owner?.SaveGraphToDisk();
					owner?.Refresh();
				}
				
				RefreshNode();
			};
			questionContainer.Add(delete);
			return questionContainer;
		}


		public override void OnSelected()
		{
			for (int i = 0; i < AttachedNode.MechanicData.markerDatas.Count; i++)
			{
				MarkerData markerData = AttachedNode.MechanicData.markerDatas[i];
				if (AttachedNode.allGameObjects.Count > i && AttachedNode.allGameObjects[i] != null)
				{
					GameObject spawnPoint = PropManager.GetProp<IPropPanel>(AttachedNode.allGameObjects[i])?.GetPropComponent().gameObject;
					if (spawnPoint != null)
						markerData.spawnPoint = spawnPoint;
				}
			}
			base.OnSelected();
		}
		
		public override bool RefreshPorts()
		{
			bool returnValue = base.RefreshPorts();

			UpdateAnswerPorts();
			
			return returnValue;
		}

		private void UpdateAnswerPorts()
		{
			if (AttachedNode == null)
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

			for (int index = 0; index < AttachedNode?.MechanicData.markerDatas?.Count; index++)
			{
				string text = AttachedNode.MechanicData.markerDatas[index].customTextForMarker;

				if (text.IsNullOrWhitespace())
				{
					text = "Answer " + (index + 1);
				}
				allPorts[index].portName = text;
				
				// Manages the checkmarks for correct answers
				allPorts[index].Q<Image>("check-mark")?.RemoveFromHierarchy();
				if (AttachedNode.MechanicData.markerDatas[index].isCorrectMarker)
				{
					Image t = new()
					{
						name = "check-mark",
						image = Resources.Load<Texture2D>("Icon/Check"),
						style = { flexShrink = 0}
					};
					allPorts[index].Add(t);
				}
			}
			
			foreach (PortView port in allPorts)
			{
				port.UpdatePortSize();
			}
			outputContainer.MarkDirtyRepaint();
		}

		protected override void OnDoubleClick()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
				return;
#endif

			if (AttachedNode.mechanicObject == null)
				SpawnPreviewMechanic();

		}
	}
}
