using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor.UIElements;
using GraphProcessor;
using SkillsVR;
using SkillsVR.UnityExtenstion;
using SkillsVR.VideoPackage;
using SkillsVR.VisualElements;
using SkillsVRNodes.Editor.NodeViews;
using VisualElements;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts;
using SkillsVRNodes.Scripts.Nodes;
using Unity.EditorCoroutines.Editor;
using UnityEngine.SceneManagement;

namespace Scripts.VisualElements
{
	public class VideoVisualElement : VisualElement
	{
		private readonly Action refresh;
		public VideoVisualElement(PanoramaVideoNode attachedNode, BaseGraphView owner, Action refresh)
		{
			AttachedNode = attachedNode;
			this.owner = owner;
			this.refresh = refresh;
			enableOnPlayMode = EditorApplication.isPlayingOrWillChangePlaymode;

			if (AttachedNode.ProcessDataUpdate(GetVideoSystem().DataVersion))
			{
				owner?.SaveGraphToDisk();
			}

			CleanCurrentVideo();

            Add(GenerateVideoNodeUI());
			UpdateViewDataFromNode();
			SetHideableContainerDisplay(!string.IsNullOrWhiteSpace(AttachedNode.videoClipLocation));
			
			RegisterCallback<DetachFromPanelEvent>(_ => Deinit());

			if (!string.IsNullOrWhiteSpace(AttachedNode.videoClipLocation))
			{
				panoramaVideoSystem.ApplySkyBoxMaterial();
                this.schedule.Execute(PreviewNodeVideoAtStartTime).ExecuteLater(100);
                this.schedule.Execute(EditorUpdate).ExecuteLater(100);
            }
			else
			{
				panoramaVideoSystem.RestorePrevSkyBoxMaterial();
			}

            CheckMissingVideoAsset();
			PollForVideoWidthAndHeight();
		}

        private BaseGraphView owner;
		const string DEFAULT_VIDEO_DIR = "Assets/Videos/";
		const string ADD_EXTERNAL_VIDEO_TITLE = "External/Add Video Clip";
		public readonly PanoramaVideoNode AttachedNode;

		private Slider slider;

		private DropdownField videoObjectField;

		private PanoramaVideoSystem panoramaVideoSystem;

		private IconButton videoIconButton;
		private IconButton playSectionButton;

		private EditorCoroutine videoEditorCoroutine;

		private Image videoDisplayImage;
		
		private FloatField loopSectionDuration;

		private Label videoTimeLabel;
		private FloatField volumeField;
		private FloatField volumeBoosterField;
		private Slider volumeBoosterSlider;
		private SliderInt virtualNorthSlider;
		private FloatField videoCutoffTime;
		private FloatField videoStartTime;

		private Texture2D capturedTexture;

		private List<VisualElement> videoDataDropDownElements = new List<VisualElement>();
		private List<VisualElement> volumeDropdownElements = new List<VisualElement>();
		
		private DropdownField audioDropdown;
		
		private VisualElement hideableVideoContainer;

        private string originVideoNameDropdownValue;

		private bool isLoadingVideo;

        private float VideoClipLength
		{
			get => AttachedNode.videoClipLengthCache > 0.0f ? AttachedNode.videoClipLengthCache : 3600.0f;
			set
			{
				float len = Mathf.Max(0.0f, value);
				if (len != AttachedNode.videoClipLengthCache)
				{
					AttachedNode.videoClipLengthCache = len;
					OnClipLengthChanged();
				}
			}
		}
		private bool enableOnPlayMode = false;
		
		private VisualElement GenerateVideoNodeUI()
		{
			var visualElement = new VisualElement();
			visualElement.Add(BuildVideoAssetDropdown());

			BuildPlayModeField();
			BuildStartStopTimeControls();


			//BuildLoopVideoToggle();
			//BuildLoopSectionToggle();
			BuildLoopSectionDurationField();

			BuildAudioDropdown();
			BuildVirtualNorthSlider();

			BuildVolumeVisual();
			BuildVolumeBoosterVisual();
			

			visualElement.Add(new Divider());

            hideableVideoContainer ??= new VisualElement();

            hideableVideoContainer.Add(BuildVideoTimeLabel());

            hideableVideoContainer.Add(new Divider());

            hideableVideoContainer.Add(BuildVideoTimeSlider());

			visualElement.Add(BuildFoldoutGroups());
			BuildVideoImage();

			return visualElement;
		}

		public void UpdateViewDataFromNode()
		{
			UpdateVideoAssetDropdownFromNodeData();
			UpdateStartStopTimeFromNode();
			UpdateLoopValuesFromNode();

			RefreshAudioAssetDropdownChoices();
			UpdateAudioAssetDropdownFromNode();

			UpdateVirtualNorthFromNode();

			UpdateVideoTimeSliderFromNode();
			UpdateVideoTimeLabel();
			UpdateVideoSectionTimeFromNode();
		}

        private void CheckMissingVideoAsset()
        {
            if (string.IsNullOrWhiteSpace(AttachedNode.videoClipLocation))
            {
                return;
            }
            string fullPath = Application.dataPath.Replace("Assets", "") + AttachedNode.videoClipLocation;
            if (File.Exists(fullPath))
            {
				return;
            }
            SetHideableContainerDisplay(false);

			string missingTxt = "[!!Missing] ";
			if (null != videoObjectField.value && !videoObjectField.value.StartsWith(missingTxt))
			{
                videoObjectField.SetValueWithoutNotify("[!!Missing] " + videoObjectField.value);
            }
        }

        private void AutoSetupInitValues()
        {
			AttachedNode.videoStartTime = 0.0f;
            AttachedNode.videoCutoffTime = VideoClipLength;
        }

        private void Deinit()
		{
			CleanCurrentVideo();
			if (videoEditorCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(videoEditorCoroutine);
				videoEditorCoroutine = null;
			}
		}

