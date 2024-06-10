using SkillsVRNodes.Scripts.Nodes;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(AudioPlaybackNodeView))]
	public class AudioPlaybackNodeViewValidation : AbstractNodeViewValidation<AudioPlaybackNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode<AudioPlaybackNode>();

			string audioFileKey = "AudioFile";

			//Playback Does not need a spawn position
			//CheckSpawnPosition();

			if (!string.IsNullOrWhiteSpace(node.AssociatedRecorderNodesaveName))
			{
				
			}
			else if (!string.IsNullOrWhiteSpace(node.AssociatedCustomClip))
			{
				ErrorIf(IsInvalidAsset(node.mechanicData.audioClip), audioFileKey, "Audio file " + node.AssociatedCustomClip + " not exist or already removed. Select or create a new audio file.");
			}
			else
			{
				ErrorIf(true, audioFileKey, "Audio file is empty. Select or create an audio file.");
			}
		}
		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "SpawnPosition": return TargetNodeView.Q("scene-element-dropdown").Q<DropdownField>();
				case "AudioFile": return TargetNodeView.Q("audio-settings-container").Q<DropdownField>();
				default: return null;
			}
		}

		protected override void CheckSpawnPosition()
		{
			var node = TargetNodeView.AttachedNode<AudioPlaybackNode>();
			var spawnerNode = TargetNodeView.nodeTarget as ISpawnerNode;
			string preText = "SpawnPosition";
			if (spawnerNode == null)
			{
				WarningIf(null == spawnerNode, preText, "Not suitable validation. Node is not a spanwer node type.");
				return;
			}
			//PropGUID<IPropTransform> - Returns GUID
			WarningIf(node.AssociatedPlaybackAudioProp == string.Empty, preText, "Prop GUID not found. - Make sure a prop is set");
			
		}
	}
}
