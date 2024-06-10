using SkillsVR.VideoPackage;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

#if AV_PRO
[CustomEditor(typeof(AdvancedAVProMediaPlayerController))]
public class EA : AdvancedVideoPlayerEditor { }
#endif

[CustomEditor(typeof(AdvancedVideoPlayerController))]
public class EB : AdvancedVideoPlayerEditor { }

public class AdvancedVideoControlEditor : Editor
{
	protected float timeSliderValue = 0.0f;

	protected VideoClip currentVideoClip;

	protected float dragToSeek;
	protected float lastDragSeekTime;
	protected bool enableDelaySeek;
	protected bool pauseTimeSliderUpdate;

	public bool repaintInBase = true;

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.LabelField("== Editor Control ==");

		var sys = target as IAdvancedVideoControl;
		if (null == sys)
		{
			EditorGUILayout.LabelField("This component is not IAdvancedVideoControl type.");
			return;
		}

		// Prepare video clip
		bool reloadClip = null == currentVideoClip || sys.VideoAssetLocation != currentVideoClip.originalPath;
		if (reloadClip && !string.IsNullOrWhiteSpace(sys.VideoAssetLocation))
		{
			currentVideoClip = AssetDatabase.LoadAssetAtPath<VideoClip>(sys.VideoAssetLocation);
		}

		// Easy asset setup from video clip assets
		var newClip = EditorGUILayout.ObjectField("Video Clip", currentVideoClip, typeof(VideoClip), false) as VideoClip;
		if (newClip != currentVideoClip)
		{
			sys.VideoAssetLocation = null == newClip ? null : newClip.originalPath;
			currentVideoClip = newClip;
		}

		// Video asset location
		EditorGUILayout.LabelField("Video Asset Location: " + (string.IsNullOrWhiteSpace(sys.VideoAssetLocation) ? "none" : sys.VideoAssetLocation));

		// Time Slider
		float dur = null != currentVideoClip ? (float)(currentVideoClip.length) : 1.0f;
		float time = sys.CurrentPlayTime;

		EditorGUILayout.BeginHorizontal();
		string playTimeTxt = string.Format("Time: {0}/{1}", time.ToTimeString(), dur.ToTimeString());
		EditorGUI.BeginChangeCheck();
		timeSliderValue = EditorGUILayout.Slider(playTimeTxt, timeSliderValue, 0.0f, dur);
		if (EditorGUI.EndChangeCheck())
		{
			dragToSeek = timeSliderValue;
			enableDelaySeek = true;
			lastDragSeekTime = Time.realtimeSinceStartup;
			pauseTimeSliderUpdate = true;
		}
		else if (!pauseTimeSliderUpdate)
		{
			timeSliderValue = time;
		}
		EditorGUILayout.EndHorizontal();


		if (enableDelaySeek && Time.realtimeSinceStartup - lastDragSeekTime > 0.15f)
		{
			enableDelaySeek = false;
			sys.Seek(dragToSeek, () => { pauseTimeSliderUpdate = false;});
		}

		// Play state
		EditorGUILayout.LabelField("CanPlay " + sys.CanPlay);
		EditorGUILayout.LabelField("IsPlaying " + sys.IsPlaying);

		if (GUILayout.Button("Update location"))
		{
			sys.VideoAssetLocation = sys.VideoAssetLocation;
		}
		
		// Control methods
		if (GUILayout.Button("Play"))
		{
			sys.Play();
		}

		if (GUILayout.Button("Pause"))
		{
			sys.Pause();
		}
		if (GUILayout.Button("Stop"))
		{
			sys.Stop();
		}

		

		if (repaintInBase)
		{
			Repaint();
		}
		
	}
}