		private void UpdatePreviewTexture()
		{
			UpdateSpriteWithRT();

			if (videoDisplayImage != null)
				videoDisplayImage.image = capturedTexture;
		}

		private void GetVideoSceneCameraImage()
		{
			if (videoDisplayImage != null)
			{
				Camera editorSceneCamera = GetVideoSystem().GetComponentInChildren<Camera>();
				if (editorSceneCamera != null)
					videoDisplayImage.image = editorSceneCamera.targetTexture;
			}
		}

		private void EditorUpdate()
		{
            this.schedule.Execute(EditorUpdate).ExecuteLater(100);
            if (isLoadingVideo)
			{
				return;
            }

            AutoUpdatePreviewInEditMode();
            CheckMissingVideoAsset();
        }
		private void AutoUpdatePreviewInEditMode()
		{
			if (!Application.isPlaying
				&& null != panoramaVideoSystem
				&& !panoramaVideoSystem.IsPlaying)
			{
                UpdatePreviewTexture();
            }
        }

        private void UpdateSpriteWithRT()
        {
            if (SceneView.lastActiveSceneView == null || SceneView.lastActiveSceneView.camera == null ||
                SceneView.lastActiveSceneView.camera.activeTexture == null)
                return;

            int width = SceneView.lastActiveSceneView.camera.activeTexture.width;
            int height = SceneView.lastActiveSceneView.camera.activeTexture.height;

            var oriTexture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);

            SceneView.lastActiveSceneView.camera.Render();

            RenderTexture.active = SceneView.lastActiveSceneView.camera.activeTexture;// videoPlayerItemSO.videoRenderTexture;
            oriTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            oriTexture.Apply();
            RenderTexture.active = null;

            capturedTexture = PickAndScaleTexture(oriTexture, 350, 200); ;
        }

        // Method to pick and scale a portion of a Texture2D while maintaining aspect ratio
        public static Texture2D PickAndScaleTexture(Texture2D sourceTexture2D, int targetWidth, int targetHeight)
        {
            // Create a new texture with the desired dimensions
            Texture2D scaledTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32, false, true);

            // Calculate the aspect ratio of the source texture
            float aspectRatio = (float)sourceTexture2D.width / sourceTexture2D.height;

            // Calculate the target aspect ratio
            float targetAspectRatio = (float)targetWidth / targetHeight;

            // Calculate the scale factors for width and height
            float widthScale = 1f;
            float heightScale = 1f;

            if (aspectRatio > targetAspectRatio)
            {
                widthScale = targetAspectRatio / aspectRatio;
            }
            else
            {
                heightScale = aspectRatio / targetAspectRatio;
            }

            // Calculate the offset to center the output area
            float xOffset = (1f - widthScale) / 2f;
            float yOffset = (1f - heightScale) / 2f;
            // Calculate the source rect
            Rect sourceRect = new Rect(
                xOffset * sourceTexture2D.width,
                yOffset * sourceTexture2D.height,
                widthScale * sourceTexture2D.width,
                heightScale * sourceTexture2D.height
            );

            int x = Mathf.FloorToInt(sourceRect.x);
            int y = Mathf.FloorToInt(sourceRect.y);
            int w = Mathf.FloorToInt(sourceRect.width);
            int h = Mathf.FloorToInt(sourceRect.height);
            // Get the raw texture data
            Color[] sourceColors = sourceTexture2D.GetPixels(x, y, w, h);

            sourceColors = ResizeTextureData(sourceColors, w, h, targetWidth, targetHeight);
            // Set the pixels of the scaled texture
            scaledTexture.SetPixels(sourceColors);

            // Apply changes to the scaled texture
            scaledTexture.Apply();

