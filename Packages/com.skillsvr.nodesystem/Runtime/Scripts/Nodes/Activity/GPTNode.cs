using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DialogExporter;
using GraphProcessor;
using Newtonsoft.Json.Linq;
using Oculus.VoiceSDK.UX;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Learning/GPT", typeof(SceneGraph)), NodeMenuItem("Learning/GPT", typeof(SubGraph))]
	public class GPTNode : ExecutableNode
	{
		[SerializeField] public PropGUID<IPropAudioSource> dialoguePosition;

		public override string name => "GPT";
		public override string icon => "Dialogue";
		public override string layoutStyle => "GPTNode";

		public override Color color => NodeColours.CharactersAndProps;

		public static GameObject gptInstance = null;
		private GameObject instance;

		private AudioSource propManager = null;
		private static Dictionary<BaseGraph, string> graphThreadIdPair = new Dictionary<BaseGraph, string>();

		public List<LocalizedDialog> fillerDialogs = new List<LocalizedDialog>();


		protected override void OnStart()
		{
			if (!graphThreadIdPair.ContainsKey(Graph))
			{
				GPTService.CreateThread((response) => {
					JObject data = JObject.Parse(response);
					graphThreadIdPair.Add(Graph, data["id"].ToString());
				});
			}

			propManager = GameObject.Find("Prop Manager").GetComponent<AudioSource>();

			if (gptInstance == null)
				gptInstance = GameObject.Instantiate(Resources.Load("GPTPrefab")) as GameObject;

			gptInstance.GetComponentInChildren<VoiceActivationButton>().onComplete.AddListener(OnVoiceEnded);
		}


		private IEnumerator RunGPT(string text)
		{
			var targetClip = fillerDialogs[(int)UnityEngine.Random.Range(0, fillerDialogs.Count)].GetAudioClip;
			var sceneAudio = PropManager.GetProp<IPropAudioSource>(dialoguePosition);
			if (sceneAudio != null)
			{
				sceneAudio.PlayAudio(targetClip);
			}

			var fillerEndTime = Time.time + targetClip.length;

			string threadId = graphThreadIdPair[Graph];
			string assistantId = (graph as SceneGraph).assistantId;
			string assistantInstruction = (graph as SceneGraph).assistantInstruction;

			GPTService.AddMessageToThread(threadId, text + "\n KEEP IT SHORT and conversation format, sometimes reiterate what the subject of the sentence and make sure you redirect it to the topic. follow up with related next question. remove all special characters. also limit to max senteces to 5", (response) => { });

			string runId = string.Empty;
			GPTService.ThreadRun(threadId, assistantId, assistantInstruction,
				(response) => {
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
					responsed = true;
				});

				yield return new WaitUntil(() => responsed == true);
				yield return new WaitForSeconds(1);
			}

			GPTService.GetMessages(threadId, assistantId, (response) =>
			{
				JObject data = JObject.Parse(response);
				string text = data["data"][0]["content"][0]["text"]["value"].ToString();

				var queue = new ElevenLabsService.QueuedRequestParameter();
				queue.text = text;
				queue.onComplete = (response) => {

					float delta = Math.Max(0, (fillerEndTime - Time.time));

					WaitMonoBehaviour.Process((float)delta, () =>
					{
						AudioClip myClip = DownloadHandlerAudioClip.GetContent(response);
						myClip.name = Path.GetFileName(queue.filePath);
						sceneAudio.PlayAudio(myClip);

						WaitMonoBehaviour.Process(myClip.length + 0.5f, () =>
						{
							gptInstance.GetComponentsInChildren<CanvasGroup>().ToList().Find(k => k.name.Equals("Canvas")).enabled = true;
							gptInstance.GetComponentsInChildren<CanvasGroup>().ToList().Find(k => k.name.Equals("Canvas")).interactable = true;
						});
					});					
				};

				ElevenLabsService.voiceId = dialoguePosition.GetPropName().Equals("Dru (Male)") ? "ZY37LYw0WtCyedeNw2EV" : "XfNU2rGpBa01ckF309OY";
				ElevenLabsService.Request(queue);
			});
		}

		public void OnVoiceEnded(string text)
        {
			gptInstance.GetComponentsInChildren<CanvasGroup>().ToList().Find(k => k.name.Equals("Canvas")).enabled = false;
			gptInstance.GetComponentsInChildren<CanvasGroup>().ToList().Find(k => k.name.Equals("Canvas")).interactable = false;
			APIService.service.StartCoroutine(RunGPT(text));
		}


        protected override void OnComplete()
        {
            base.OnComplete();
			gptInstance.GetComponentInChildren<VoiceActivationButton>().onComplete.RemoveListener(OnVoiceEnded);

		}
    }
}