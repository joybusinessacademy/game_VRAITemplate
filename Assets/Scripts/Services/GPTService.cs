using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class GPTService : AbstractService<GPTService>
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        service = new GameObject(nameof(GPTService)).AddComponent<GPTService>();
        GameObject.DontDestroyOnLoad(service.gameObject);
    }
    private const string base64x = "c2stZDlVZjBXNklwMk9rVW5NeDBtTjJUM0JsYmtGSnNSWE1MdUdMZ0dQdmE1MzN1SDlO";
    private static string apiKey
    {
        get
        {
            byte[] data = Convert.FromBase64String(base64x);
            return System.Text.Encoding.UTF8.GetString(data);
        }
    }

    public static void CreateAssistant(string @instructions, string vectorStoreId, System.Action<string> onComplete)
    {
        var vectorStore = vectorStoreId == "None" ? 
            new ToolResources.VectorStoreBody() :
            new ToolResources.VectorStoreBody() { vector_store_ids = new List<string>{ vectorStoreId } };
        APIService.POST("https://api.openai.com/v1/assistants", BaseAssistantRequest.header,
           new CreateAssistantRequest() { instructions = @instructions, tool_resources = new ToolResources() { file_search = vectorStore } },
           (response) =>
           {
               //Debug.Log("CreateAssistant " + response.downloadHandler.text);
               onComplete.Invoke(response.downloadHandler.text);
           }); ;
    }

    public static void CreateThread(System.Action<string> onComplete)
    {
        APIService.POST("https://api.openai.com/v1/threads", BaseAssistantRequest.header,
           new CreateThreadRequest() { },
           (response) =>
           {
               //Debug.Log("CreateThread " + response.downloadHandler.text);
               onComplete.Invoke(response.downloadHandler.text);
           });
    }

    public static void AddMessageToThread(string threadId, string message, System.Action<string> onComplete)
    {
        APIService.POST(string.Format("https://api.openai.com/v1/threads/{0}/messages", threadId), BaseAssistantRequest.header,
           new ThreadMessageRequestData() { content = message },
           (response) =>
           {
               //Debug.Log("AddMessageToThread " + response.downloadHandler.text);
               onComplete.Invoke(response.downloadHandler.text);
           });
    }

    public static void ThreadRun(string threadId, string assistantId, string @instructions = null, Action<string> onComplete = null)
    {

        APIService.POST(string.Format("https://api.openai.com/v1/threads/{0}/runs", threadId), BaseAssistantRequest.header,
           new AssistantThreadRunRequest() { assistant_id = assistantId, instructions = @instructions },
           (response) =>
           {
               //Debug.Log("ThreadRun " + response.downloadHandler.text);
               onComplete.Invoke(response.downloadHandler.text);
           });
    }

    public static void RetrieveRun(string threadId, string runId, System.Action<string> onComplete)
    {
        APIService.GET(string.Format("https://api.openai.com/v1/threads/{0}/runs/{1}", threadId, runId), BaseAssistantRequest.header,
           (response) =>
           {
               //Debug.Log("RetrieveRun " + response.downloadHandler.text);
               onComplete.Invoke(response.downloadHandler.text);
           });
    }

    public static void GetMessages(string threadId, string assistantId, System.Action<string> onComplete)
    {
        APIService.GET(string.Format("https://api.openai.com/v1/threads/{0}/messages", threadId), BaseAssistantRequest.header,
           (response) =>
           {
               //Debug.Log("GetMessages " + response.downloadHandler.text);
               onComplete.Invoke(response.downloadHandler.text);
           });
    }

    public static void DeleteAssistant(string assistantId, System.Action<dynamic> onComplete)
    {
        APIService.DELETE(string.Format("https://api.openai.com/v1/assistants/{0}", assistantId), BaseAssistantRequest.header,
          (response) =>
          {
              onComplete.Invoke(response.downloadHandler.text);
          });
    }

    public static void CreateVectorStore(string @name, System.Action<string> onComplete, params string[] ids)
    {
        APIService.POST(string.Format("https://api.openai.com/v1/vector_stores"), BaseAssistantRequest.header,
            new CreateVectorStoreRequest() { name = name, file_ids = ids.ToList() },
           (response) =>
           {
               //Debug.Log("CreateVectorStore " + response.downloadHandler.text);
               onComplete.Invoke(response.downloadHandler.text);
           });
    }

    public static void GetAllVectorStore(System.Action<string> onComplete)
    {
        APIService.GET(string.Format("https://api.openai.com/v1/vector_stores"), BaseAssistantRequest.header,
        (response) =>
        {
            //Debug.Log("GetAllVectorStore " + response.downloadHandler.text);
            onComplete.Invoke(response.downloadHandler.text);
        });
    }

    public static void UploadFile(string path, System.Action<string> onComplete)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", File.ReadAllBytes(path), Path.GetFileName(path));
        form.AddField("purpose", "assistants");
        APIService.POST(string.Format("https://api.openai.com/v1/files"), BaseRequest.header, form,
          (response) =>
          {
              Debug.Log(response.downloadHandler.text);
              onComplete.Invoke(response.downloadHandler.text);
          });
    }

    public static void Completion<T>(string userMessage, System.Action<T> onComplete)
    {
        APIService.POST("https://api.openai.com/v1/chat/completions", CompletionRequestData.header,
           new CompletionRequestData()
           {
               messages = new List<CompletionRequestData.Message>() {
                    new CompletionRequestData.Message()
                            { role = "user", content = userMessage }
                   }
           },
           (response) =>
           {
               Debug.Log(response.downloadHandler.text);
               onComplete.Invoke(ParseToGPTBuilderContent<T>(response.downloadHandler.text));
           });
    }

    public static T ParseToGPTBuilderContent<T>(string json)
    {
        var cleanedResponse = RemoveNewlinesFromJson(json);
        var gptResponse = JsonUtility.FromJson<RequestResponse<T>>(cleanedResponse);

        return gptResponse.choices.First().message.content;
    }

    public static string RemoveNewlinesFromJson(string json)
    {
        // Use regular expression to remove newline characters from JSON
        string cleanedJson = Regex.Replace(json, @"\\n|\\r|\\n\r", "")
            .Replace("\\\"", "\"")
            .Replace("}\"", "}")
            .Replace("\"{", "{")
            .Replace("]\"", "]")
            .Replace("\"[", "[");

        return cleanedJson;
    }

    [System.Serializable]
    protected class BaseRequest
    {
        public static Dictionary<string, string> header = new Dictionary<string, string>() { { "Authorization", string.Format("Bearer {0}", apiKey) } };
    }

    [System.Serializable]
    protected class BaseAssistantRequest
    {
        public static Dictionary<string, string> header = new Dictionary<string, string>() { { "Authorization", string.Format("Bearer {0}", apiKey) }, { "OpenAI-Beta", "assistants=v2" } };
    }

    [System.Serializable]
    protected class CreateVectorStoreRequest : BaseRequest
    {
        public List<string> file_ids;
        public string name;
    }

    [System.Serializable]
    protected class CreateThreadRequest : BaseAssistantRequest
    {

    }



    [System.Serializable]
    protected class CreateAssistantRequest : BaseAssistantRequest
    {
        public string instructions;
        public string model = "gpt-4o";

        public ToolResources tool_resources = new ToolResources();
        public List<Tools> tools = new List<Tools>() { { new Tools() { type = "file_search" }  } };
    }

    [System.Serializable]
    public class ToolResources
    {
        public CodeInterpreterBody code_interpreter = new CodeInterpreterBody() ;
        public VectorStoreBody file_search = new VectorStoreBody();
        [System.Serializable]
        public class CodeInterpreterBody
        {
            public List<string> file_ids;
        }

        [System.Serializable]
        public class VectorStoreBody
        {
            public List<string> vector_store_ids;
        }
    }

    [System.Serializable]
    public class Tools
    {
        public string type;
    }

    [System.Serializable]
    protected class AssistantThreadRunRequest : BaseAssistantRequest
    {
        public string assistant_id; 
        public string instructions = null;
        public string model = "gpt-4o";
    }

    [System.Serializable]
    protected class ThreadMessageRequestData : BaseRequest
    {
        public string role = "user";
        public string content;
    }

    [System.Serializable]
    protected class CompletionRequestData : BaseRequest
    {
        public string model = "gpt-3.5-turbo";
        public List<Message> messages = new List<Message>();
        public float temperature = 0.7f;

        [System.Serializable]
        public class Message
        {
            public string role;
            public string content;
        }
    }

    [System.Serializable]
    public class RequestResponse<T>
    {
        public List<ChoicesBody> choices = new List<ChoicesBody>();
        [System.Serializable]
        public class ChoicesBody
        {
            public int index = -1;
            public MessgeBody<T> message = new MessgeBody<T>();
        }

        [System.Serializable]
        public class MessgeBody<T>
        {
            public string role;
            public T content;
        }
    }


}
