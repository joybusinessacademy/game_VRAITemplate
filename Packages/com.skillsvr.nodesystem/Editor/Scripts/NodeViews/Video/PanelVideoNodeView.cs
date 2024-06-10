using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVR.Mechanic.MechanicSystems.PanelVideo;
using SkillsVR.VideoPackage;
using SkillsVR.VisualElements;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

using UnityEngine.UIElements;
using UnityEngine.Video;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
    [NodeCustomEditor(typeof(PanelVideoNode))]
    public class PanelVideoNodeView : SpawnerNodeView<SpawnerPanelVideo, IPanelVideoSystem, PanelVideoData>
    {
        public PanelVideoNode AttachedNode => nodeTarget as PanelVideoNode;
        
        //Clip Assinging
        //private DropdownField videoObjectField;
        private VideoClip clipAssignedToNode;

		//private Slider videoTimeSlider;
		//private IconButton videoIconButton;

		public override VisualElement GetNodeVisualElement()
		{
			return new TextLabel("Video file", !AttachedNode.MechanicData.videoClipLocation.IsNullOrWhitespace() ? Path.GetFileName(AttachedNode.MechanicData.videoClipLocation) : null);
		}

        private float ClampTimeWithinLength(float value, float length)
        {
            float max = length > 0.0f ? length : 3600;
            return Mathf.Clamp(value, 0.0f, max);
        }

        public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
            //Location Panel
            visualElement.Add(CreateTransformDropdown<IPropPanel>(AttachedNode));
            visualElement.Add(new Divider());

            clipAssignedToNode = AssetDatabase.LoadAssetAtPath<VideoClip>(AttachedNode.MechanicData.videoClipLocation);
            visualElement.Add(new AssetDropdown<VideoClip>(OnChange, AssetDatabase.LoadAssetAtPath<VideoClip>(AttachedNode.MechanicData.videoClipLocation), "Video Clip"));

            //SetVideoObjectDropdown();
            visualElement.Add(GenerateVideoNodeUI());
            
            // BuildVideoTimeSlider
            Slider videoTimeSlider = new Slider();

            videoTimeSlider.lowValue = 0f;
            if(clipAssignedToNode != null)
                videoTimeSlider.highValue = ClampTimeWithinLength((float)clipAssignedToNode.length, (float)clipAssignedToNode.length);

            IconButton videoIconButton = new(() => PlayVideoInEditor(videoTimeSlider), "play", 16)
            {
	            tooltip = "Play"
            };
            videoTimeSlider.Add(videoIconButton);

            visualElement.Add(new Divider(5));

            visualElement.Add(videoTimeSlider);
            return visualElement;
		}

        private VisualElement GenerateVideoNodeUI()
        {
	        VisualElement visualElement = new VisualElement();
            if (clipAssignedToNode == null)
            {
                return visualElement;
            }

            visualElement.Add(VideoStartTimeUI());
            visualElement.Add(VideoEndTimeUI());
            visualElement.Add(SetLoopToggleUI());
            visualElement.Add(VolumeUI());
            // Set Skip Video UI
            var skipVideoToggle = new Toggle()
            {
	            label = "Show Skip Video Button: ",
	            value = AttachedNode.MechanicData.showSkipButton,
            };
            skipVideoToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
	            AttachedNode.MechanicData.showSkipButton = evt.newValue;
            });

            visualElement.Add(skipVideoToggle);

            return visualElement;
        }

        private void OnChange(VideoClip item)
        {
            if (item == null)
            {
	            AttachedNode.MechanicData.videoClipGUID = "";
	            AttachedNode.MechanicData.videoClipLocation = "";
                RefreshNode();
                return;
            }

            clipAssignedToNode = item;
            string assetPath = AssetDatabase.GetAssetPath(clipAssignedToNode);

            string objectGUID = AssetDatabase.AssetPathToGUID(assetPath);

            AttachedNode.MechanicData.videoClipGUID = objectGUID;
            AttachedNode.MechanicData.videoClipLocation = assetPath; 

            if (clipAssignedToNode != null)
            {
                AttachedNode.MechanicData.videoWidth = clipAssignedToNode.width;
                AttachedNode.MechanicData.videoHeight = clipAssignedToNode.height;
            }

            GenerateVideoNodeUI();

            RefreshNode();
        }

        private void OnVideoObjectFieldChanged(ChangeEvent<string> evt)
        {
            foreach (VideoClip item in GetAllVideoClips().Where(item => evt.newValue.Contains(item.name)))
            {
                clipAssignedToNode = item;
            }
    
            if (clipAssignedToNode != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(clipAssignedToNode);
    
                if (!string.IsNullOrEmpty(assetPath))
                {
                    // Get the GUID of the target object based on its asset path
                    string objectGUID = AssetDatabase.AssetPathToGUID(assetPath);
                    AttachedNode.MechanicData.videoClipGUID = objectGUID;
					AttachedNode.MechanicData.videoClipLocation = assetPath; 
                }
            }
            else if (evt.newValue.Equals("External/Add Video Clip"))
            {
                string videoPath = SkillsVRAssetImporter.ImportAssetType<VideoClip>();
    
                if (!string.IsNullOrEmpty(videoPath))
                {
                    clipAssignedToNode = AssetDatabase.LoadAssetAtPath<VideoClip>(videoPath);
                    AttachedNode.MechanicData.videoWidth = clipAssignedToNode.width;
                    AttachedNode.MechanicData.videoHeight = clipAssignedToNode.height;
    
                    AttachedNode.MechanicData.videoClipGUID = AssetDatabase.AssetPathToGUID(videoPath);
					AttachedNode.MechanicData.videoClipLocation = videoPath; 
                }
            }
            else if (evt.newValue.Equals("External/Batch Import Video Clips"))
            {
                SkillsVRAssetImporter.BatchImportAssetType<VideoClip>();
            }
            else
            {
                controlsContainer.Clear();
    
                AttachedNode.MechanicData.videoClipGUID = String.Empty;
				AttachedNode.MechanicData.videoClipLocation = String.Empty; 
            }
    
            if (clipAssignedToNode != null)
            {
                VideoEndTimeUI();
            }
    
            MarkDirtyRepaint();
            BaseGraphWindow.Instance.graphView.Refresh(); 
        }

        private List<VideoClip> GetAllVideoClips()
        {
            string[] guids = AssetDatabase.FindAssets("t:VideoClip");
        
            List<VideoClip> videoClips = new List<VideoClip>();
        
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                VideoClip videoClip = AssetDatabase.LoadAssetAtPath<VideoClip>(assetPath);
        
                if (videoClip != null)
                    videoClips.Add(videoClip);
            }
        
            return videoClips;
        }

        private VisualElement VideoStartTimeUI()
		{
            var videoStartTime = new FloatField("Video Start Time: ")
            {
	            value = AttachedNode.mechanicData.startTimeVideo
            };

            videoStartTime.RegisterValueChangedCallback(e =>
            {
                float maxTime = 0;

                if (clipAssignedToNode != null)
                {
                    maxTime = (float)clipAssignedToNode.length;
                }

                videoStartTime.value = Mathf.Clamp(e.newValue, 0, maxTime);
                AttachedNode.MechanicData.startTimeVideo = videoStartTime.value;
            });

            return videoStartTime;
		}

        private FloatField VideoEndTimeUI()
        {
            var videoEndTime = new FloatField("Video End Time: ");

            if (AttachedNode.MechanicData.endTimeVideo == 0)
            {
                AttachedNode.MechanicData.endTimeVideo = (float)clipAssignedToNode.length;
            }

            videoEndTime.value = AttachedNode.MechanicData.endTimeVideo;

            videoEndTime.RegisterValueChangedCallback(e =>
            {
                float maxTime = 0;

                if (clipAssignedToNode != null)
                    maxTime = (float)clipAssignedToNode.length;

                videoEndTime.value = Mathf.Clamp(e.newValue, 0, maxTime);
                AttachedNode.MechanicData.endTimeVideo = videoEndTime.value;
            });
            
            return videoEndTime;
        }

        public VisualElement SetLoopToggleUI()
        {
            Toggle loopVideoToggle = new()
            {
                label = "Loop Entire Video: ",
                value = AttachedNode.MechanicData.loopVideo,
            };
            loopVideoToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                AttachedNode.MechanicData.loopVideo = evt.newValue;
            });

            return loopVideoToggle;
        }

        private VisualElement VolumeUI()
        {
	        SliderFloat volumeSlider = new SliderFloat(0, 1, AttachedNode.MechanicData.volumeVideo, "Volume: ", (value) => AttachedNode.MechanicData.volumeVideo = value);

            return volumeSlider;
        }

        private AdvancedVideoPlayerController advancedVideoPlayerController;

		private void PlayVideoInEditor(Slider videoTimeSlider)
		{
			IconButton videoIconButton = videoTimeSlider.Q<IconButton>();
            //Currently have a preview
            if(AttachedNode.mechanicObject != null)
            {
                if (advancedVideoPlayerController == null)
                {
	                advancedVideoPlayerController = AttachedNode.mechanicObject.GetComponentInChildren<AdvancedVideoPlayerController>();
                }
                else if (advancedVideoPlayerController.IsPlaying)
                {
                    advancedVideoPlayerController.Pause();
                    SetupVideoButtonIconPlay(videoIconButton);
                    return;
				}

                if (advancedVideoPlayerController != null)
                {
                    advancedVideoPlayerController.CurrentRenderTexture = advancedVideoPlayerController.GetComponent<VideoPlayer>().targetTexture;
                    
                    CanvasGroup cg = AttachedNode.mechanicObject.GetComponent<CanvasGroup>();
                    if (cg)
                    {
	                    cg.alpha = 1;
                    }

                    advancedVideoPlayerController.Seek(videoTimeSlider.value, () => PlayVideo(videoTimeSlider, videoIconButton));

				}
            }
		}

        private void PlayVideo(Slider videoTimeSlider, IconButton videoIconButton)
        {
			advancedVideoPlayerController.Play();
			EditorCoroutineUtility.StartCoroutineOwnerless(OnPlayVideo(videoTimeSlider));
			SetupVideoButtonIconPause(videoIconButton);
		}

		private void SetupVideoButtonIconPause(IconButton videoIconButton)
		{
			videoIconButton.iconImage.image = GetIcon("pause");
		}

        private void SetupVideoButtonIconPlay(IconButton videoIconButton)
        {
            videoIconButton.iconImage.image = GetIcon("play");
        }

		private Texture2D GetIcon(string name)
		{
			return string.IsNullOrWhiteSpace(name) ? null : Resources.Load<Texture2D>("Icon/" + name);
		}

		private IEnumerator OnPlayVideo(Slider videoTimeSlider)
		{
			int delayStopFrameCountAfterNotPlaying = 10;
			int delayCount = delayStopFrameCountAfterNotPlaying;
			while (advancedVideoPlayerController.IsPlaying || delayCount > 0)
            {
				videoTimeSlider.SetValueWithoutNotify(advancedVideoPlayerController.CurrentPlayTime);

				if (advancedVideoPlayerController.IsPlaying)
				{
					delayCount = 10;
				}
				else
				{
					--delayCount;
				}
				yield return null;
			}
		}
	}
}