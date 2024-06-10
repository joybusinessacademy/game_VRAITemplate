using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DialogExporter
{
	public static class VoiceExporter
	{
		internal static readonly Dictionary<UnityWebRequest, TextToSpeechTerm> OutstandingRequestList = new Dictionary<UnityWebRequest, TextToSpeechTerm>();
		internal static readonly Dictionary<UnityWebRequest, TextToSpeechTerm> webRequestToTermDataMap = new Dictionary<UnityWebRequest, TextToSpeechTerm>();
	
		internal static KeyValuePair<UnityWebRequest, TextToSpeechTerm> currentWebRequest;

		private static readonly List<TextToSpeechTerm> termToSpeechData = new List<TextToSpeechTerm>();
		
		internal static int awaitingOverdubResultCounter = 0;

		public static int addedExportCount = 0;

		public delegate void FinishedGeneratedClip(AudioClip generatedClip);
		public static FinishedGeneratedClip finishedGenerated;

		public static void Listen()
		{
			if (addedExportCount == 0)
			{
				addedExportCount++;
				EditorApplication.update += UpdateExport;
			}
		}

		public static void Release()
		{
			if (addedExportCount != 0)
			{
				addedExportCount--;
				EditorApplication.update -= UpdateExport;
				EditorUtility.ClearProgressBar();
            }
        }

		public static void CleanWebRequest(UnityWebRequest webRequest)
		{
			if (null == webRequest)
			{
				return;
			}
			if (!webRequestToTermDataMap.ContainsKey(webRequest))
			{
				return;
			}
			webRequestToTermDataMap.Remove(webRequest);
			OutstandingRequestList.Remove(webRequest);
			if (currentWebRequest.Key == webRequest)
			{
				currentWebRequest = new KeyValuePair<UnityWebRequest, TextToSpeechTerm>();
			}
            if (!webRequest.isDone)
            {
                webRequest.Abort();
            }
            webRequest.Dispose();
        }

		/// <summary>
		/// Exports a LocalizedDialog to overdub to be processed
		/// </summary>
		/// <param name="localizedDialogs">The dialogue to be processed</param>
		/// <param name="languageCode">The language that will be exported (English is default)</param>
		public static void Export(List<LocalizedDialog> localizedDialogs, string languageCode = "en")
		{

			Listen();

			if (localizedDialogs == null || localizedDialogs.Count == 0)
			{
				Debug.LogError("Please input a localizedDialog");
				return;
			}
			
			PrepareVoiceToSpeechContent(localizedDialogs, languageCode);
			foreach (TextToSpeechTerm term in termToSpeechData)
			{
				PrepareAndSendCreateVoiceClipRequests(term);
			}
		}
		
		public static void Export(LocalizedDialog localizedDialog, string languageCode = "en")
		{
			Export(new List<LocalizedDialog> {localizedDialog}, languageCode);
		}

		internal static void UpdateExport()
		{
			if(awaitingOverdubResultCounter > 0)
			{
				int processedElements = webRequestToTermDataMap.Count - awaitingOverdubResultCounter;
				float progress = 1f - ((float)(awaitingOverdubResultCounter) / webRequestToTermDataMap.Count);
				EditorUtility.DisplayProgressBar("Converter", "Waiting for overdub processing:" + processedElements + "/" + webRequestToTermDataMap.Count, progress);
			}
			else
			{
				float progress = 1f - ((float)(OutstandingRequestList.Count) / webRequestToTermDataMap.Count);
				int processedElements = webRequestToTermDataMap.Count - OutstandingRequestList.Count;
				EditorUtility.DisplayProgressBar("Converter", "Processing all terms:" + processedElements + "/" + webRequestToTermDataMap.Count, progress);
			}
			if (currentWebRequest.Key != null)
			{
				CheckForRequestCompletion(currentWebRequest.Key, currentWebRequest.Value);
			}
		}

			
		internal static void CheckForRequestCompletion(UnityWebRequest webRequest, TextToSpeechTerm textToSpeech)
		{
			if (webRequest.isDone == false)
			{
				return;
			}

			TextToSpeechTerm termData = webRequestToTermDataMap[webRequest];

			if (webRequest.result == UnityWebRequest.Result.ProtocolError)
			{
				string errorMessage = "";
				GoogleErrorResponseData errorResponse = null;
				try
				{
					errorResponse = JsonUtility.FromJson<GoogleErrorResponseData>(webRequest.downloadHandler.text);
				}
				catch (Exception e)
				{
					errorResponse = new GoogleErrorResponseData
					{
						error =
						{
							message = webRequest.downloadHandler.text ?? e.Message
						}
					};
				}
				errorMessage += webRequest.error;
				errorMessage += termData.TermName;
				errorMessage += errorResponse?.error.message != null ? errorResponse.error.message + " : " + errorResponse.error.reason : errorResponse?.error.reason ?? webRequest.downloadHandler.text;

				 OnVoiceRequestFailed(errorMessage);
			}
			else if (webRequest.result == UnityWebRequest.Result.ConnectionError)
			{
				OnVoiceRequestFailed(termData.TermName + " " + webRequest.error);
			}
			else
			{
				textToSpeech.localizedDialog.textToSpeechAPI.ProcessRequest(webRequest, textToSpeech);
				currentWebRequest = new KeyValuePair<UnityWebRequest, TextToSpeechTerm>();
			}

			Release();
			EditorUtility.ClearProgressBar();
		}

		//private static ITextToSpeechAPI textToSpeech;

		internal static void OnVoiceRequestFailed(string error)
		{
			Debug.LogError(error);
			CleanWebRequest(currentWebRequest.Key);
            currentWebRequest = new KeyValuePair<UnityWebRequest, TextToSpeechTerm>();
		}

		internal static void OnVoiceRequestSuccessful(UnityWebRequest request, TextToSpeechResponse response, LocalizedDialog localizedDialog)
		{
			if (request == null || !webRequestToTermDataMap.ContainsKey(request))
			{
				Debug.Log("Missing Request");
				return;
			}

			TextToSpeechTerm termData = webRequestToTermDataMap[request];

			if (termData != null)
			{
				if (AssetDatabase.IsValidFolder(termData.AudioDirectory) == false)
				{
					CreateFoldersRecursively(termData.AudioDirectory);
				}

				if (AssetDatabase.IsValidFolder(termData.AudioDirectoryLanguage) == false)
				{
					CreateFoldersRecursively(termData.AudioDirectoryLanguage);
				}

				byte[] bytes = null;

				bytes = response.voiceApi == VoiceAPI.GOOGLE ? Convert.FromBase64String(response.audioContent) : response.audioBytes;

				if (bytes.Length > 0)
				{
					string dir = Path.GetDirectoryName(termData.FilePath);
					if (!Directory.Exists(dir))
					{
						if (dir != null)
						{
							Directory.CreateDirectory(dir);
						}
					}
					FileStream fileStream = File.Create(termData.FilePath);
					fileStream.Write(bytes, 0, bytes.Length);
					fileStream.Close();

					AssetDatabase.Refresh();

					string audioClipAssetPath = termData.AudioDirectoryLanguage + "/" + "Generated_" + termData.FileName;
					AudioClip generatedClip = AssetDatabase.LoadAssetAtPath(audioClipAssetPath, typeof(AudioClip)) as AudioClip;
					localizedDialog.GeneratedClip = generatedClip;
					localizedDialog.UseGeneratedClip();
					EditorUtility.SetDirty(localizedDialog);

					finishedGenerated?.Invoke(generatedClip);
				}
			}

			SendNextCreateVoiceClipRequest();
		}

		private static void SendNextCreateVoiceClipRequest()
		{
			if (OutstandingRequestList.Count > 0)
			{
				currentWebRequest = OutstandingRequestList.Last();
				OutstandingRequestList.Remove(currentWebRequest.Key);
				currentWebRequest.Key.SendWebRequest();
			}
			else
			{
				if (awaitingOverdubResultCounter > 0)
				{
					return;
				}

				Release();
				EditorUtility.ClearProgressBar();
				
				AssetDatabase.Refresh();
				AssetDatabase.SaveAssets();
				currentWebRequest = new KeyValuePair<UnityWebRequest, TextToSpeechTerm>();
			}
		}
		
		public static void SendWebRequest(TextToSpeechTerm termToSpeechData)
		{
			string text = termToSpeechData.Text;

			text = text.Replace("[", string.Empty);
			text = text.Replace("]", string.Empty);
			text = text.Replace("(", string.Empty);
			text = text.Replace(")", string.Empty);


			UnityWebRequest webRequest = termToSpeechData.localizedDialog.textToSpeechAPI.CreateWebRequest(termToSpeechData);
			
			
			OutstandingRequestList.Add(webRequest, termToSpeechData);
			webRequestToTermDataMap.Add(webRequest, termToSpeechData);
		}

		internal static void PrepareAndSendCreateVoiceClipRequests(TextToSpeechTerm textToSpeech)
		{

			textToSpeech.localizedDialog.textToSpeechAPI ??= APIManager.CurrentAPI;
			
			SendWebRequest(textToSpeech);

			Listen();

			SendNextCreateVoiceClipRequest();
		}

		/// <summary>
		/// Loads all the text into a list to be processed
		/// </summary>
		/// <param name="localizedDialogs"> A list of all the dialog to be processed</param>
		/// <param name="languageCode">the language that will be processed (default is english)</param>
		public static void PrepareVoiceToSpeechContent(List<LocalizedDialog> localizedDialogs, string languageCode = "en")
		{
			termToSpeechData.Clear();

			foreach (LocalizedDialog localizedDialog in localizedDialogs)
			{
				if (localizedDialog.textToSpeechAPI == null)
				{
					continue;
				}
				
				string assetLocation = AssetDatabase.GetAssetPath(localizedDialog);
				assetLocation = assetLocation.Substring(0, assetLocation.LastIndexOf('/') + 1);
				
				TextToSpeechTerm speechData = new TextToSpeechTerm
				{
					localizedDialog = localizedDialog,
					exportFolderPath = assetLocation,
					languageCode = languageCode
				};
				
				termToSpeechData.Add(speechData);
			}
		}
		
		
		/// <summary>
		/// Loads text into a list to be processed
		/// </summary>
		/// <param name="localizedDialog">The Dialog to be processed</param>
		/// <param name="languageCode">The language that will be processed (default is english)</param>
		public static void PrepareVoiceToSpeechContent(LocalizedDialog localizedDialog, string languageCode = "en")
		{
			PrepareVoiceToSpeechContent(new List<LocalizedDialog> {localizedDialog}, languageCode);	
		}

		public static void CreateFoldersRecursively(string folderName)
		{
			if (AssetDatabase.IsValidFolder(folderName)) 
			{
				return;
			}

			string[] foldersHierarchy = folderName.Split('/');
			string parent = "";
			foreach (string folder in foldersHierarchy) 
			{
				if (!string.IsNullOrEmpty(folder)) 
				{
					if (!AssetDatabase.IsValidFolder(parent + "/" + folder)) 
					{
						AssetDatabase.CreateFolder(parent, folder);
					}

					parent = string.IsNullOrEmpty(parent) ? folder : parent + "/" + folder;
				}
			}

		}
	}


	[Serializable]
	public enum VoiceAPI
	{
		GOOGLE,
		AWS,
		OVERDUB
	}


	[Serializable]
	public class TextToSpeechResponse
	{
		public string audioContent;
		public byte[] audioBytes;
		public VoiceAPI voiceApi;
		public AudioType format;
	}


	[Serializable]
	public class GoogleErrorResponseData
	{
		[Serializable]
		public class ErrorDataObject
		{
			public string message;
			public int code;
			public string reason;
		}

		public ErrorDataObject error = new ErrorDataObject();
	}
}

	
