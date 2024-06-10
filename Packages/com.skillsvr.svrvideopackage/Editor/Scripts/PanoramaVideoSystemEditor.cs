using SkillsVR.VideoPackage;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

[CustomEditor(typeof(PanoramaVideoSystem), true)]
public class PanoramaVideoSystemEditor : AdvancedVideoControlEditor
{
	float startFromTime;
	public override void OnInspectorGUI()
	{
		repaintInBase = false;
		base.OnInspectorGUI();


		var sys = target as PanoramaVideoSystem;

		if (GUILayout.Button("Play as Config"))
		{
			sys.RemoveAllTimeEvents();
			sys.PlayWithConfig();
		}

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Play from"))
		{
			sys.PlayFrom(startFromTime);
		}
		startFromTime = EditorGUILayout.Slider(startFromTime, 0.0f, sys.CurrentVideoDuration);
		
		EditorGUILayout.EndHorizontal();
		Repaint();
	}
}
