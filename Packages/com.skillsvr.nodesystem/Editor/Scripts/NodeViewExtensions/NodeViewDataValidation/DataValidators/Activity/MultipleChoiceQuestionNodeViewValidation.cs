
using System.Linq;
using UnityEngine.UIElements;


namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(MultipleChoiceQuestionNodeView))]
	public class MultipleChoiceQuestionNodeViewValidation : AbstractNodeViewValidation<MultipleChoiceQuestionNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode;


			string answerListKey = "QuestionList";
			string questionTextKey = "QuestionText";
			string minSelectCountKey = "MinSelectCount";
			string displayTimeoutKey = "DisplayTimeout";

			CheckSpawnPosition();
			string questionText = node.MechanicData.questionTitle;
			WarningIf(string.IsNullOrWhiteSpace(questionText), questionTextKey, "Question cannot be empty or white space.");
			WarningIf("New Question" == questionText, questionTextKey, "Default question text in use. Change to your own one.");

			ErrorIf(0 == node.MechanicData.questions.Count, answerListKey, "Add at least 1 question.");

			if (null != node.MechanicData && null != node.MechanicData.questions)
			{
				int index = 0;
				foreach (var item in node.MechanicData.questions)
				{
					string textKey = "AnswerText." + (index + 1);

					string answerText = item.answerText;
					WarningIf(string.IsNullOrWhiteSpace(answerText), textKey, "Text cannot be empty or white space.");
					WarningIf("Answer Text" == answerText, textKey, "Default answer text in use. Change to your own one.");
					index++;
				}
			}

			ErrorIf(0 >= node.MechanicData.minSelectionAmount, minSelectCountKey, "Min selection amount must no less than 0. Current is " + node.MechanicData.minSelectionAmount);
			int maxCount = node.MechanicData.questions.Count();
			ErrorIf(maxCount < node.MechanicData.minSelectionAmount, minSelectCountKey, 
			"Min selection amoutn cannot larger than answer count " + maxCount + ". Current is " + node.MechanicData.minSelectionAmount);

			ErrorIf(0 > node.MechanicData.delayTimeUntilTurnedOff, displayTimeoutKey, "Display timeout  must no less than 0. Current is " + node.MechanicData.delayTimeUntilTurnedOff);
			WarningIf(20 < node.MechanicData.delayTimeUntilTurnedOff, displayTimeoutKey, "Display timeout may too long. Current is " + node.MechanicData.delayTimeUntilTurnedOff);
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "SpawnPosition": return TargetNodeView.Q("scene-element-dropdown").Q<DropdownField>();
				case "QuestionList": return TargetNodeView.Q("list-container");
				case "QuestionText": return TargetNodeView.Q<TextField>();
				case "MinSelectCount": return TargetNodeView.Q("min-selection-amount-propertyfield");
				case "DisplayTimeout": return TargetNodeView.Q("delay-time-until-turned-off-floatfield");
				default: break;
			}

			if (path.StartsWith("AnswerText."))
			{
				var p = path.Split(".");
				int index = 0;
				int.TryParse(p[1], out index);
				index--;
				return TargetNodeView.Q("list-container").Query<TextField>().AtIndex(index);
			}
			return null;
		}
	}
}
