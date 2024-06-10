using SkillsVR.TimelineTool.Editor.TimelineExtensions;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;


namespace SkillsVR.TimelineTool.Editor.TimelineExtensions
{
    [CustomEditor(typeof(BlendableAnimationTrack)), CanEditMultipleObjects]
	public class BlendableAnimationTrackInspector : UnityEditor.Editor
	{
		protected static GUIContent applyEaseInButtonLabel = new GUIContent("Apply for All Clips", "Override ease in duration for all children clips.");
		protected static GUIContent applyEaseOutButtonLabel = new GUIContent("Apply for All Clips", "Override ease out duration for all children clips.");
		protected static GUIContent applyLoopModeButtonLabel = new GUIContent("Apply for All Clips", "Override loop mode for all children animation clips.");

		public bool advancedMode = false;

		protected SerializedProperty applyAvatarMaskProperty;
		protected SerializedProperty avatarMaskProperty;

		protected SerializedProperty easeInProperty;
		protected SerializedProperty easeOurProperty;
		protected SerializedProperty loopModeProperty;

		protected BlendableAnimationTrack smartTarget;

		protected void OnEnable()
		{
			smartTarget = serializedObject.targetObject as BlendableAnimationTrack;
			applyAvatarMaskProperty = serializedObject.FindProperty("m_ApplyAvatarMask");
			avatarMaskProperty = serializedObject.FindProperty("m_AvatarMask");

			easeInProperty = serializedObject.FindProperty(nameof(BlendableAnimationTrack.defaultEaseInDuration));
			easeOurProperty = serializedObject.FindProperty(nameof(BlendableAnimationTrack.defaultEaseOurDuration));
			loopModeProperty = serializedObject.FindProperty(nameof(BlendableAnimationTrack.defaultAnimLoopMode));
		}


		public override void OnInspectorGUI()
		{
			if (!advancedMode)
			{
				EditorGUILayout.PropertyField(applyAvatarMaskProperty);
				if (applyAvatarMaskProperty.boolValue)
				{
					EditorGUI.indentLevel++;
					EditorGUILayout.PropertyField(avatarMaskProperty);
					EditorGUILayout.HelpBox("Applying an Avatar Mask to the base track may not properly mask Root Motion or Humanoid bones from an Animator Controller or ther Timeline track.", MessageType.Warning);
					EditorGUI.indentLevel--;
				}

				EditorGUILayout.Space();

				EditorGUILayout.LabelField("Init values for new added clips", EditorStyles.boldLabel);

				DrawPropertyWithApplyButton(easeInProperty, applyEaseInButtonLabel, ApplyEaseInForAllChildren);
				DrawPropertyWithApplyButton(easeOurProperty, applyEaseOutButtonLabel, ApplyEaseOutForAllChildren);
				DrawPropertyWithApplyButton(loopModeProperty, applyLoopModeButtonLabel, ApplyLoopModeForAllChildren);

				EditorGUILayout.Space();

				serializedObject.ApplyModifiedProperties();
			}

			advancedMode = EditorGUILayout.Toggle("Advanced Mode", advancedMode);
			if (advancedMode)
			{
				EditorGUILayout.HelpBox("Don't use advanced mode unless you know what you are doing.", MessageType.Warning);
				base.OnInspectorGUI();
			}

			EditorGUILayout.Space();
			EditorGUILayout.Space();

			if (GUILayout.Button("Reset Default Ease Duration from Performance"))
			{
				easeInProperty.floatValue = TimelineBlenderPreferences.instance.defaultEaseInDuration;
				easeOurProperty.floatValue = TimelineBlenderPreferences.instance.defaultEaseOurDuration;
				serializedObject.ApplyModifiedProperties();
			}
		}

		protected void ApplyEaseInForAllChildren()
		{
			smartTarget.GetClips().ToList().ForEach((clip) => { clip.easeInDuration = easeInProperty.floatValue; });
		}

		protected void ApplyEaseOutForAllChildren()
		{
			smartTarget.GetClips().ToList().ForEach((clip) => { clip.easeOutDuration = easeOurProperty.floatValue; });
		}

		protected void ApplyLoopModeForAllChildren()
		{
			smartTarget.GetClips().ToList().ForEach((clip) =>
			{
				var animClip = clip.GetAssetAs<AnimationPlayableAsset>();
				animClip.loop = (AnimationPlayableAsset.LoopMode)loopModeProperty.enumValueIndex;
			});
		}

		protected void DrawPropertyWithApplyButton(SerializedProperty property, GUIContent buttonLabel, Action buttonAction)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(property);
			if (GUILayout.Button(buttonLabel, EditorStyles.miniButton))
			{
				buttonAction?.Invoke();
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}