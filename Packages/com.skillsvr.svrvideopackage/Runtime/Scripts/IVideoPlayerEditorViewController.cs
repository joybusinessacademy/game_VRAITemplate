using UnityEngine.Video;

namespace SkillsVR.VideoPackage
{
	public interface IVideoPlayerEditorViewController
	{
		bool forceUpdateView { get; set; }
		VideoPlayer videoPlayer { get; set; }

		void StartUpdateInEditor();
		void StopUpdateInEditor();
	}
}