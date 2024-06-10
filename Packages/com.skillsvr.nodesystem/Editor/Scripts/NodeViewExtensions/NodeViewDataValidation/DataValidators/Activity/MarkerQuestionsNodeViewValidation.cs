using Props;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(MarkerQuestionsNodeView))]
	public class MarkerQuestionsNodeViewValidation : AbstractNodeViewValidation<MarkerQuestionsNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode;

			string answerListKey = "AnswerList";
			string submitButtonPosKey = "SubmitButtonPosition";

			ErrorIf(0 == node.mechanicData.markerDatas.Count, answerListKey, "Add at least 1 answer to work.");

			if (null != node.mechanicData && null != node.mechanicData.markerDatas)
			{
				int index = 0;
				foreach (var item in node.mechanicData.markerDatas)
				{
					string posKey = "AnswerPos." + (index + 1);
					string customSpriteKey = "AnswerCustomSprite." + (index + 1);

					var posGuid = node.allGameObjects.ElementAt(index);
					CheckPropGuid(posGuid, "Position", posKey);

					if (item.useCustomSprite)
						ErrorIf(IsNull(item.changeMarkerSprite), customSpriteKey, "Sprite asset cannot be null. Select a sprite.");

					index++;
				}
			}

			//Should have Submit Position if False
			if(node.mechanicData.checkStraightAway == false)
				CheckPropGuid(node.lockInButtonProp, "Position", submitButtonPosKey);
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "SubmitButtonPosition":
					var index = TargetNodeView.AttachedNode.mechanicData.markerDatas.Count();
					return TargetNodeView.Query("prop-dropdown").AtIndex(index).Q<DropdownField>();
				case "AnswerList": return TargetNodeView.Q("list-container");
				default: break;
			}

			if (path.StartsWith("Answer"))
			{
				var p = path.Split(".");
				var key = p[0];
				int index = 0;
				int.TryParse(p[1], out index);
				index--;
				switch(key)
				{
					case "AnswerPos": return TargetNodeView.Query("prop-dropdown").AtIndex(index).Q<DropdownField>();
					case "AnswerCustomSprite": return TargetNodeView.Query("prop-dropdown")
							.AtIndex(index)
							.parent
							.Q<ObjectField>();
					default:return null;
				}
			}
			return null;
		}
	}
}
