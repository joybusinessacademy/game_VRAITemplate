using GraphProcessor;
using Props.PropInterfaces;
using Props;
using Scripts.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;
using DialogExporter;
using System.Collections.Generic;
using System;
using SkillsVRNodes.Managers;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(SurveyNode))]
	public class SurveyNodeView : SpawnerNodeView<SpawnerSurvey, ISurveySystem, SurveyData>
	{
		public SurveyNode AttachedNode => nodeTarget as SurveyNode;

		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();

			//Survey Type
			EnumField surveyTypeField = new("Scale Type: ", AttachedNode.MechanicData.surveyScaleType);

			surveyTypeField.RegisterValueChangedCallback((evt) =>
			{
				AttachedNode.MechanicData.surveyScaleType = (SurveyScaleType)evt.newValue;
				RefreshNode();
			});

			visualElement.Add(surveyTypeField);

			//Optional Text
			visualElement.Add(AttachedNode.MechanicData.instructionText.LocField("Instruction (Optional)"));

			//Survey Likert
			if (AttachedNode.MechanicData.surveyScaleType == SurveyScaleType.LIKERT)
			{
				ListDropdown<SurveyLikertData> likertChoices = new("Likert Labels", AttachedNode.MechanicData.likertLabels, SurveyLikertBox, 10);
				visualElement.Add(likertChoices);
			}
			else
			{
				visualElement.Add(AttachedNode.MechanicData.prefixLabel.LocField("Prefix Label"));
				visualElement.Add(AttachedNode.MechanicData.postFixLabel.LocField("Postfix Label"));
			}
			//Survey Prompts
			ListDropdown<SurveyStatementPromptData> statementChoices = new("Statement Prompts", AttachedNode.MechanicData.statementPrompts, SurveyStatementBox, 10);
			visualElement.Add(statementChoices);


			//Button Done Text
			visualElement.Add(AttachedNode.MechanicData.finishedButtonLabel.LocField("Button Label"));

			return visualElement;
		}

		public override VisualElement GetNodeVisualElement()
		{
			return null;
		}

		private VisualElement SurveyLikertBox(SurveyLikertData surveyLikertData)
		{
			VisualElement questionContainer = new();

			
			int index = AttachedNode.MechanicData.likertLabels.IndexOf(surveyLikertData);
			questionContainer.Add(surveyLikertData.likertString.LocField("Label" + (index + 1)));

			var space = new VisualElement();
			space.style.flexGrow = 1;
			questionContainer.Add(space);


			IconButton delete = new IconButton("Close")
			{
				tooltip = "Delete",
			};

			delete.clicked += () =>
			{
				RefreshNode();
			};

			delete.clicked += () =>
			{
				bool requireRefresh = this.ResetEdgeFieldNameBeforeRemoveDynamicPortItem(index);
				AttachedNode.MechanicData.likertLabels.RemoveAt(index);

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

		private VisualElement SurveyStatementBox(SurveyStatementPromptData surveyStatementData)
		{
			VisualElement questionContainer = new();

			int index = AttachedNode.MechanicData.statementPrompts.IndexOf(surveyStatementData);
			questionContainer.Add(surveyStatementData.statementString.LocField("Statement " + (index + 1)));
			questionContainer.Add(surveyStatementData.learnMoreString.LocField("Learn more text"));
			questionContainer.Add(new AudioSelector(evt => surveyStatementData.learnMoreAudio = evt, surveyStatementData.learnMoreAudio));

			var space = new VisualElement();
			space.style.flexGrow = 1;
			questionContainer.Add(space);

			IconButton delete = new IconButton("Close")
			{
				tooltip = "Delete",
			};

			delete.clicked += () =>
			{
				RefreshNode();
			};

			delete.clicked += () =>
			{
				bool requireRefresh = this.ResetEdgeFieldNameBeforeRemoveDynamicPortItem(index);
				AttachedNode.MechanicData.statementPrompts.RemoveAt(index);

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

	}
}
