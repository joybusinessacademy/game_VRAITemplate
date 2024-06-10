using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KBEService : AbstractService<KBEService>
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        service = new GameObject(nameof(KBEService)).AddComponent<KBEService>();
        GameObject.DontDestroyOnLoad(service.gameObject);
    }

    private const string subscribeUrl = "https://ai-coach.createvr.app/api/subscribe";
    private static string ChatURL(int coachId, int threadId) => string.Format("https://ai-coach.createvr.app/api/coaches/{0}/threads/{1}/messages", coachId, threadId);

    public static void Subscribe(int pin, System.Action<SubscribeResponse> onComplete)
    {
        APIService.POST(subscribeUrl, new Dictionary<string, string>(),
           new SubscribeRequestData()
           {
              invite_code = pin
           },
           (response) =>
           {        
               onComplete.Invoke(JsonUtility.FromJson<SubscribeResponse>(response.downloadHandler.text));
           });        
    }

    public static void Chat(int coachId, int threadsId, string content, System.Action<ChatResponse> onComplete)
    {
        APIService.POST(ChatURL(coachId, threadsId), new Dictionary<string, string>(),
           new ChatRequestData()
           {
              message = content
           },
           (response) =>
           {        
               onComplete.Invoke(JsonUtility.FromJson<ChatResponse>(response.downloadHandler.text));
           });        
    }


    [System.Serializable]
    public class SubscribeRequestData
    {
        public int invite_code;
    }

    [System.Serializable]
    public class SubscribeResponse
    {
        public Coach coach;
        public string welcome_audio;
        public int thread_id;

        [System.Serializable]
        public class Coach
        {
            //coach id
            public int id;
            // content in text
            public string intro_message;
            // gender
            public string type;
        }
    }

    [System.Serializable]
    public class ChatRequestData
    {
        public string message;
    }

    [System.Serializable]
    public class ChatResponse
    {
        public string audio_link;
        public string content;
    }
}
