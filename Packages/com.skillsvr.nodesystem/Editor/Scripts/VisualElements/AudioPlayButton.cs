using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;
using Unity.EditorCoroutines.Editor;

	
namespace Scripts.VisualElements
{
	public class AudioPlayButton : Button
	{
		private AudioClip audioClip;
		private Texture2D playIcon = Resources.Load<Texture2D>("Icon/Play");
		private Texture2D pauseIcon = Resources.Load<Texture2D>("Icon/Pause");
		private Image playPauseIcon;
		
		public AudioPlayButton(AudioClip audioClip)
		{
			Initialise();

			SetAudioFile(audioClip);
		}

		public AudioPlayButton()
		{
			Initialise();
		}
		
		private void Initialise()
		{
			// if images are not loaded
			playIcon ??= new Texture2D(16, 16);
			pauseIcon ??= new Texture2D(16, 16);
			
			playPauseIcon = new Image
			{
				image = playIcon
			};
			Add(playPauseIcon);

			clicked += ButtonClicked;
		}

		public void SetAudioFile(AudioClip newAudioClip)
		{
			StopClip();
			// This audio clip is allowed to be null
			audioClip = newAudioClip;
			SetEnabled(audioClip != null);
		}
		
		public void ButtonClicked()
		{
			if (IsClipPlaying())
			{
				StopClip();
			}
			else
			{
				PlayClip();
			}
		}

		private void PlayClip()
		{
			if (audioClip == null)
			{
				return;
			}
			if (currentPlayingCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(currentPlayingCoroutine);
			}
			currentPlayingCoroutine = EditorCoroutineUtility.StartCoroutine(IsPlayingCoroutine(), this);
			playPauseIcon.image = pauseIcon;
			PlayClipReflection(audioClip);
		}

		public Action<float> whilePlaying = delegate {  };
		public Action onEnded = delegate { };
		
		public EditorCoroutine currentPlayingCoroutine;

		public IEnumerator IsPlayingCoroutine()
		{
			while (IsClipPlaying())
			{
				yield return new WaitForSeconds(0.1f);
				if (audioClip == null)
				{
					break;
				}
				whilePlaying?.Invoke(GetPreviewPosition(audioClip));
			}

			StopClip();
		}

		private void StopClip()
		{
			if (currentPlayingCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(currentPlayingCoroutine);
			}
			
			playPauseIcon.image = playIcon;
			onEnded?.Invoke();
			whilePlaying?.Invoke(0);
			StopAllAudio();
		}

		public static bool IsClipPlaying()
		{
			object isClipPlaying = IsPreviewClipPlayingMethod?.Invoke(typeof(bool), new object[] {});

			return isClipPlaying != null && (bool)isClipPlaying;
		}
		
		public static float GetPreviewPosition(AudioClip audioClip)
		{
			object position = GetPreviewClipPositionMethod?.Invoke(typeof(float), new object[] {});

			return position != null ? (float)position : 0;
		}
		
		public static void StopAllAudio() 
		{
			StopAllPreviewClipsMethod?.Invoke(null, new object[] {});
		}
		
		public static void PlayClipReflection(AudioClip clip, int startSample = 0, bool loop = false)
		{
			PlayPreviewClipMethod?.Invoke(null, new object[] { clip, startSample, loop } );
		}

		#region Reflection for Audio Util
		private static MethodInfo playPreviewClipMethod;
		private static MethodInfo PlayPreviewClipMethod => playPreviewClipMethod ??= AudioUtilClass?.GetMethod("PlayPreviewClip");

		private static MethodInfo getPreviewClipPositionMethod;
		private static MethodInfo GetPreviewClipPositionMethod => getPreviewClipPositionMethod ??= AudioUtilClass?.GetMethod("GetPreviewClipPosition");
		
		private static MethodInfo isPreviewClipPlayingMethod;
		private static MethodInfo IsPreviewClipPlayingMethod => isPreviewClipPlayingMethod ??= AudioUtilClass?.GetMethod("IsPreviewClipPlaying");

		
		private static MethodInfo stopAllPreviewClipsMethod;
		private static MethodInfo StopAllPreviewClipsMethod => stopAllPreviewClipsMethod ??= AudioUtilClass?.GetMethod("StopAllPreviewClips");

		private static Type audioUtilClass ;
		public static Type AudioUtilClass => audioUtilClass ??= typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

		#endregion
	}
}
