using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(PopupNodeView))]
	public class PopupNodeViewValidation : AbstractNodeViewValidation<PopupNodeView>
	{
		

		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode;

			string titleKey = "Title";
			string bodyKey = "Body";
			string displayTimeKey = "TimeUntilDisappear";
			string customSpriteKey = "CustomSprite";
			string nextButtonTextKey = "NextButtonText";

			CheckSpawnPosition();

			string title = node.MechanicData.information;
			ErrorIf(string.IsNullOrWhiteSpace(title), titleKey, "Information cannot be empty or white space.");
			WarningIf("Information Text" == title, titleKey, "Default information text in use. Type to your own one.");

			string body = node.MechanicData.multiMediaInformation;
			WarningIf("MultiMedia Information" == body, bodyKey, "Default body text in use. Type to your own one.");

			if (!node.MechanicData.hasNextButton)
			{
				ErrorIf(0 > node.MechanicData.timeUntilDisappear, displayTimeKey, "Disappear time cannot less than 0. Current is " + node.MechanicData.timeUntilDisappear);
			}
			else
			{
				string nextButtonText = node.MechanicData.buttonText;
				ErrorIf(string.IsNullOrWhiteSpace(nextButtonText), nextButtonTextKey, "Next button title cannot be empty or white space. Type a new title.");
			}


			if (node.MechanicData.showCustomImage)
			{
				var sprite = node.MechanicData.imageToShow;
				ErrorIf(IsInvalidAsset(sprite), customSpriteKey, "Custom image cannot be null. Select an image.");
				ErrorIf(IsMissingAsset(sprite), customSpriteKey, "Custom image already removed. Select a new image.");
			}
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch(path)
			{
				case "SpawnPosition": return TargetNodeView.Q("scene-element-dropdown").Q<DropdownField>();
				case "Title": return TargetNodeView.Query<TextField>().AtIndex(0);
				case "Body": return TargetNodeView.Query<TextField>().AtIndex(1);
				case "TimeUntilDisappear": return TargetNodeView.Q<FloatField>();
				case "CustomSprite": return TargetNodeView.Q<ObjectField>();
				case "NextButtonText": return TargetNodeView.Query<TextField>().AtIndex(2);
				default:return null;
			}
		}

	}
}
