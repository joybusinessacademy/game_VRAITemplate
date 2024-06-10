
using System.Collections.Generic;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using SkillsVR.Mechanic.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillsVRNodes.Scripts.Nodes
{

	[System.Serializable, NodeMenuItem("Learning/Marker Question", typeof(SceneGraph)), NodeMenuItem("Learning/Marker Question", typeof(SubGraph))]
	public class MarkerQuestionsNode : SpawnerNode<SpawnerMarkerQuestion, IMarkerQuestionSystem, MarkerQuestionsData>, IDynamicOutputPortCollection
	{
		public override string name => "Marker Question";
		public override string icon => "Quiz";
		public override Color color => NodeColours.Learning;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#marker-question-node";
		public override int Width => MEDIUM_WIDTH;

		public List<PropGUID<IPropPanel>> allGameObjects = new();

		[HideInInspector] public PropGUID<IPropPanel> lockInButtonProp;

		[Output(name = "Correct")] public ConditionalLink Correct = new();

		[Output(name = "Incorrect")] public ConditionalLink Incorrect = new();

		private bool mechanicFinishedCorrect;
		private int selectedAnswer;

		protected override void OnStart()
		{
			base.OnStart();
		}
		protected void InitMarkerData()
		{
			for (int i = 0; i < MechanicData.markerDatas.Count; i++)
			{
				MarkerData markerData = MechanicData.markerDatas[i];
				if (allGameObjects.Count > i && allGameObjects[i] != null)
				{
					if (allGameObjects[i] == null || allGameObjects[i] == null)
						markerData.spawnPoint = new GameObject("Temp Marker Position");
					else
					{
						if (allGameObjects[i].propGUID != "00000000-0000-0000-0000-000000000000")
							markerData.spawnPoint = PropManager.GetProp<IPropPanel>(allGameObjects[i]).GetPropComponent()?.gameObject;
						else
							Debug.LogWarning("Missing spawn point data for Marker");

					}
				}

			}

			if (lockInButtonProp != null && string.IsNullOrEmpty(lockInButtonProp) == false)
				MechanicData.lockInButtonLocation = PropManager.GetProp<IPropPanel>(lockInButtonProp).GetPropComponent().transform;

			// for the meantime, send event to first object assigned
			if (allGameObjects.Count > 0 && MechanicData.markerDatas[0].spawnPoint != null)
				MechanicData.markerDatas[0].spawnPoint.SendMessage("ReceiveParams", mechanicSpawner.gameObject, SendMessageOptions.DontRequireReceiver);

		}

		protected override void MechanicListener(IMechanicSystemEvent mechanicSystemEvent)
		{
			switch (mechanicSystemEvent.eventKey)
			{
				case MechSysEvent.OnSetData:
					InitMarkerData();
					break;
				case MechSysEvent.AfterFullStop:
					if (!nodeActive)
					{
						break;
					}

					RunLink(mechanicFinishedCorrect ? nameof(Correct) : nameof(Incorrect));
					// need this
					// complete node is not call since active node is marked false
					RunLink(nameof(Complete), false);
					RunSelectedOutput();
					break;
				case MarkerEvent.MarkerFinishedCorrect:
					mechanicFinishedCorrect = true;
					break;
				case MarkerEvent.MarkerFinishedIncorrect:
					mechanicFinishedCorrect = false;
					break;
				case MarkerEvent.OnChoiceSelected:
					selectedAnswer = mechanicSystemEvent.GetData<int>();
					break;
			}

			base.MechanicListener(mechanicSystemEvent);
		}

		public void RunSelectedOutput()
		{

			RunLink("Output" + (selectedAnswer + 1), false);
		}

		[Output(name = "Answer 1:")]
		public ConditionalLink Output1 = new();
		[Output(name = "Answer 2:")]
		public ConditionalLink Output2 = new();
		[Output(name = "Answer 3:")]
		public ConditionalLink Output3 = new();
		[Output(name = "Answer 4:")]
		public ConditionalLink Output4 = new();
		[Output(name = "Answer 5:")]
		public ConditionalLink Output5 = new();
		[Output(name = "Answer 6:")]
		public ConditionalLink Output6 = new();
		[Output(name = "Answer 7:")]
		public ConditionalLink Output7 = new();
		[Output(name = "Answer 8:")]
		public ConditionalLink Output8 = new();
		[Output(name = "Answer 9:")]
		public ConditionalLink Output9 = new();
		[Output(name = "Answer 10:")]
		public ConditionalLink Output10 = new();

		public string GetOutputPortNameByIndex(int index)
		{
			return "Output" + (index + 1);
		}
	}
}
