using System;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DialogExporter
{

	/// <summary>
	/// Scriptable object for storing localized Dialog and location of audio file
	/// </summary>
	[CreateAssetMenu]
	public class LocalizedDialog : ScriptableObject
	{
		/// <summary>
		/// The unique name of this term
		/// </summary>
		public string Term => /*Category + "/" +*/ name;

		/// <summary>
		/// Which category it is stored in
		/// </summary>
		public string Category
		{
			get
			{
				#if UNITY_EDITOR
				string folder = AssetDatabase.GetAssetPath(this);
				folder = folder.Substring(0, folder.LastIndexOf('/'));
				folder = folder.Substring(folder.LastIndexOf('/') + 1);
				return folder;
				#else
					return "";
				#endif
			}
		}

		/// <summary>
		/// The dialog as an audio file
		/// </summary>
		[SerializeField]
		private AudioClip customAudioClip;

		[SerializeField] public AudioClip GeneratedClip;
		public bool IsUsingCustom => customAudioClip;
		public bool GeneratedClipNull => GeneratedClip == null;
		public string GeneratedClipName => (GeneratedClip == null) ? null : GeneratedClip.name;

		public AudioClip GetAudioClip => customAudioClip ? customAudioClip : GeneratedClip;
		public void SetCustomAudio(AudioClip audioClip)
		{
#if UNITY_EDITOR
			Undo.RecordObject(this, "ForcedSave");
			EditorUtility.SetDirty(this);
#endif
			
			customAudioClip = audioClip;
		}

		/// <summary>
		/// The dialog as text
		/// </summary>
		public string Dialog
		{
			get
			{
				Source ??= new DefaultLocalisationSource();
				 
				string text = Source.Translation();
				return text != null ? Source.Translation() : "ERROR - No translation found";
			}

            set
            {			
				Source ??= new DefaultLocalisationSource();

				textIsDirty = value != Source.Translation("");
				Source.EditDialog(value);
			}
		}

		public int index;

		public bool TextIsDirty
		{
			get => textIsDirty;
			set => textIsDirty = value;
		}
        bool textIsDirty;



		[SerializeReference]
		public ILocalisationSource Source = new DefaultLocalisationSource();

		public void GenerateAudio()
		{
			//VoiceExporter.Export(this, language);
		}
		
		//[ShowInInspector, FoldoutGroup("Export Settings")]
		public string language = "en";

		//[ShowInInspector, SerializeReference, FoldoutGroup("Export Settings")]
		public ITextToSpeechAPI textToSpeechAPI;
		

		public void UseGeneratedClip()
		{
			customAudioClip = null;
		}

// editor Tools
#if UNITY_EDITOR

		public void PlayAudio()
		{
			if (GetAudioClip)
			{
				PlayClip(GetAudioClip);
			}
		}
		
		public static void PlayClip(AudioClip clip, int startSample = 0, bool loop = false)
		{
			Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
			
			Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
			MethodInfo method = audioUtilClass.GetMethod(
				"PlayPreviewClip",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new [] { typeof(AudioClip), typeof(int), typeof(bool) },
				null
			);
			
			method?.Invoke(null, new object[] { clip, startSample, loop } );
		}
		
		public static void StopAudio() 
		{
			Assembly unityEditorAssembly = typeof(AudioImporter).Assembly;
			Type audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil");
			
			MethodInfo method = audioUtilClass.GetMethod(
				"StopAllClips",
				BindingFlags.Static | BindingFlags.Public,
				null,
				new Type[]{},
				null
			);
			
			method?.Invoke(null, new object[] {});
		}
#endif

	}
}





