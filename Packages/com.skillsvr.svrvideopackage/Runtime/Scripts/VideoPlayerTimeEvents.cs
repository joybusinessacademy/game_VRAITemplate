using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.VideoPackage
{
	[ExecuteInEditMode]
	public class VideoPlayerTimeEvents : MonoBehaviour
	{
		public IAdvancedVideoPlayer targetVideoPlayer { get; private set; }

		private void OnEnable()
		{
			if (null == targetVideoPlayer)
			{
				targetVideoPlayer = GetComponent<IAdvancedVideoPlayer>();
			}
		}

		private void OnDisable()
		{
			targetVideoPlayer = null;
		}
	}
}

