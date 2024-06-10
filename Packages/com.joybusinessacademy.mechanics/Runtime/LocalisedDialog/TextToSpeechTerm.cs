using System;
using UnityEngine;

namespace DialogExporter
{
	[Serializable]
	public class TextToSpeechTerm
	{
		public string TermName => localizedDialog.Term;
		public string languageCode;


		public string AudioDirectory => exportFolderPath;
		public string AudioDirectoryLanguage => AudioDirectory + "/" + languageCode;
		public string OutputDirectoryPath => Application.dataPath.Replace("/Assets", "") + "/" + AudioDirectoryLanguage;
		public string FileName => localizedDialog.Term.Replace('/', '_') + "." + (audioType == AudioType.OGGVORBIS ? "ogg" : "wav");
		public string FilePath => OutputDirectoryPath + "/" + "Generated_"+ FileName;

		private AudioType audioType = AudioType.OGGVORBIS;
		
		public string ssmlAlternateText;

		public LocalizedDialog localizedDialog;
		
		public string Text => localizedDialog.Dialog;
		public string exportFolderPath;
		//public LanguageVoiceAudioSetting languageSetting;
	}
}