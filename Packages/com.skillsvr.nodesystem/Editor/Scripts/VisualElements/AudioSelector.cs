using System;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;

namespace Scripts.VisualElements
{
	public class AudioSelector : DisposableVisualElement
	{
		internal AudioClip audioClip;
		public AudioPlayButton playPauseButton;
		public AssetDropdown<AudioClip> audioDropdown;

		private Action<float> whilePlayingAction;

		public AudioSelector(AssetDropdown<AudioClip>.ChangeDropdown dropdownCallback, AudioClip audioClip)
		{
			this.audioClip = audioClip;
			VisualElement audioContainer = new VisualElement();
			
			styleSheets.Add(Resources.Load<StyleSheet>("AudioSelector"));
			
			playPauseButton = new AudioPlayButton(audioClip)
			{
				name = "play-pause-button"
			};

			audioDropdown = new AssetDropdown<AudioClip>(evt =>
			{
				playPauseButton.SetAudioFile(evt);
				dropdownCallback.Invoke(evt);
				this.audioClip = evt;
			}, audioClip);
			
			audioContainer.Add(audioDropdown);
			
			audioContainer.Add(playPauseButton);
			
			VisualElement volumeContainer = new()
			{
				name = "volume-container"
			};
			VisualElement volumeBar = new()
			{
				name = "volume-bar"
			};
			volumeContainer.Add(volumeBar);

			whilePlayingAction = time =>
			{
				if (this.audioClip == null)
				{
					volumeBar.style.width = 0;
				}
				else
				{
					volumeBar.style.width = new StyleLength(new Length(time / this.audioClip.length * 100, LengthUnit.Percent));
				}
			};

			playPauseButton.whilePlaying += whilePlayingAction;

			Add(audioContainer);
			Add(volumeContainer);
		}

		public override void Dispose()
		{
			this.audioClip = null;
			audioDropdown = null;

			if(playPauseButton != null)
				playPauseButton.whilePlaying -= whilePlayingAction;
		}
	}
}