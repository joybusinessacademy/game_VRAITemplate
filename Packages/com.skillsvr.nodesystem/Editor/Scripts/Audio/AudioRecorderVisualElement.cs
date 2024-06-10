using DialogExporter;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Audio
{
	public class AudioRecorderVisualElement : VisualElement
	{
		
		public string defaultFileName;
		public string defaultFileLocation;
		private LocalizedDialog localizedDialog;

		public delegate void OnRecordingFinished(AudioClip audioClip);
		public OnRecordingFinished recordingFinished;

		public AudioRecorderVisualElement(LocalizedDialog localizedDialog, string defaultFileName = "New Audio", string defaultFileLocation = "")
		{
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/AudioRecorder"));
			this.defaultFileName = defaultFileName;
			this.defaultFileLocation = defaultFileLocation;
			this.localizedDialog = localizedDialog;
			
			Button playAudio = new();
			playAudio.Add(new Image { image = Resources.Load<Texture>("Icon/Play") });
			playAudio.clicked += () =>
			{
				if (localizedDialog != null)
				{
					localizedDialog.PlayAudio();
				}
			};
			
			DropdownField devices = new()
			{
				choices = AudioRecorder.GetDevices(),
				value = AudioRecorder.DefaultDevice()
			};
			devices.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				AudioRecorder.RecordingDevice = evt.newValue;
				devices.value = AudioRecorder.RecordingDevice;
			});
			
			Button record = new Button();
			Image recordIcon = new Image { image = Resources.Load<Texture>("Icon/Record") };
			
			record.Add(recordIcon);
			record.clicked += () =>
			{
				if (this.localizedDialog == null)
				{
					return;
				}
				
				AudioClip newAudio = AudioRecorder.Record(GetLocationToSave(), this.localizedDialog.Term);

				recordIcon.image = Resources.Load<Texture>(AudioRecorder.IsRecording ? "Icon/Recording" : "Icon/Record");

				localizedDialog.SetCustomAudio(newAudio);

				recordingFinished?.Invoke(newAudio);
			};

			VisualElement container = new VisualElement();
			
			container.Add(playAudio);
			container.Add(record);
			container.Add(devices);
			
			Add(new Label("Record Dialogue"));
			Add(container);
		}

		private string GetLocationToSave()
		{
			string fileLocation = AssetDatabase.GetAssetPath(localizedDialog);
			fileLocation = fileLocation.Substring(0, fileLocation.LastIndexOf('/'));
			fileLocation = Application.dataPath.Replace("Assets", "") + fileLocation;
			fileLocation += "/Unity Recordings/";
			return fileLocation;
		}
	}
}