using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(AudioRecorderNodeView))]
	public class AudioRecorderNodeViewValidation : AbstractNodeViewValidation<AudioRecorderNodeView>
	{
		public override void OnValidate()
		{

			string clipSaveNameKey = "ClipSaveName";
			string startRecordTextKey = "StartRecordText";
			string stopRecordTextKey = "StopRecordText";

			string maxDurationKey = "MaxDuration";


			var node = TargetNodeView.AttachedNode;
			CheckSpawnPosition();
			ErrorIf(string.IsNullOrWhiteSpace(node.saveName), clipSaveNameKey, "Save name cannot be empty or white space. Type a valid name to save clip.");

			WarningIf(string.IsNullOrWhiteSpace(node.mechanicData.startRecordingInformationText), startRecordTextKey, "Text is empty.");
			WarningIf(string.IsNullOrWhiteSpace(node.mechanicData.stopRecordingInformationText), stopRecordTextKey, "Text is empty.");

			if (node.enableMaxRecordDuration)
			{
				var duration = node.mechanicData.clipDuration;
				ErrorIf(duration <= 0.0f, maxDurationKey, "Max duration must no less than 0. Current is " + duration);

				if (duration > 0)
				{
					WarningIf(duration < 5.0f, maxDurationKey, "Max duration may too short for recording. Current is " + duration);
				}
			}
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "SpawnPosition": return TargetNodeView.Q("scene-element-dropdown").Q<DropdownField>();
				case "ClipSaveName": return TargetNodeView.Query<TextField>().First();
				case "StartRecordText": return TargetNodeView.Query<TextField>().AtIndex(1);
				case "StopRecordText": return TargetNodeView.Query<TextField>().AtIndex(2);
				case "MaxDuration": return TargetNodeView.Q("clip-duration-floatfield");
				default: return null;
			}
		}
	}
}
