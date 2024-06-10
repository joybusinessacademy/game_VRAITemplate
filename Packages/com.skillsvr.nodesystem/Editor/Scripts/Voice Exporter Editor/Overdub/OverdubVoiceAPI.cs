using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using System;
using System.Text;
using System.Linq;

namespace DialogExporter
{

	[Serializable]
	public class OverdubVoiceAPI : AbstractVoiceAPIConfig<OverdubTextToSpeechRequestData>, ITextToSpeechAPI
    {

	    public OverdubVoiceAPI()
	    {
			LoadJson();
			string voiceName = EditorPrefs.GetString("OverdubVoiceId", "");
			voice = voices.voices.Find(v => v.name == voiceName) ?? voices.voices[0];
	    }
	    
	    protected override void AddCustomHeaders(UnityWebRequest request, OverdubTextToSpeechRequestData data)
        {
	        string authenticationToken = $"Bearer {OverdubApiKey}";
            request.SetRequestHeader("Authorization", authenticationToken);
        }

        internal override string GetApiUrl()
        {
            return "https://descriptapi.com/v1/overdub/generate_async";
        }


        public UnityWebRequest RequestVoiceFileComplete(string asyncOperationId)
        {
            UnityWebRequest webRequest = new UnityWebRequest();
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.url = GetApiUrl() + "/" + asyncOperationId;
            string authenticationToken = $"Bearer {OverdubApiKey}";
            webRequest.SetRequestHeader("Authorization", authenticationToken);

            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.method = "GET";
            return webRequest;
        }

        public static IEnumerator ProcessOverdubAsyncRequests(UnityWebRequest originalRequest, OverdubAsyncSpeechProcessingResponse response, LocalizedDialog localizedDialog)
        {
            float proc = 0.0f;
            EditorUtility.DisplayProgressBar("Processing Overdub...", "", proc);
	        string asyncTaskId = response.id;
	        TextToSpeechTerm term = VoiceExporter.webRequestToTermDataMap[originalRequest];
	        double lastUpdateTime = EditorApplication.timeSinceStartup;
	        if (string.IsNullOrEmpty(asyncTaskId) == false)
	        {
		        VoiceExporter.awaitingOverdubResultCounter++;
		        bool isDone = false;
		        while (isDone == false)
		        {
			        if (lastUpdateTime + 1.5f < EditorApplication.timeSinceStartup)
			        {
						proc += 0.1f;
						proc = Mathf.Max(proc, 0.7f);

                        EditorUtility.DisplayProgressBar("Processing Overdub...", "", proc);
                        lastUpdateTime = EditorApplication.timeSinceStartup;
				        UnityWebRequest webRequest = OverdubApiConfig.RequestVoiceFileComplete(asyncTaskId);
				        yield return webRequest.SendWebRequest();
				        if (webRequest.isHttpError || webRequest.isNetworkError)
				        {
					        Debug.LogErrorFormat("Error occured while processing overdub voice clip for term '{0}' | Code: {1} | Error: {2}", term.TermName, webRequest.responseCode, webRequest.downloadHandler.text);
					        if (webRequest.isNetworkError)
					        {
						        isDone = true;
					        }
				        }
				        else
				        {
					        response = JsonUtility.FromJson<OverdubAsyncSpeechProcessingResponse>(webRequest.downloadHandler.text);
					        if (response.state == "done")
					        {
						        webRequest = new UnityWebRequest(response.url);
						        webRequest.downloadHandler = new DownloadHandlerBuffer();
						        yield return webRequest.SendWebRequest();
						        if (webRequest.isHttpError || webRequest.isNetworkError)
						        {
							        Debug.LogErrorFormat("Error occured while processing overdub voice clip for term '{0}' | Code: {1} | Error: {2}", term.TermName, webRequest.responseCode, webRequest.downloadHandler.text);
							        if (webRequest.isNetworkError)
							        {
								        isDone = true;
							        }
						        }
						        else
						        {
							        isDone = true;
							        VoiceExporter.awaitingOverdubResultCounter--;
							        TextToSpeechResponse textToSpeechResponse = new TextToSpeechResponse
							        {
								        voiceApi = VoiceAPI.OVERDUB,
								        format = AudioType.WAV,
								        audioBytes = (webRequest.downloadHandler as DownloadHandlerBuffer)?.data
							        };
                                    EditorUtility.DisplayProgressBar("Processing Overdub Voice...", "", 0.8f);
                                    VoiceExporter.OnVoiceRequestSuccessful(originalRequest, textToSpeechResponse, localizedDialog);
						        }
					        }
				        }
			        }
		        }
	        }
	        else
	        {
		        Debug.LogError("Invalid overdub async task id for ");
	        }
            VoiceExporter.CleanWebRequest(originalRequest);
            EditorUtility.ClearProgressBar();
        }

