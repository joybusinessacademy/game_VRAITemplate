using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Unity.EditorCoroutines.Editor;

public class APIService : AbstractService<APIService>
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        service = new GameObject(nameof(APIService)).AddComponent<APIService>();
        GameObject.DontDestroyOnLoad(service.gameObject);
    }

    public static void POST(string url, Dictionary<string, string> headers, object data, Action<UnityWebRequest> onComplete, DownloadHandler downloadHandler = null)
    {
        if (data is WWWForm)
        {
            var formPostRequest = UnityWebRequest.Post(url, data as WWWForm);
            headers.ToList().ForEach(pair => { formPostRequest.SetRequestHeader(pair.Key, pair.Value); });

            if (Application.isPlaying)
                service.StartCoroutine(SendRequest(formPostRequest, onComplete));
            else
                EditorCoroutineUtility.StartCoroutineOwnerless(SendRequest(formPostRequest, onComplete));
            return;
        }

        var postRequest = new UnityWebRequest(url, "POST");
        headers.ToList().ForEach(pair => { postRequest.SetRequestHeader(pair.Key, pair.Value); });
        if (data != null)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data as object));
            postRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
        }
        postRequest.SetRequestHeader("Content-Type", "application/json");
        postRequest.downloadHandler = downloadHandler ?? new DownloadHandlerBuffer();

        if (Application.isPlaying)
            service.StartCoroutine(SendRequest(postRequest, onComplete));
        else
            EditorCoroutineUtility.StartCoroutineOwnerless(SendRequest(postRequest, onComplete));
    }

    public static void DELETE(string url, Dictionary<string, string> headers, Action<UnityWebRequest> onComplete)
    {
        var request = new UnityWebRequest(url, "DELETE");
        headers.ToList().ForEach(pair => { request.SetRequestHeader(pair.Key, pair.Value); });
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        if (Application.isPlaying)
            service.StartCoroutine(SendRequest(request, onComplete));
        else
            EditorCoroutineUtility.StartCoroutineOwnerless(SendRequest(request, onComplete));
    }

    public static void GET(string url, Dictionary<string, string> headers, Action<UnityWebRequest> onComplete)
    {
        var request = new UnityWebRequest(url, "GET");
        headers.ToList().ForEach(pair => { request.SetRequestHeader(pair.Key, pair.Value); });
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();

        if (Application.isPlaying)
            service.StartCoroutine(SendRequest(request, onComplete));
        else
            EditorCoroutineUtility.StartCoroutineOwnerless(SendRequest(request, onComplete));
    }

    public static void GETTexture(string url, Action<Texture> onComplete) 
    {
        var request = UnityWebRequestTexture.GetTexture(url);

        if (Application.isPlaying)
        {
            service.StartCoroutine(SendRequest(request, (request) =>
            {
                onComplete(DownloadHandlerTexture.GetContent(request));
            }));
        }
        else
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(SendRequest(request, (request) =>
            {
                onComplete(DownloadHandlerTexture.GetContent(request));
            }));
        }
    }

    public static void GETAudioClip(string url, Action<AudioClip> onComplete)
    {        
        var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);

        if (Application.isPlaying)
        {
            service.StartCoroutine(SendRequest(request, (request) =>
            {
                onComplete(DownloadHandlerAudioClip.GetContent(request));
            }));
        }
        else
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(SendRequest(request, (request) =>
            {
                onComplete(DownloadHandlerAudioClip.GetContent(request));
            }));
        }
    }

    private static IEnumerator SendRequest(UnityWebRequest request, Action<UnityWebRequest> onComplete)
    {
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.error);
        }
        
        onComplete.Invoke(request);
    }

}
