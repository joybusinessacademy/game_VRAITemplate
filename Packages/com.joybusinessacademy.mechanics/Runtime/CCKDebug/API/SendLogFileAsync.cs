using Newtonsoft.Json;
using SkillsVR.EnterpriseCloudSDK.Networking;
using SkillsVR.EnterpriseCloudSDK.Networking.API;
using System;
using System.Collections;
using System.IO;
using System.Text;
using static SkillsVR.Telemetry.Networking.API.PostEditorLog;

namespace SkillsVR.Telemetry.Networking.API
{
    public class SendLogFileAsync : IEnumerator
    {
        public object Current =>null;

        public string LogFilePath { get; protected set; }

        public bool IsDone { get; protected set; }
        public bool? Success { get; protected set; }

        public string Error { get; protected set; }


        IEnumerator action;

        public SendLogFileAsync(string logFilePath)
        {
            IsDone = false;
            Success = null;
            Error = null;
            LogFilePath = logFilePath;
            action = SendLog();
        }

        protected IEnumerator SendLog()
        {
            IsDone = false;
            if (string.IsNullOrWhiteSpace(LogFilePath))
            {
                FailByError("Log file path cannot be null or empty.");
                yield break;
            }

            if (!File.Exists(LogFilePath))
            {
                FailByError($"Log file not exists: {LogFilePath}");
                yield break;
            }

            try
            {
                string[] lines = File.ReadAllLines(LogFilePath);
                string jsonFile = JsonConvert.SerializeObject(lines, Formatting.Indented);

                string logFileName = Path.GetFileNameWithoutExtension(LogFilePath);
                object[] logData = new object[] { logFileName, jsonFile };

                string filename = logData[0] as string ?? "";
                string logContent = logData[1] as string ?? "";

                string binary = StringToBinary(logContent);

                EditorLogData editorLogData = new EditorLogData();
                editorLogData.logContent = binary;

                PostEditorLog postLogData = new PostEditorLog(filename);
                postLogData.data = editorLogData;

                RESTService.Send(postLogData, OnSendRequestSuccess, OnSendRequestFail);
            }
            catch(Exception e)
            {
                FailByError(e.Message);
            }

            while(!IsDone)
            {
                yield return null;
            }
            IsDone = true;
        }

        private void OnSendRequestFail(string error)
        {
            FailByError(error);
        }

        private void OnSendRequestSuccess(AbstractAPI.EmptyResponse obj)
        {
            OnCompleteSuccessful();
        }

        protected string StringToBinary(string input)
        {
            // Convert the string to byte array using UTF-8 encoding
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            // Convert the byte array to binary representation
            string binary = string.Join(" ", Array.ConvertAll(bytes, b => Convert.ToString(b, 2).PadLeft(8, '0')));

            return binary;
        }

        protected void FailByError(string error)
        {
            IsDone = true;
            Error = error;
            Success = false;
        }

        protected void OnCompleteSuccessful()
        {
            IsDone = true;
            Error = null;
            Success = true;
        }
        
        public bool MoveNext()
        {
            return action.MoveNext();
        }

        public void Reset()
        {
            throw new System.NotSupportedException();
        }
    }
}