            return scaledTexture;
        }

        private static Color[] ResizeTextureData(Color[] sourceColors, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            Color[] resizedColors = new Color[targetWidth * targetHeight];

            // Loop through each pixel of the resized texture
            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    // Calculate the corresponding index in the source texture data
                    int sourceX = Mathf.FloorToInt((float)x * sourceWidth / targetWidth);
                    int sourceY = Mathf.FloorToInt((float)y * sourceHeight / targetHeight);
                    int sourceIndex = sourceY * sourceWidth + sourceX;

                    // Set the color from the source texture data to the resized texture data
                    resizedColors[y * targetWidth + x] = sourceColors[sourceIndex];
                }
            }

            return resizedColors;
        }

        //private void AddVideoPlayer()
        //{
        //	GameObject vo = GetVideoEditorObject();
        //	if (vo != null)
        //	{
        //		videoPlayer = GetVideoSystem().
        //		GetVideoSystem().videoPlayer = videoPlayer;
        //	}
        //}

        private static PanoramaVideoSystem sharedVideoSystem;
		private PanoramaVideoSystem GetVideoSystem()
		{
			if (panoramaVideoSystem != null)
			{
				return panoramaVideoSystem;
			}

			if (null == sharedVideoSystem)
			{
				sharedVideoSystem = GameObject.FindObjectOfType<PanoramaVideoSystem>();
			}
			if (null == sharedVideoSystem)
			{
				sharedVideoSystem = TryCreateVideoSystemInScene(this.AttachedNode.Graph.GetDefaultGraphScenePath());
			}

			panoramaVideoSystem = sharedVideoSystem;
			panoramaVideoSystem.enabled = true;
			return panoramaVideoSystem;
		}

		private PanoramaVideoSystem TryCreateVideoSystemInScene(string scenePath)
		{
			scenePath = null == scenePath ? "" : scenePath;
			var targetScene = SceneManager.GetSceneByPath(scenePath);

			var prevActScene = SceneManager.GetActiveScene();

			if (targetScene.IsValid())
			{
				SceneManager.SetActiveScene(targetScene);
			}

			var videoSystemInstance = CreatePanoramaVideoSystem();
			SceneManager.SetActiveScene(prevActScene);
			return videoSystemInstance;
		}

		private PanoramaVideoSystem CreatePanoramaVideoSystem()
		{
			var videoGameobject = new GameObject("VideoPlayer - Editor");
			var videoSystem = videoGameobject.GetOrAddComponent<PanoramaVideoSystem>();
			return videoSystem;
		}

		protected void RefreshVideoSystemReferenceAfterPlay()
		{
			if (enableOnPlayMode
				&& !EditorApplication.isPlayingOrWillChangePlaymode)
			{
				panoramaVideoSystem = null;
				panoramaVideoSystem = GetVideoSystem();
				enableOnPlayMode = false;
			}
		}

		private IEnumerable<string> GetAllVideoAssetPath()
		{
			string[] guids = AssetDatabase.FindAssets("t:VideoClip");
			var pathList = guids.Select(x => AssetDatabase.GUIDToAssetPath(x));
			return pathList;
		}

		private void BuildVideoImage()
		{
			videoDisplayImage ??= new Image();

			videoDisplayImage.style.flexGrow = 0;
			videoDisplayImage.style.minHeight = 200;
			videoDisplayImage.style.maxHeight = 200;
			videoDisplayImage.style.minWidth = 450;
			videoDisplayImage.style.maxWidth = 450;
			videoDisplayImage.style.alignSelf = Align.Center;

			if (!hideableVideoContainer.Contains(videoDisplayImage))
			{
				hideableVideoContainer.Add(new Divider(5));
				hideableVideoContainer.Add(new Label("Play to Preview Video\n"));
				hideableVideoContainer.Add(videoDisplayImage);
				hideableVideoContainer.Add(new Divider(5));
			}

			GetVideoSceneCameraImage();
			EditorCoroutineUtility.StartCoroutineOwnerless(WaitForVisualUpdate());
		}

		private string GetVideoTimeText(float time, float duration)
		{
			if (time < 0.15f)
			{
				time = 0.0f;
			}
			int timeDecimalAsInt = (int)((time - (int)time) * 100);
			int durDecimalAsInt = (int)((duration - (int)duration) * 100);
			string timeFormatted = string.Format("Current Video Time: {0}/{1} [{2:00}.{3}/{4:00}.{5}]",
				time.ToTimeString(), duration.ToTimeString(),
				time, timeDecimalAsInt, duration, durDecimalAsInt);
			return timeFormatted;
		}

        private float ClampTimeWithinLength(float value, float length)
		{
			float max = length > 0.0f ? length : 3600;
			return Mathf.Clamp(value, 0.0f, max);
		}

        private void UpdateVideoSectionTimeFromNode()
        {
			videoCutoffTime.SetValueWithoutNotify(AttachedNode.videoCutoffTime);
			videoStartTime.SetValueWithoutNotify(AttachedNode.videoStartTime);
		}

		private float GetPlaySectionDuration()
		{
			return Mathf.Max(0.0f, Mathf.Abs(AttachedNode.videoCutoffTime - AttachedNode.videoStartTime));
		}

        private void OnCutoffTimeChanged(ChangeEvent<float> e)
        {
			videoCutoffTime.SetValueWithoutNotify( ClampTimeWithinLength(e.newValue, VideoClipLength));
			DelayClampSectionTimeValues();
		}

		private void OnStartTimeChanged(ChangeEvent<float> e)
		{
			videoStartTime.SetValueWithoutNotify(ClampTimeWithinLength(e.newValue, VideoClipLength));
			DelayClampSectionTimeValues();
		}

		private IVisualElementScheduledItem delayClampSectionTimeValuesScheduledItem;
		private void DelayClampSectionTimeValues()
		{
			delayClampSectionTimeValuesScheduledItem ??= schedule.Execute(ClampSectionTimeValues);
			delayClampSectionTimeValuesScheduledItem.ExecuteLater(800);
		}

		private void ClampSectionTimeValues()
		{
			float startValue = videoStartTime.value;
			startValue = ClampTimeWithinLength(startValue, VideoClipLength);

			float endValue = videoCutoffTime.value;
			endValue = ClampTimeWithinLength(endValue, VideoClipLength);

			float tempStart = startValue;
			float tempEnd = endValue;
			startValue = Mathf.Min(tempStart, tempEnd);
			endValue = Mathf.Max(tempStart, tempEnd);

			videoStartTime.SetValueWithoutNotify(startValue);
			videoCutoffTime.SetValueWithoutNotify(endValue);

			AttachedNode.videoStartTime = startValue;
			AttachedNode.videoCutoffTime = endValue;

			UpdateLoopDurationOnSectionChanged();
		}

        private string GetAssetPathFromSimplifiedName(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return string.Empty;
			}
			return Path.GetFileName(name) == name ? Path.Combine(DEFAULT_VIDEO_DIR, name) : name;
		}
        
		private void OnChangeVideoAssetByName(ChangeEvent<string> evt)
        {
            string videoPath = null;
			if (ADD_EXTERNAL_VIDEO_TITLE == evt.newValue)
            {
				string path = EditorUtility.OpenFilePanel("Select Video Clip", "", "mp4;avi;mov;wmv;mkv;flv"); // Filter for video file extensions
				videoPath = ImportNewVideoAsset(path);
                if (string.IsNullOrWhiteSpace(videoPath))
                {
                    UpdateVideoAssetDropdownFromNodeData();
					SetHideableContainerDisplay(false);
					return;
                }
			}
            else
            {
				videoPath = GetAssetPathFromSimplifiedName(evt.newValue);
			}

			if (videoPath == panoramaVideoSystem.VideoAssetLocation)
			{
				return;
			}

			CleanCurrentVideo();

            panoramaVideoSystem.VideoAssetLocation = videoPath;
			AttachedNode.videoClipLocation = videoPath;

			PollForVideoWidthAndHeight();

			SetHideableContainerDisplay(false);
			OnLoadVideoStart();
        }

		private void PollForVideoWidthAndHeight()
		{
			//Video Clip For Width and Height
			UnityEngine.Video.VideoClip videoClip = AssetDatabase.LoadAssetAtPath<UnityEngine.Video.VideoClip>(AttachedNode.videoClipLocation);

			if (videoClip != null)
			{
				AttachedNode.videoWidth = (int)videoClip.width;
				AttachedNode.videoHeight = (int)videoClip.height;

				GetVideoSystem().SetRenderTextureSize(AttachedNode.videoWidth,AttachedNode.videoHeight);

				videoClip = null;
			}
		}

		public void CleanCurrentVideo()
		{
            panoramaVideoSystem.Stop();
            panoramaVideoSystem.RestorePrevSkyBoxMaterial();
            panoramaVideoSystem.VideoAssetLocation = null;
        }

		private void OnLoadVideoStart() 
		{
            bool canPlay = panoramaVideoSystem.CanPlay;
			if (!canPlay)
			{
				return;
			}
			isLoadingVideo = true;
            videoObjectField.SetEnabled(false);
			originVideoNameDropdownValue = videoObjectField.value;
            videoObjectField.SetValueWithoutNotify("Loading " + originVideoNameDropdownValue);
            panoramaVideoSystem.currentVideoPlayer.RegisterOneTimeCallback(VideoPlayerEventType.FinishedSeeking, OnLoadVideoEnd);
            PreviewNodeVideoAtStartTime();
        }
        private void OnLoadVideoEnd(IAdvancedVideoPlayer videoPlayer, VideoPlayerEventType eventType, string error)
        {
            videoObjectField.SetValueWithoutNotify(originVideoNameDropdownValue);
            videoObjectField.SetEnabled(true);
            SetHideableContainerDisplay(false);
            RefreshClipDurationFromVideoSystem();
			AutoSetupInitValues();
            UpdateViewDataFromNode();
            refresh.Invoke();
            isLoadingVideo = false;
			PreviewNodeVideoAtStartTime();
        }


        private string ImportNewVideoAsset(string fullFilePath)
        {
            if (string.IsNullOrWhiteSpace(fullFilePath))
            {
                return null;
            }

            if (!File.Exists(fullFilePath))
            {
                return null;
            }
			
            string fileName = Path.GetFileName(fullFilePath);

			string videoDirPath = DEFAULT_VIDEO_DIR;
			if (!Directory.Exists(videoDirPath))
            {
				Directory.CreateDirectory(videoDirPath);
			}
				

			string videoPath = Path.Combine(videoDirPath,  fileName);

			if (!string.IsNullOrEmpty(fileName))
			{
				FileUtil.ReplaceFile(fullFilePath, videoPath);
				AssetDatabase.Refresh();

                return videoPath;
			}
            return null;
		}

		private void PlayVideoInEditor()
		{
			AdvPlayVideoInEditor(false);
		}
		private void PlayVideoSectionInEditor()
		{
			AdvPlayVideoInEditor(true);
		}

		private void AdvPlayVideoInEditor(bool usePlayMode)
		{
            RefreshBeforePlay();
            if (panoramaVideoSystem.IsPlaying)
			{
				panoramaVideoSystem.Stop();
				StopUpdateOnVideoPlay();
				SetupVideoButtonIconPlay();
			}
			else
			{
				panoramaVideoSystem.Reset();
				if (usePlayMode)
				{
					panoramaVideoSystem
						.SetPlaySection(AttachedNode.videoStartTime, AttachedNode.videoCutoffTime)
						.SetCustomLoopSection(AttachedNode.loopSectionStartTime, AttachedNode.loopSectionEndTime)
						.SetPlayMode(AttachedNode.playMode.ToVideoPlayMode())
						.PlayWithConfig(StartUpdateOnVideoPlay);
				}
				else
				{
					panoramaVideoSystem.PlayFrom(slider.value, StartUpdateOnVideoPlay);
				}
			}
		}

		private void StartUpdateOnVideoPlay()
		{
			StopUpdateOnVideoPlay();
            SetupVideoButtonIconPause();
			RefreshClipDurationFromVideoSystem();
            videoEditorCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(OnPlayVideo());
		}

		private void StopUpdateOnVideoPlay()
		{
			if (null != videoEditorCoroutine)
			{
				EditorCoroutineUtility.StopCoroutine(videoEditorCoroutine);
				videoEditorCoroutine = null;
			}
		}

		private IEnumerator OnPlayVideo()
        {
			int delayStopFrameCountAfterNotPlaying = 10;
			int delayCount = delayStopFrameCountAfterNotPlaying;
			while(panoramaVideoSystem.IsPlaying || delayCount > 0)
			{
				slider.SetValueWithoutNotify(panoramaVideoSystem.CurrentPlayTime);
				UpdateVideoTimeLabel();
				if (panoramaVideoSystem.IsPlaying)
				{
					UpdatePreviewTexture();
					delayCount = 10;
				}
				else
				{
					--delayCount;
				}
				yield return null;
			}
            SetupVideoButtonIconPlay();
        }

		private void PreviewNodeVideoAtStartTime()
		{
			PreviewNodeVideoAtTime(AttachedNode.videoStartTime);
		}
		private void PreviewNodeVideoAtCutoffTime()
		{
			PreviewNodeVideoAtTime(AttachedNode.videoCutoffTime);
		}

        private void PreviewNodeVideoAtTime(float time)
        {
            RefreshBeforePlay();
            panoramaVideoSystem.PreviewAt(time, OnVideoTimeUpdate);
		}

		private void LoadNodeVideoToPlayer()
		{
			if (panoramaVideoSystem.VideoAssetLocation == AttachedNode.videoClipLocation)
			{
				return;
			}
			panoramaVideoSystem.VideoAssetLocation = AttachedNode.videoClipLocation;
		}

		private void RefreshSkyboxMaterial()
		{
			GetVideoSystem().ApplySkyBoxMaterial();
		}

		private void RefreshBeforePlay()
		{
            GetVideoSystem();
            RefreshVideoSystemReferenceAfterPlay();
            LoadNodeVideoToPlayer();
			RefreshSkyboxMaterial();
			panoramaVideoSystem.VirtualNorth = AttachedNode.virtualNorth;
        }

		#region Video Asset Dropdown
		private VisualElement BuildVideoAssetDropdown()
		{
			videoObjectField = new DropdownField
			{
				label = "Add Video Clip To Play: ",
				tooltip = "Video file to play"
			};
            videoObjectField.tooltip = videoObjectField.value;
            videoObjectField.RegisterCallback<PointerEnterEvent>(_ => RefreshVideoAssetDropdownChoices(videoObjectField));
			videoObjectField.RegisterValueChangedCallback(OnChangeVideoAssetByName);
			
			RefreshVideoAssetDropdownChoices(videoObjectField);

			return videoObjectField;
		}

		private void RefreshVideoAssetDropdownChoices(DropdownField videoObjectField)
		{
			videoObjectField.choices.Clear();
			IEnumerable<string> pathList = GetAllVideoAssetPath();
			List<string> otherAssets = new();
			foreach (string path in pathList)
			{
				if (path.StartsWith(DEFAULT_VIDEO_DIR))
				{
					videoObjectField.choices.Add(path.Replace(DEFAULT_VIDEO_DIR, ""));
				}
				else
				{
					otherAssets.Add(path);
				}
			}
			videoObjectField.choices.AddRange(otherAssets);
			videoObjectField.choices.Add(ADD_EXTERNAL_VIDEO_TITLE);
		}

		private void UpdateVideoAssetDropdownFromNodeData()
		{
			string name = AttachedNode.videoClipLocation;

			if (string.IsNullOrWhiteSpace(AttachedNode.videoClipLocation))
			{
				name = "None";
			}

			videoObjectField.SetValueWithoutNotify(name);
		}
		#endregion Video Asset Dropdown

		#region Video Time Control
		private VisualElement BuildVideoTimeLabel()
		{
			videoTimeLabel = new Label();
			videoTimeLabel.style.alignSelf = Align.FlexStart;

			return videoTimeLabel;
		}

		private EditorCoroutine seekAfterCorountine;

		private void UpdateAfterSeeking()
		{
			UpdateVideoTimeLabel();

			if(seekAfterCorountine != null)
			{
				EditorCoroutineUtility.StopCoroutine(seekAfterCorountine);
				seekAfterCorountine = null;
			}

			seekAfterCorountine = EditorCoroutineUtility.StartCoroutineOwnerless(WaitForVisualUpdate());
		}

		private IEnumerator WaitForVisualUpdate()
		{
			yield return new WaitForEndOfFrame();

			UpdatePreviewTexture();
		}

		private void UpdateVideoTimeLabel()
		{
			videoTimeLabel.text = GetVideoTimeText();
		}

        private string GetVideoTimeText()
        {
            if (null == panoramaVideoSystem
                || panoramaVideoSystem.VideoAssetLocation != AttachedNode.videoClipLocation)
            {
                return GetVideoTimeText(slider.value, VideoClipLength);
            }
            float currentVideoTime = panoramaVideoSystem.CurrentPlayTime;
            float duration = panoramaVideoSystem.CurrentVideoDuration;
            return GetVideoTimeText(currentVideoTime, duration);
        }

        private VisualElement BuildVideoTimeSlider()
		{
			slider = new Slider();
			slider.RegisterCallback<ChangeEvent<float>>(OnVideoTimeChanged);

			videoIconButton = new IconButton(PlayVideoInEditor, "play", 16)
			{
				tooltip = "Play"
			};

			slider.Add(CreateSetValueButton(slider, () => 0.0f, "Jump to Begin", "CollapseLeft"));
			slider.Add(videoIconButton);
			slider.Add(CreateSetValueButton(slider, () => VideoClipLength, "Jump to End", "CollapseRight"));
			slider.Add(CreateSetValueButton(slider, () => AttachedNode.videoStartTime, "Jump to Section Start", "ContainStart"));
			playSectionButton = CreatePlaySectionButton();
			slider.Add(playSectionButton);
			slider.Add(CreateSetValueButton(slider, () => AttachedNode.videoCutoffTime, "Jump to Section End", "ContainEnd"));
			return slider;
		}

		IVisualElementScheduledItem callback;
		private float delaySeekTime;
		private void OnVideoTimeChanged(ChangeEvent<float> e)
		{
			delaySeekTime = e.newValue;
			RefreshBeforePlay();

			// Don't seek multiple times when silding value cause seek may very slow.
			// Seek once when stop sliding.
			if (null == callback)
			{
				callback = this.schedule.Execute(DelaySeekCallback);
			}
			callback.ExecuteLater(100);
		}

		private void DelaySeekCallback()
		{
			//TODO: Handle Seek 0
			if (delaySeekTime == 0)
				delaySeekTime = 0.1f;

			panoramaVideoSystem.Seek(delaySeekTime, UpdateAfterSeeking);
			callback = null;
		}

		private void UpdateVideoTimeSliderFromNode()
		{
			slider.lowValue = 0f;
			slider.highValue = ClampTimeWithinLength(VideoClipLength, VideoClipLength);
            slider.SetValueWithoutNotify(AttachedNode.videoStartTime);
			videoTimeLabel.text = GetVideoTimeText(slider.value, slider.highValue);
        }

		private void SetupVideoButtonIconPlay()
		{
            videoIconButton.iconImage.image = GetIcon("play");
			playSectionButton.iconImage.image = GetIcon("play");
        }
        private void SetupVideoButtonIconPause()
        {
            videoIconButton.iconImage.image = GetIcon("pause");
			playSectionButton.iconImage.image = GetIcon("pause");
		}

		private Texture2D GetIcon(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				return null;
			}
			return Resources.Load<Texture2D>("Icon/" + name);
		}
		private void OnVideoTimeUpdate()
		{
			RefreshClipDurationFromVideoSystem();
			UpdateVideoTimeLabel();
		}
		private void RefreshClipDurationFromVideoSystem()
		{
			if (null == panoramaVideoSystem
				|| panoramaVideoSystem.VideoAssetLocation != AttachedNode.videoClipLocation)
			{
				return;
			}
			VideoClipLength = panoramaVideoSystem.CurrentVideoDuration;
		}

		private void OnClipLengthChanged()
		{
			ClampSectionTimeValues();
			slider.highValue = VideoClipLength;
			slider.SetValueWithoutNotify(videoStartTime.value);
			UpdateVideoTimeLabel();
		}
        #endregion Video Time Control

        #region Start Stop Time Views
        private void BuildStartStopTimeControls()
		{
			videoStartTime = new FloatField("Video Start Time: ");
			videoStartTime.RegisterValueChangedCallback(OnStartTimeChanged);
			videoDataDropDownElements.Add(videoStartTime);

			videoCutoffTime = new FloatField("Video Cut Off Time: ");
			videoCutoffTime.RegisterValueChangedCallback(OnCutoffTimeChanged);
			videoDataDropDownElements.Add(videoCutoffTime);


			var setStartTimePreviewButton = CreatePreviewStartTimeButton();
			videoStartTime.Add(setStartTimePreviewButton);
			videoStartTime.Add(CreatePullCurrentTimeButton(videoStartTime));
			//videoStartTime.Add(CreateSetValueButton(videoStartTime, ()=>0.0f, "Set to Begin", "CollapseLeft"));

			var setEndTimePreviewButton = CreatePreviewEndTimeButton();
			videoCutoffTime.Add(setEndTimePreviewButton);
			videoCutoffTime.Add(CreatePullCurrentTimeButton(videoCutoffTime));
			//videoCutoffTime.Add(CreateSetValueButton(videoCutoffTime, ()=> VideoClipLength, "Set to End", "CollapseRight"));
		}

		private Button CreatePullCurrentTimeButton(FloatField targetFloatField)
		{
			var button = new IconButton("CollapseDown", 16);
			button.tooltip = "Pick Current Video Time";
			button.clicked += () => { 
				if (null == targetFloatField)
				{
					return;
				}
				targetFloatField.value = slider.value;
			};
			return button;
		}

		private Button CreateSetValueButton(INotifyValueChanged<float> targetValueField, Func<float> getValueCallback, string tooltip, string icon)
		{
			var button = new IconButton(icon, 16)
			{
				tooltip = tooltip
			};
			button.clicked += () => {
				if (null == targetValueField)
				{
					return;
				}
				targetValueField.value = getValueCallback?.Invoke() ?? 0.0f;
			};
			return button;
		}

		
		private IconButton CreatePlaySectionButton()
		{
			IconButton button = new("play", 16);
			button.SetBackgroundImage(GetIcon("Contain"));
			button.tooltip = "Play Section with Settings";
			button.clicked += PlayVideoSectionInEditor;
			return button;
		}


		private Button CreatePreviewStartTimeButton()
		{
			IconButton button = new("Target", 16)
			{
				tooltip = "Preview Start Time"
			};
			button.clicked += PreviewNodeVideoAtStartTime;
			return button;
		}

		private Button CreatePreviewEndTimeButton()
		{
			IconButton button = new("Target", 16)
			{
				tooltip = "Preview End Time"
			};
			button.clicked += PreviewNodeVideoAtCutoffTime;
			return button;
		}

		private void UpdateStartStopTimeFromNode()
		{
			videoStartTime.value = AttachedNode.videoStartTime;
			videoCutoffTime.value = AttachedNode.videoCutoffTime;
		}
		#endregion Start Stop Time Views

		private void BuildPlayModeField()
		{
			EnumField playModeField = new("PlayMode:", AttachedNode.playMode);
			playModeField.RegisterValueChangedCallback(OnPlayModeChanged);
			videoDataDropDownElements.Add(playModeField);
		}

		private void OnPlayModeChanged(ChangeEvent<Enum> evt)
		{
			AttachedNode.playMode = (PanoramaVideoNode.PlayMode)evt.newValue;
			UpdateLoopValuesFromNode();
		}

		#region Loop
		public void BuildLoopVideoToggle()
		{
			var loopToggle = new Toggle()
			{
				label = "Loop Entire Video: ",
			};
			loopToggle.RegisterCallback<ChangeEvent<bool>>(OnVideoLoopChanged);
			videoDataDropDownElements.Add(loopToggle);
		}

		private void OnVideoLoopChanged(ChangeEvent<bool> evt)
		{
			if (evt.newValue != (AttachedNode.playMode == PanoramaVideoNode.PlayMode.LoopSection ))
			{
				AttachedNode.playMode = evt.newValue ? PanoramaVideoNode.PlayMode.LoopSection : PanoramaVideoNode.PlayMode.Section;
			}
		}

		public void BuildLoopSectionToggle()
		{
			var loopSectionToggle = new Toggle()
			{
				label = "Loop Section Of Video: ",
			};
			loopSectionToggle.RegisterCallback<ChangeEvent<bool>>(OnVideoLoopSectionChanged);
			videoDataDropDownElements.Add(loopSectionToggle);
		}

		private void OnVideoLoopSectionChanged(ChangeEvent<bool> evt)
		{
			//AttachedNode.loopSectionVideo = evt.newValue;
			EnableLoopSectionDruationField(evt.newValue);
		}

		private void EnableLoopSectionDruationField(bool enabled)
		{
			loopSectionDuration?.SetDisplay(enabled);
		}

		public void BuildLoopSectionDurationField()
		{
			loopSectionDuration = new FloatField("Loop Last Seconds: ");
			loopSectionDuration.RegisterValueChangedCallback(OnLoopSectionDurationChanged);
			videoDataDropDownElements.Add(loopSectionDuration);
		}

		private void OnLoopSectionDurationChanged(ChangeEvent<float> e)
		{
			UpdateLoopDurationOnSectionChanged();
		}

		private void UpdateLoopValuesFromNode()
		{
			float duration = ClampLoopDurationValue(AttachedNode.loopSectionEndTime - AttachedNode.loopSectionStartTime);
			loopSectionDuration.SetValueWithoutNotify(duration);
			EnableLoopSectionDruationField(PanoramaVideoNode.PlayMode.SectionThanLoopLastSeconds == AttachedNode.playMode);
		}
		private void UpdateLoopDurationOnSectionChanged()
		{
			float maxValue = GetPlaySectionDuration();
			if (loopSectionDuration.value > maxValue)
			{
				loopSectionDuration.SetValueWithoutNotify(maxValue);
			}
			AttachedNode.loopSectionStartTime = videoCutoffTime.value - loopSectionDuration.value;
			AttachedNode.loopSectionEndTime = videoCutoffTime.value;
		}
		private float ClampLoopDurationValue(float rawTime)
		{
			float v = Mathf.Max(0.0f, Mathf.Abs(rawTime));
			return Mathf.Clamp(v, 0.0f, Mathf.Abs(AttachedNode.videoCutoffTime - AttachedNode.videoStartTime));
		}


		#endregion Loop


		#region Audio Dropdown
		private void BuildAudioDropdown()
		{
			var audioSettingsContainer = new VisualElement
			{
				name = "audio-settings-container"
			};

			audioDropdown = CreateAudioDropDown();
			RefreshAudioAssetDropdownChoices();
			audioSettingsContainer.Add(audioDropdown);
			videoDataDropDownElements.Add(audioSettingsContainer);

		}

		private DropdownField CreateAudioDropDown()
		{
			var dropdown = new DropdownField
			{
				label = "Add Audio File: ",
				tooltip = "Audio file to play"
			};
			dropdown.RegisterCallback<ChangeEvent<string>>(OnAudioAssetChanged);
			return dropdown;
		}

		private void RefreshAudioAssetDropdownChoices()
		{
			audioDropdown.choices.Clear();
			//find all custom imported
			if (ScriptableObjectManager.GetAllInstances<AudioSO>().Count != 0)
			{
				foreach (AudioSO audio in ScriptableObjectManager.GetAllInstances<AudioSO>())
				{
					if (audio.clip == null)
					{
						continue;
					}
					audioDropdown.choices.Add("Audio Clips/" + audio.clip.name);
				}
			}
			audioDropdown.choices.Add("External/Add Audio Clip");
		}

		private void OnAudioAssetChanged(ChangeEvent<string> evt)
		{
			AudioSO asset = null;
			if (evt.newValue.Equals("External/Add Audio Clip"))
			{
				asset = OnAddCustomAudio(evt.previousValue);
			}
			else
			{
				string[] newValSplit = evt.newValue.Split('/');
				asset = ScriptableObjectManager.GetAllInstances<AudioSO>().Find(t => t.clip.name.Equals(newValSplit[newValSplit.Length - 1]));
			}

			if (asset != null)
			{
				AttachedNode.AssociatedCustomClip = asset.clip.name;
				AttachedNode.customTrackAudio = asset.clip;
			}
		}

		private void UpdateAudioAssetDropdownFromNode()
		{
			if (!string.IsNullOrEmpty(AttachedNode.AssociatedCustomClip))
			{
				//else search all custom
				audioDropdown.index = ScriptableObjectManager.GetAllInstances<AudioSO>().FindIndex(
				t => t.clip.name == AttachedNode.AssociatedCustomClip);

				audioDropdown.SetValueWithoutNotify(audioDropdown.index != -1 ? audioDropdown.choices[audioDropdown.index] : "Null");
			}
			else
			{
				audioDropdown.SetValueWithoutNotify("Null");
			}
		}
		#endregion Audio Dropdown


		#region Virtual North
		private void BuildVirtualNorthSlider()
		{
			virtualNorthSlider = new SliderInt("Virtual North: ", 0, 360);
			virtualNorthSlider.RegisterValueChangedCallback(OnVirtualNorthSliderChanged);

			IntegerField virtualNorthField = new()
			{
				value = virtualNorthSlider.value,
				style =
				{
					minWidth = 55,
					paddingLeft = 5
				}
			};
			virtualNorthField.RegisterValueChangedCallback(OnVirtualNorthSliderChanged);

			virtualNorthSlider.Add(virtualNorthField);
			
			videoDataDropDownElements.Add(virtualNorthSlider);
		}

		private void OnVirtualNorthSliderChanged(ChangeEvent<int> evt)
		{
			AttachedNode.virtualNorth = evt.newValue;
			UpdateVirtualNorthFromNode();
			panoramaVideoSystem.VirtualNorth = evt.newValue;
			PreviewNodeVideoAtTime(slider.value);
		}

		private void UpdateVirtualNorthFromNode()
		{
			virtualNorthSlider.SetValueWithoutNotify(AttachedNode.virtualNorth);
			var intField = virtualNorthSlider.Q<IntegerField>();
			intField?.SetValueWithoutNotify(AttachedNode.virtualNorth);
		}
		#endregion Virtual North

		#region Volume Visuals
		private void BuildVolumeVisual()
		{
			var volumeSlider = new Slider("Volume: ", 0, 1);
			volumeSlider.RegisterValueChangedCallback(OnVolumeSliderChanged);

			volumeField = new FloatField();
			volumeField.style.minWidth = 55;
			volumeField.style.paddingLeft = 5;

			volumeSlider.Add(volumeField);

			volumeDropdownElements.Add(volumeSlider);
			
			volumeSlider.SetValueWithoutNotify(AttachedNode.volumeSetting);
			volumeField.SetValueWithoutNotify(volumeSlider.value);

		}

		private void OnVolumeSliderChanged(ChangeEvent<float> evt)
		{
			AttachedNode.volumeSetting = evt.newValue;
			volumeField.value = evt.newValue;
		}

		private void BuildVolumeBoosterVisual()
		{
			var volumeBoosterSlider = new Slider("Volume Booster: ", 0, 20f)
			{
				value = AttachedNode.volumeBooster
			};

			volumeBoosterField = new FloatField
			{
				value = volumeBoosterSlider.value,
				style =
				{
					minWidth = 55,
					paddingLeft = 5
				}
			};
			volumeBoosterSlider.Add(volumeBoosterField);
			
			volumeBoosterSlider.RegisterValueChangedCallback(OnVolumeBoosterSliderChanged);
			volumeDropdownElements.Add(volumeBoosterSlider);
		}

		private void OnVolumeBoosterSliderChanged(ChangeEvent<float> evt)
		{
			AttachedNode.volumeBooster = evt.newValue;
			volumeBoosterField.value = evt.newValue;
		}

		#endregion Volume Visuals

		#region Video Type Visuals

		private void OnTransitionValueChanged(ChangeEvent<Enum> evt)
		{
			AttachedNode.transitionType = (VideoTransitionType)evt.newValue;
		}

		private void OnVideoImageValueChanged(ChangeEvent<Enum> evt)
		{
			AttachedNode.videoImageType = (VideoImageType)evt.newValue;

			GetVideoSystem().SetImageType(AttachedNode.videoImageType, AttachedNode.videoLayoutType);
		}

		private void OnVideoLayoutChanged(ChangeEvent<Enum> evt)
		{
			AttachedNode.videoLayoutType = (VideoLayoutType)evt.newValue;

			GetVideoSystem().SetImageType(AttachedNode.videoImageType, AttachedNode.videoLayoutType);
		}

		#endregion Video Type Visuals

		private VisualElement BuildFoldoutGroups()
		{
			hideableVideoContainer ??= new VisualElement();
			
			AdvancedDropdown videoDataDropdown = new("Video Data: ", GetAllVideoDataGroupChildren, true);
			hideableVideoContainer.Add(videoDataDropdown);

			AdvancedDropdown volumeDropdown = new("Volume Settings: ", GetAllVolumeGroupChildren, false);
			hideableVideoContainer.Add(volumeDropdown);

			
			hideableVideoContainer.Add(new Divider());
			hideableVideoContainer.Add(new Label("Video Type: "));
			
			EnumField transitionField = new("Video Transition:", AttachedNode.transitionType);

			transitionField.RegisterValueChangedCallback(OnTransitionValueChanged);
			hideableVideoContainer.Add(transitionField);
			
			hideableVideoContainer.Add(AttachedNode.CustomFloatField(nameof(AttachedNode.fadeDuration), "Fade Duration"));
			
			EnumField videoImageTypeField = new("Video Image Type:", AttachedNode.videoImageType);
			videoImageTypeField.RegisterValueChangedCallback(OnVideoImageValueChanged);
			hideableVideoContainer.Add(videoImageTypeField);
			
			EnumField videoLayoutField = new("Video Layout Type:", AttachedNode.videoLayoutType);
			videoLayoutField.RegisterValueChangedCallback(OnVideoLayoutChanged);
			hideableVideoContainer.Add(videoLayoutField);


			
			return hideableVideoContainer;
		}

		private void SetHideableContainerDisplay(bool display)
		{
			hideableVideoContainer?.SetDisplay(display);
		}

		private VisualElement[] GetAllVideoDataGroupChildren()
		{
			videoDataDropDownElements.ForEach(x => x.SetMargin(20, 0, 0, 0));
			VisualElement[] visualElements = videoDataDropDownElements.ToArray();

			return visualElements;
		}

		private VisualElement[] GetAllVolumeGroupChildren()
		{
			volumeDropdownElements.ForEach(x => x.SetMargin(20, 0, 0, 0));
			return volumeDropdownElements.ToArray();
		}
		
		#region audio and events

		IEnumerator DelayedUpdatePorts()
		{
			yield return new EditorWaitForSeconds(0.05f);
		}
		
		private AudioSO OnAddCustomAudio(string previousValue)
		{
			AudioClip clip = AskUserForWav.TryAddAudioFile("Error in Finding that File",
				 "Could not detect a .wav or .ogg filetype", out string filename);

			if (clip == null)
			{
				return null;
			}

			AudioSO returnAsset = null;

			if (!ScriptableObjectManager.CheckIfAssetNameExists<AudioSO>(filename))
			{
				returnAsset = ScriptableObjectManager.CreateScriptableObject<AudioSO>("Assets/Contexts/Scriptables", filename);
				returnAsset.clip = clip;
				ScriptableObjectManager.ForceSerialization(returnAsset);
			}
			else
			{
				foreach (AudioSO asset in ScriptableObjectManager.GetAllInstances<AudioSO>())
				{
					if (asset.name.Equals(filename))
					{
						asset.clip = clip;
						ScriptableObjectManager.ForceSerialization(asset);
						returnAsset = asset;
						break;
					}
				}
			}

			if (returnAsset != null)
			{
				audioDropdown.value = "Audio Clips/" + returnAsset.clip.name;
			}
			else
			{
				audioDropdown.value = previousValue;
			}

			return returnAsset;
		}
		#endregion
	}
}