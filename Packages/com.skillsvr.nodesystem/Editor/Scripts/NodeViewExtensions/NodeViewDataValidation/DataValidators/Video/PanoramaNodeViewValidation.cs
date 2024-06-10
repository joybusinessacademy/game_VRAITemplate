using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using SkillsVR.UnityExtenstion;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
#if PANORAMA_VIDEO
	[CustomDataValidation(typeof(PanoramaNodeView))]
	public class PanoramaNodeViewValidation : AbstractNodeViewValidation<PanoramaNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode;

			string videoAssetKey = "VideoClip";

			bool nullFilePath = string.IsNullOrWhiteSpace(node.videoClipLocation);

            ErrorIf(nullFilePath, videoAssetKey, "Video clip cannot be null. Select a video.");
            ErrorIf(!nullFilePath && !IsAssetExist(node.videoClipLocation), videoAssetKey, "Video clip is removed. Select a new video.");
        }

        public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch(path)
			{
				case "VideoClip": return TargetNodeView.Q<DropdownField>();
				default: return null;
			}
		}
	}
#endif
}
