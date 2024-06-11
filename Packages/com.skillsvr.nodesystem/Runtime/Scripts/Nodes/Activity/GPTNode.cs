using System;
using System.Collections;
using GraphProcessor;
using Newtonsoft.Json.Linq;
using Oculus.VoiceSDK.UX;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Learning/GPT", typeof(SceneGraph)), NodeMenuItem("Learning/GPT", typeof(SubGraph))]
	public class GPTNode : ExecutableNode
	{
		public override string name => "GPT";
		public override string icon => "Dialogue";
		public override string layoutStyle => "GPTNode";

		public override Color color => NodeColours.CharactersAndProps;

		public static GameObject gptInstance = null;
		private GameObject instance;
		protected override void OnStart()
		{
			if (gptInstance == null)
				gptInstance = GameObject.Instantiate(Resources.Load("GPTPrefab")) as GameObject;

			gptInstance.GetComponentInChildren<VoiceActivationButton>().onComplete.AddListener(OnVoiceEnded);
		}

		private IEnumerator RunGPT(string text)
		{
			string threadId = "";
			string assistantId = "";

			// create self introduction
			GPTService.AddMessageToThread(threadId, text, (response) => { });

			string runId = string.Empty;
			GPTService.ThreadRun(threadId, assistantId, (response) => {
				JObject data = JObject.Parse(response);
				runId = data["id"].ToString();
			});

			yield return new WaitUntil(() => !string.IsNullOrEmpty(runId));

			string status = string.Empty;
			// poll run status
			while (!status.Equals("completed"))
			{
				bool responsed = false;
				GPTService.RetrieveRun(threadId, runId, (response) =>
				{
					JObject data = JObject.Parse(response);
					status = data["status"].ToString();
					Debug.Log(status);
					responsed = true;
				});

				yield return new WaitUntil(() => responsed == true);
				yield return new WaitForSeconds(1);
			}

			GPTService.GetMessages(threadId, assistantId, (response) =>
			{
				JObject data = JObject.Parse(response);
				string value = data["data"][0]["content"][0]["text"]["value"].ToString();
			});
		}
		public void OnVoiceEnded(string text)
        {
			
		}


        protected override void OnComplete()
        {
            base.OnComplete();
			gptInstance.GetComponentInChildren<VoiceActivationButton>().onComplete.RemoveListener(OnVoiceEnded);

		}
    }
}