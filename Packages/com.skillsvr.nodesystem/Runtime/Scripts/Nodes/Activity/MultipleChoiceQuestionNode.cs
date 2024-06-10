using GraphProcessor;
using SkillsVR.Mechanic.Core;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Learning/Multi Choice Question", typeof(SceneGraph)), NodeMenuItem("Learning/Multi Choice Question", typeof(SubGraph))]
	public class MultipleChoiceQuestionNode : ScriptableSpawnerNode<SpawnerMultipleChoiceQuestion, IMultipleChoiceQuestionSystem, MultipleChoiceQuestionScriptable>, IDynamicOutputPortCollection
	{
		public override string name => "Multi Choice Question";
		public override string icon => "Quiz";
		public override Color color => NodeColours.Learning;
		public override string layoutStyle => "MultipleChoiceQuestionNode";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#multi-choice-question-node";
		public override int Width => MEDIUM_WIDTH;

		[SerializeReference] public MultipleChoiceQuestionScriptable data;

		[Output(name = "Correct")] public ConditionalLink Correct = new();

		[Output(name = "Incorrect")] public ConditionalLink Incorrect = new();

		private bool wasCorrect;

		private Dictionary<int, bool> selectionResults;
		protected override void MechanicListener(IMechanicSystemEvent mechanicSystemEvent)
		{
			switch (mechanicSystemEvent.eventKey)
			{
				case MechSysEvent.AfterFullStop:
					RunLink(wasCorrect ? nameof(Correct) : nameof(Incorrect), false);
					RunSelectedOutput();
					CompleteNode();
					break;
				case MCQEvent.InCorrectButton:
					wasCorrect = false;
					break;
				case MCQEvent.CorrectButton:
					wasCorrect = true;
					break;
				case MCQEvent.MultipleSelectionCorrect:
					wasCorrect = true;
					break;
				case MCQEvent.MultipleSelectionIncorrect:
					wasCorrect = false;
					break;
				case MCQEvent.OnChoiceSelected:
					//int index = mechanicSystemEvent.GetData<int>();
					break;
				case MCQEvent.OnChoiceUnselected:
					//int index = mechanicSystemEvent.GetData<int>();
					break;
				case MCQEvent.OnSelectionResult:
					selectionResults = mechanicSystemEvent.GetData<Dictionary<int, bool>>();
					break;
			}
			
			base.MechanicListener(mechanicSystemEvent);
		}

		public void RunSelectedOutput()
		{
			if (null == selectionResults)
			{
				return;
			}
			foreach(var index in selectionResults.Keys)
			{
				RunLink(GetOutputPortNameByIndex(index), false);
			}
		}
		
		[Output(name = "Answer 1:")]
		public ConditionalLink Output1 = new();
		[Output(name = "Answer 2:")]
		public ConditionalLink Output2 = new();
		[Output(name = "Answer 3:")]
		public ConditionalLink Output3 = new();
		[Output(name = "Answer 4:")]
		public ConditionalLink Output4 = new();
		[Output(name = "Answer 5:")]
		public ConditionalLink Output5 = new();
		[Output(name = "Answer 6:")]
		public ConditionalLink Output6 = new();
		[Output(name = "Answer 7:")]
		public ConditionalLink Output7 = new();
		[Output(name = "Answer 8:")]
		public ConditionalLink Output8 = new ();
		[Output(name = "Answer 9:")]
		public ConditionalLink Output9 = new();
		[Output(name = "Answer 10:")]
		public ConditionalLink Output10 = new();

		public string GetOutputPortNameByIndex(int index)
		{
			return "Output" + (index + 1);
		}
	}
}
