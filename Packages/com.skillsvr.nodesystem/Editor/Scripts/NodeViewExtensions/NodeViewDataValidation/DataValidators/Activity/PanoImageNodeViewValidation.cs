using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(PanoImageNodeView))]
	public class PanoImageNodeViewValidation : AbstractNodeViewValidation<PanoImageNodeView>
	{
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			return null;
		}

		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode;

			CheckSpawnPosition();

			ErrorIf(node.mechanicData.image == null, "360 Image", "Custom Sprite is set to null");
			WarningIf(node.mechanicData.imageDuration <= 0, "360 Image", "Duration set to less that 0 - Will not show image");
		}
	}
}
