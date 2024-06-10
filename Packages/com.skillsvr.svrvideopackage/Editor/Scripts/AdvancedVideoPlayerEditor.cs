using SkillsVR.VideoPackage;
using UnityEditor;
using UnityEngine;

public class AdvancedVideoPlayerEditor : AdvancedVideoControlEditor
{
	public override void OnInspectorGUI()
	{
		repaintInBase = false;
		base.OnInspectorGUI();


		var sys = target as IAdvancedVideoPlayer;
		if (null == sys)
		{
			EditorGUILayout.LabelField("This component is not IAdvancedVideoPlayer type.");
			return;
		}

		sys.CurrentRenderTexture = EditorGUILayout.ObjectField("RenderTexture", sys.CurrentRenderTexture, typeof(RenderTexture), false) as RenderTexture;
		sys.SkyBoxMaterial = EditorGUILayout.ObjectField("Material", sys.SkyBoxMaterial, typeof(Material), false) as Material;

		if (null != sys.SkyBoxMaterial
			&& RenderSettings.skybox != sys.SkyBoxMaterial
			&& GUILayout.Button("Set as Skybox"))
		{
			RenderSettings.skybox = sys.SkyBoxMaterial;
		}

		Repaint();
	}
}
