using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	public class AnimationPreview : VisualElement
	{
		private GameObject model;
		private AnimationClip animationClip;
		private UnityEditor.Editor editor;
		private readonly Slider slider;
		private bool isPlaying;

		public AnimationPreview(GameObject gameObject, AnimationClip animationClip)
		{
			model = gameObject;
			this.animationClip = animationClip;
		    
			slider = CreateAnimationClipSlider(animationClip ? animationClip.length : 0);
			Add(slider);
			Add(CreateEditor());
		}
		
		~AnimationPreview()
		{
			Object.DestroyImmediate(editor);
		}

		public void ChangeAnimationClip(AnimationClip newAnimationClip)
		{
			animationClip = newAnimationClip;
			slider.SetValueWithoutNotify(0);
			slider.highValue = animationClip.length;
			UpdateAnimationEditor();
		}
		
		public void ChangeGameObject(GameObject gameObject)
		{
			model = gameObject;
			Object.DestroyImmediate(editor);
			CreateEditor();
		}
		
		private Slider CreateAnimationClipSlider(float clipLength)
		{
			Slider animationClipSlider = new Slider
			{
				value = 0f,
				highValue = clipLength
			};
			animationClipSlider.RegisterCallback<ChangeEvent<float>>(e =>
			{
				isPlaying = false;
				UpdateAnimationEditor();
			});
			animationClipSlider.Add(new IconButton(() =>
			{
				if (isPlaying)
				{
					isPlaying = false;
					return;
				}
				
				if (SliderAtMax)
				{
					slider.SetValueWithoutNotify(0);
				}
				isPlaying = true;
				EditorCoroutineUtility.StartCoroutine(PlayAnimation(), this);
			}, "play", 16));
			return animationClipSlider;
		}

		private VisualElement CreateEditor()
		{
			if (editor == null)
				editor = UnityEditor.Editor.CreateEditor(model);
			IMGUIContainer editorContainer = new IMGUIContainer(() =>
			{
				GUIStyle bgColor = new GUIStyle();
				if (!editor || !editor.target)
				{
					return;
				}

				if (Application.isPlaying)
				{
					return;
					
				}
				editor.OnInspectorGUI();
				editor.OnPreviewGUI(GUILayoutUtility.GetRect(256, 256), bgColor);
				editor.ReloadPreviewInstances();
			});

			return  editorContainer;
		}

		private IEnumerator PlayAnimation()
		{
			long previousTime = System.DateTime.Now.Ticks;
			
			while (isPlaying && !SliderAtMax)
			{
				float delta = System.DateTime.Now.Ticks - previousTime;
				slider.SetValueWithoutNotify(slider.value + (delta) * 0.0000001f);
				UpdateAnimationEditor();
				
				previousTime = System.DateTime.Now.Ticks;
				yield return null;
			}

			isPlaying = false;
			yield break;
		}

		private bool SliderAtMax => slider.value >= slider.highValue;

		private void UpdateAnimationEditor()
		{
			if (animationClip != null)
			{
				if (!AnimationMode.InAnimationMode())
				{
					AnimationMode.StartAnimationMode();
				}

				if (editor == null)
				{
					return;
				}
				var gameObject = editor.target as GameObject;
				if (gameObject == null)
				{
					return;
				}
				AnimationMode.BeginSampling();
				AnimationMode.SampleAnimationClip(editor.target as GameObject, animationClip, slider.value);
				AnimationMode.EndSampling();
			}
		}
		
		
	}
}