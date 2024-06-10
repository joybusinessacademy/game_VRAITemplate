using System;

namespace SkillsVR.Mechanic.MechanicSystems.PanelVideo
{
	[Serializable]
	public class PanelVideoData
	{
		public bool usingExternalPlayer = false;

		public string videoClipGUID = string.Empty;
		public string videoClipLocation = string.Empty;

		public bool loopVideo = false;
		public float startTimeVideo = 0;
		public float endTimeVideo = 0;
		public float volumeVideo = 1;

		public float videoWidth;
		public float videoHeight;

		public bool showSkipButton = false;
	}
}
