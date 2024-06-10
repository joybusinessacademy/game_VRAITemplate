using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DialogExporter
{
	[Serializable]
    public abstract class AbstractVoiceAPIConfig<T>
    {
	    //public string apiKey;

	    protected abstract void AddCustomHeaders(UnityWebRequest request, T data);

	    public UnityWebRequest RequestVoiceFile(string fileName, T data)
	    {
		    UnityWebRequest webRequest = new UnityWebRequest();
		    webRequest.SetRequestHeader("Content-Type", "application/json");
		    webRequest.url = GetApiUrl();
		    AddCustomHeaders(webRequest, data);

		    string jsonData = JsonUtility.ToJson(data);
		    
		    // this removes empty strings which breaks google
		    jsonData = jsonData.Replace(",\"ssml\":\"\",", String.Empty);
		    jsonData = jsonData.Replace("\"ssml\":\"\",", String.Empty);
		    jsonData = jsonData.Replace(",\"ssml\":\"\"", String.Empty);
		    jsonData = jsonData.Replace("\"ssml\":\"\"", String.Empty);
		    
		    jsonData = jsonData.Replace(",\"text\":\"\",", String.Empty);
		    jsonData = jsonData.Replace("\"text\":\"\",", String.Empty);
		    jsonData = jsonData.Replace(",\"text\":\"\"", String.Empty);
		    jsonData = jsonData.Replace("\"text\":\"\"", String.Empty);
		    
		    byte[] rawData = Encoding.UTF8.GetBytes(jsonData);
		    
		    
		    webRequest.uploadHandler = new UploadHandlerRaw(rawData);
		    webRequest.downloadHandler = new DownloadHandlerBuffer();
		    webRequest.method = "POST";
		    return webRequest;
	    }
	    internal abstract string GetApiUrl();
    }
}
