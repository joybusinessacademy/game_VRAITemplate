using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class FileService : AbstractService<FileService>
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        service = new GameObject(nameof(FileService)).AddComponent<FileService>();
        GameObject.DontDestroyOnLoad(service.gameObject);
    }

    private Dictionary<string, object> loadedFiles = new Dictionary<string, object>();

    private IEnumerator LoadInternal<T>(string url, System.Action<T> onComplete) where T : Object
    {
        if (!File.Exists(url))
        {
            yield break;
        }

        if (loadedFiles.ContainsKey(url))
        {
            onComplete.Invoke(loadedFiles[url] as T);
            yield break;
        }

        if (File.Exists(url))
        {            
            var receivedBytes = File.ReadAllBytes(url);
            float[] samples = new float[receivedBytes.Length / 4]; //size of a float is 4 bytes

            System.Buffer.BlockCopy(receivedBytes, 0, samples, 0, receivedBytes.Length);

            int channels = 1; //Assuming audio is mono because microphone input usually is
            int sampleRate = 44100; //Assuming your samplerate is 44100 or change to 48000 or whatever is appropriate

            AudioClip clip = AudioClip.Create(Path.GetFileName(url), samples.Length, channels, sampleRate, false);
            clip.SetData(samples, 0);

            onComplete.Invoke(clip as T);
            loadedFiles.Add(url, clip);
            yield break;
            
        }

        switch (typeof(T).Name)
        {
            case nameof(AudioClip):
                System.Uri uri = new System.Uri(url);
                var areq = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG);
                yield return areq.SendWebRequest();
                
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(areq);
                onComplete.Invoke(myClip as T);
                loadedFiles.Add(url, myClip);
                break;

            case nameof(Texture2D):
            case nameof(Texture):
                var req = UnityWebRequestTexture.GetTexture(url);
                yield return req.SendWebRequest();
                var texture = ((DownloadHandlerTexture)req.downloadHandler).texture;
                onComplete.Invoke(texture as T);
                loadedFiles.Add(url, texture);
                break;

            case nameof(TextAsset):
                var treq = UnityWebRequest.Get(url);
                yield return treq.SendWebRequest();
                var textAsset = new TextAsset(treq.downloadHandler.text);
                onComplete.Invoke(textAsset as T);
                loadedFiles.Add(url, textAsset);
                break;

        }
    }

    public static void Load<T>(string url, System.Action<T> onComplete) where T : Object
    {
        service.StartCoroutine(service.LoadInternal(url, onComplete));
    }
}
