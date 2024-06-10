using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(MultipleChoiceQuestionNode))]
	public class MultipleChoiceQuestionNodeView : ScriptableSpawnerNodeView<SpawnerMultipleChoiceQuestion,
		IMultipleChoiceQuestionSystem, MultipleChoiceQuestionScriptable>
	{
		public MultipleChoiceQuestionNode AttachedNode => nodeTarget as MultipleChoiceQuestionNode;

		public override void Enable()
		{
			base.Enable();

			EditorCoroutineUtility.StartCoroutineOwnerless(DelayedUpdatePorts());
		}

		public override VisualElement GetNodeVisualElement()
		{
			return null;
		}

		protected override void OnRefresh()
		{
			UpdateAnswerPorts();
			base.OnRefresh();
		}

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement inspectorVisualElement = new();

			MultipleChoiceQuestionNode spawnerNode = AttachedNode;
			inspectorVisualElement.RegisterCallback<ChangeEvent<string>>(_ => RefreshPorts());
			inspectorVisualElement.Clear();
			
			inspectorVisualElement.Add(CreateTransformDropdown<IPropPanel>(spawnerNode));
			
			inspectorVisualElement.Add(new Divider());

			inspectorVisualElement.Add(spawnerNode.MechanicData.questionTitle.LocField("Question:"));

			ListDropdown<MultipleChoiceAnswer> allAnswersContainer = new("Answers", spawnerNode.MechanicData.questions, QuestionBox, 10);
			allAnswersContainer.onRefresh += () => RefreshPorts();
			inspectorVisualElement.Add(allAnswersContainer);

			inspectorVisualElement.Add(spawnerNode.MechanicData.CustomField(nameof(spawnerNode.MechanicData.multipleChoiceFeedback)));

			// converted to automated
			inspectorVisualElement.Add(spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.allowMultipleSelection)));
			inspectorVisualElement.Add(spawnerNode.MechanicData.CustomIntField(nameof(spawnerNode.MechanicData.minSelectionAmount)));

			inspectorVisualElement.Add(AttachedNode.MechanicData.CustomFloatField(nameof(AttachedNode.MechanicData.delayTimeUntilTurnedOff), "Choice display timeout (seconds)"));
			inspectorVisualElement.Add(spawnerNode.MechanicData.CustomToggle(nameof(spawnerNode.MechanicData.shuffleAnswers)));
			
			return inspectorVisualElement;
		}

		private void UpdateMinimumSelectionAmount()
		{
			if (null == AttachedNode || null == AttachedNode.MechanicData)
			{
				return;
			}
			if (AttachedNode.MechanicData.questions == null)
				return;

			int correctCount = AttachedNode.MechanicData.questions.FindAll(t => t.isCorrectAnswer).Count;
			if (GetPortViewsFromFieldName("Correct") != null)
			{
				GetPortViewsFromFieldName("Correct").ForEach(k =>
				{
					k.SetEnabled(correctCount > 0);
					k.GetEdges().ForEach(edge => edge.style.display = correctCount == 0 ? DisplayStyle.None : DisplayStyle.Flex);
				});
			}
			if (GetPortViewsFromFieldName("Incorrect") != null)
			{
				GetPortViewsFromFieldName("Incorrect").ForEach(k =>
				{
					k.SetEnabled(correctCount > 0);
					k.GetEdges().ForEach(edge => edge.style.display = correctCount == 0 ? DisplayStyle.None : DisplayStyle.Flex);
				});
			}

			AttachedNode.MechanicData.minSelectionAmount = AttachedNode.MechanicData.allowMultipleSelection ? AttachedNode.MechanicData.minSelectionAmount : UnityEngine.Mathf.Max(1, correctCount);
		}

		
		IEnumerator DelayedUpdatePorts()
		{
			yield return new EditorWaitForSeconds(0.05f);
			UpdateMinimumSelectionAmount();
		}
		
		private VisualElement QuestionBox(MultipleChoiceAnswer question)
		{
			int questionIndex = AttachedNode.MechanicData.questions.IndexOf(question);
			
			VisualElement questionContainer = new();
			
			Toggle toggle = new()
			{
				label = "Correct: ",
				value = question.isCorrectAnswer,
				tooltip = "Is correct answer"
			};
			toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
			{
				// i dont know why it showed error here
				AttachedNode.MechanicData.questions.Find(t => t == question).isCorrectAnswer = evt.newValue;
				UpdateMinimumSelectionAmount();
				RefreshNode();
			});
			questionContainer.Add(toggle);
			questionContainer.Add(question.answerText.LocField("Answer"));

			IconButton delete = new IconButton("Close",16, 1, true)
			{
				tooltip = "Delete",
			};

			delete.clicked += () =>
			{
				bool requireRefresh = this.ResetEdgeFieldNameBeforeRemoveDynamicPortItem(questionIndex);
				DisconnectPort(AttachedNode.GetOutputPortNameByIndex(questionIndex));
				AttachedNode.MechanicData.questions.Remove(question);
				UpdateMinimumSelectionAmount();
				
				RefreshNode();
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
			if (AttachedNode == null)
				return;

			UpdateMinimumSelectionAmount();

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

			for (int index = 0; index < AttachedNode?.MechanicData.questions?.Count; index++)
			{
				string portText = AttachedNode.MechanicData.questions[index].answerText;

				if (string.IsNullOrWhiteSpace(portText))
					portText = "Answer is Empty";	

				allPorts[index].portName = portText;
				
				// Manages the checkmarks for correct answers
				allPorts[index].Q<Image>("check-mark")?.RemoveFromHierarchy();
				if (AttachedNode.MechanicData.questions[index].isCorrectAnswer)
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
	}
}
