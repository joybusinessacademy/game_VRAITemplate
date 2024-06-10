using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(PanelVideoNodeView))]
	public class PanelVideoNodeViewValidation : AbstractNodeViewValidation<PanelVideoNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode;

			string videoAssetKey = "VideoClip";

			ErrorIf(InvalidPath(node.mechanicData.videoClipLocation), videoAssetKey, "Video clip cannot be null. Select a video.");
			ErrorIf(InvalidPath(node.mechanicData.videoClipLocation), videoAssetKey, "Video clip is removed. Select a new video.");
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "VideoClip": return TargetNodeView.Q<DropdownField>();
				default: return null;
			}
		}
	}
}
