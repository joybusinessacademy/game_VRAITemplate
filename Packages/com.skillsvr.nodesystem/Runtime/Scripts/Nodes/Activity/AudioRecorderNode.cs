using GraphProcessor;
using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.MechanicSystems.AudioRecorder;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Learning/Audio Recorder", typeof(SceneGraph)), NodeMenuItem("Learning/Audio Recorder", typeof(SubGraph))]
	public class AudioRecorderNode : SpawnerNode<SpawnerAudioRecorder, IAudioRecorderSystem, SkillsVR.Mechanic.MechanicSystems.AudioRecorder.AudioRecorderData>
	{
		public override string name => "Audio Recorder";
		public override string icon => "Record";
		public override Color color => NodeColours.Learning;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#audio-recorder-node";
		public override int Width => MEDIUM_WIDTH;

		public string saveName = "";

		// update this via node view
		// || means any button press will trigger it
		// the name are referenced on Unity Input Manager
		public string inputIds = "Axis9||Axis10"; 

		public static Dictionary<string, AudioClip> audioClipDictionary = new Dictionary<string, AudioClip>();

		public string nodePresetKey = "AudioRecorderSystem";

		public string AssociatedCustomClip;

		public int recorderInputStyle;

		[HideInInspector]
		public int currentSelectedRecorderType;
		 
		public Dictionary<string, string> inputIdsRemap = new Dictionary<string, string>()
		{
			{ "Any Trigger", "Axis9||Axis10" },
			{ "Both Trigger", "Axis9&&Axis10" },
			{ "Any Grip", "Axis11||Axis12" },
			{ "Both Grip", "Axis11&&Axis12" },
		};

		public Dictionary<string, int> nodePresetKeyRemap = new Dictionary<string, int>()
		{
			{ "Audio Recorder (Trigger Activation)", 0 },
			{ "Audio Recorder (Button Activation)", 1 },
		};

		[HideInInspector]
		public bool enableMaxRecordDuration = false;

		[HideInInspector]
		public bool keepMechanicAliveAfterRecord = false;

		protected override void OnStart()
		{
			base.OnStart();


			if (recorderInputStyle == 0)
			{
				// registering to mediator
				PlayerDistributer.LocalPlayer.SendMessage(name.Replace(" ", string.Empty), new object[] { mechanicSpawner.gameObject, inputIds }, SendMessageOptions.DontRequireReceiver);
			}
		}

		public override void SpawnObject()
		{
			base.SpawnObject();
			mechanicSpawner.mechanicData.recorderName = "Audio " + (string.IsNullOrWhiteSpace(saveName) ? "x" : saveName);
			mechanicSpawner.presetKey = "AudioRecorderSystem";// nodePresetKey.Contains("AudioRecorderSystem") ? nodePresetKey : "AudioRecorderSystem_UI_Default_V1";

			mechanicSpawner.mechanicData.stopMechanicOnRecordStop = !keepMechanicAliveAfterRecord;
			mechanicSpawner.mechanicData.loop = !enableMaxRecordDuration;
			mechanicSpawner.mechanicData.exportLoopClip = true;
		}

		protected override void MechanicListener(IMechanicSystemEvent mechanicSystemEvent)
		{
		    switch (mechanicSystemEvent.eventKey)
		    {
			case MechSysEvent.AfterFullStop:
			    audioClipDictionary.TryAdd(saveName, MechanicData.audioClip);
			    audioClipDictionary[saveName] = MechanicData.audioClip;
			    break;
		    }

		    base.MechanicListener(mechanicSystemEvent);
		}

		protected override void OnComplete()
		{
			base.OnComplete();
			if (recorderInputStyle == 0)
			{
				// unregistering to mediator
				PlayerDistributer.LocalPlayer.SendMessage(UnregisterMediatorId, mechanicSpawner.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}

	}
}
