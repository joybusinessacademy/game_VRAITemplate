using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(PanelImageNodeView))]
	public class PanelImageNodeViewValidation : AbstractNodeViewValidation<PanelImageNodeView>
	{
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			return null;
		}

		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode;

			CheckSpawnPosition();

			ErrorIf(node.mechanicData.image == null, "Panel Image", "Custom Sprite is set to null");
			WarningIf(node.mechanicData.imageDuration <= 0, "Panel Image", "Duration set to less that 0 - Will not show image");
			WarningIf(node.mechanicData.nextButtonText == string.Empty, "Panel Image", "Next Button text should have a field - EG: Next");
		}
	}
}
