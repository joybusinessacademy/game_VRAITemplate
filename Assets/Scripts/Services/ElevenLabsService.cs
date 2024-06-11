using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.Networking;

public class ElevenLabsService : AbstractService<ElevenLabsService>
{ 

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        service = new GameObject(nameof(ElevenLabsService)).AddComponent<ElevenLabsService>();
        GameObject.DontDestroyOnLoad(service.gameObject);
    }
        
    private const string url = "https://api.elevenlabs.io/v1/text-to-speech/ZY37LYw0WtCyedeNw2EV";
    //"https://api.elevenlabs.io/v1/text-to-speech/GXv29rfMJdSYxJgUyfZr";
    private const string apiKey = "85afa703f97ce1537d14636323cf35ea";
    //"6497d351c68f906bee01c111a4e90aa1";

    // lady XfNU2rGpBa01ckF309OY
    // male ZY37LYw0WtCyedeNw2EV

    public void Generate(string content)
    {
        if (!Application.isPlaying)
        {
            Debug.LogError("works only if application is in playmode");
            return;   
        }
        
        var queue = new QueuedRequestParameter();
        queue.text = content;
        queue.onComplete =  (response) => {};
        queue.filePath = Path.Combine(Application.dataPath, System.DateTime.Now.Ticks.ToString() + ".mp3");
        QueuedRequestParameter.queue.Add(queue);
    }

    public static void Request(QueuedRequestParameter queue, bool ignoreFilePath = false)
    {
        if (!ignoreFilePath)
            queue.filePath = queue.filePath ?? Path.Combine(Application.temporaryCachePath, System.DateTime.Now.Ticks.ToString() + ".mp3");

        APIService.POST(url, RequestData.header, new RequestData() { text = queue.text }, (response) =>
        {
            QueuedRequestParameter.concurrentRequest--;
            if (Application.isPlaying)
                service.StartCoroutine(LoadTmpClip(queue.filePath, queue.onComplete));
            else
                EditorCoroutineUtility.StartCoroutineOwnerless(LoadTmpClip(queue.filePath, queue.onComplete));
        }, new DownloadHandlerFile(queue.filePath));
    }

    internal static IEnumerator LoadTmpClip(string filePath, System.Action<UnityWebRequest> onComplete)
    {
        var clipRequest = UnityWebRequestMultimedia.GetAudioClip(filePath, AudioType.MPEG);
        yield return clipRequest.SendWebRequest();
        onComplete(clipRequest);
    }

    public static void QueueRequest(string @text, System.Action<UnityWebRequest> @onComplete, string targetFilePath = null)
    {
        var queued = new QueuedRequestParameter() { text = @text, onComplete = @onComplete, filePath = targetFilePath };
        QueuedRequestParameter.queue.Add(queued);
    }

    private void Update()
    {
        if (QueuedRequestParameter.concurrentRequest >= 2)
            return;

        var queue = QueuedRequestParameter.queue.FirstOrDefault();
        if (queue != null)
        {
            QueuedRequestParameter.queue.Remove(queue);
            QueuedRequestParameter.concurrentRequest++;
            Request(queue);
        }
    }

    public class QueuedRequestParameter
    {
        public static int concurrentRequest = 0;
        public static List<QueuedRequestParameter> queue = new List<QueuedRequestParameter>();
        public string filePath = null;

        public string text;
        public System.Action<UnityWebRequest> onComplete;
    }

    [System.Serializable]
    protected class RequestData
    {
        public string text;
        public string model_id = "eleven_monolingual_v1";
        public static Dictionary<string, string> header = new Dictionary<string, string>() { { "xi-api-key", ElevenLabsService.apiKey } };

        public VoiceSettings voice_settings = new VoiceSettings();

        [System.Serializable]
        public class VoiceSettings
        {
            public float stability = 0.5f;
            public float similarity_boost = 0.5f;
        }
    }
}