        public static string OverdubVoiceId;
        
        public static string OverdubApiKey
        {
	        get => Encoding.UTF8.GetString(Convert.FromBase64String(Resources.Load<TextAsset>("vrcoreodub").text));
	        set => EditorPrefs.SetString("OverdubApiKey", value);
        }

        internal static readonly OverdubVoiceAPI OverdubApiConfig = new OverdubVoiceAPI();

        public UnityWebRequest CreateWebRequest(TextToSpeechTerm term)
        {	        
	        OverdubTextToSpeechRequestData data = new OverdubTextToSpeechRequestData
	        {
		        text = term.Text,
		        voice_id = ((OverdubVoiceAPI)term.localizedDialog.textToSpeechAPI).voice.id
	        };
	        return OverdubApiConfig.RequestVoiceFile(term.TermName, data);
        }

        public void ProcessRequest(UnityWebRequest webRequest, TextToSpeechTerm termData)
        {
	        OverdubAsyncSpeechProcessingResponse response = JsonUtility.FromJson<OverdubAsyncSpeechProcessingResponse>(webRequest.downloadHandler.text);

	        EditorCoroutineUtility.StartCoroutineOwnerless(ProcessOverdubAsyncRequests(webRequest, response, termData.localizedDialog));
        }

        public Voice voice;
        
        private OverdubVoices voices = new OverdubVoices();
        private Dictionary<string, Voice> DropdownVoices = new ();
        
        
        public void LoadJson()
        {
			string json = "";
			try
			{
				var overdubAsset = Resources.Load("Overdub / OverdubPeople.json");
				string textAssetPath = AssetDatabase.GetAssetPath(overdubAsset.GetInstanceID());

				StreamReader streamReader = new StreamReader(textAssetPath);
				//StreamReader streamReader = new StreamReader(Path.GetFullPath("Packages/com.joybusinessacademy.nodesystem/Editor/Scripts/Voice Exporter Editor/Overdub/OverdubPeople.json"));
				json = streamReader.ReadToEnd();
			}
			catch(Exception e)
			{
				var guid = AssetDatabase.FindAssets("OverdubPeople t:TextAsset").FirstOrDefault();

				if (string.IsNullOrWhiteSpace(guid))
				{
					Debug.LogError("Load OverdubPeople Json Fail: File Not Found.");
					return;
				}
				var path = AssetDatabase.GUIDToAssetPath(guid);
				var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				json = null == textAsset ? json : textAsset.text;
			}
	        
	        voices = JsonUtility.FromJson<OverdubVoices>("{\"voices\":" + json + "}");

	        Dictionary<string, Voice> DropdownVoicesTest = new();
	        foreach (Voice voice in voices.voices)
	        {
		        DropdownVoicesTest.Add(voice.name, voice);
	        }

	        DropdownVoices = DropdownVoicesTest;
        }

        [Serializable]
        public class Voice
        {
	        public string id;
	        public string status;
	        public bool is_public;
	        public string name;
        }
        
        [Serializable]
        public class OverdubVoices
        {
	        public List<Voice> voices = new List<Voice>();
        }

        public VisualElement VisualElement()
        {
	        VisualElement container = new();

	        DropdownField dropdownField = new();
	        foreach (Voice voice in voices.voices)
	        {
		        dropdownField.choices.Add(voice.name);
	        }

	        dropdownField.value = voice.name;
	        dropdownField.RegisterValueChangedCallback(evt =>
	        {
		        EditorPrefs.SetString("OverdubVoiceId", evt.newValue);
		        voice = voices.voices.Find(v => v.name == evt.newValue);
	        });
	        
	        
	        container.Add(dropdownField);
	        return container;
        }
    }
	
    [Serializable]
    public class OverdubTextToSpeechRequestData
    {
        public string text;
        public string voice_id;
    }

    [Serializable]
    public class OverdubAsyncSpeechProcessingResponse
    {
        public string id;
        public string state;
        public string url;
    }
}
