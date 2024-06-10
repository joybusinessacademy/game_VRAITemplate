using GraphProcessor;
using SkillsVR.Mechanic.MechanicSystems.AudioPlayback;
using System;
using System.Collections.Generic;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Learning/Audio Playback", typeof(SceneGraph)), NodeMenuItem("Learning/Audio Playback", typeof(SubGraph))]
	public class AudioPlaybackNode : SpawnerNode<SpawnerAudioPlayback, IAudioPlaybackSystem, SkillsVR.Mechanic.MechanicSystems.AudioPlayback.AudioPlaybackData>
	{
		public override string name => "Audio Playback";
		public override string icon => "Sound";
		public override Color color => NodeColours.Learning;
		public override string layoutStyle => "AudioPlaybackNode";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#audio-playback-node";
		public override int Width => MEDIUM_WIDTH;

		public BaseGraph Graph => ((BaseNode)this).Graph;
		
		public string AssociatedRecorderNodesaveName;
		public PropGUID<IPropAudioSource> AssociatedPlaybackAudioProp;
		public string AssociatedCustomClip; 



		protected override void OnStart()
		{	
			if (AudioRecorderNode.audioClipDictionary.ContainsKey(AssociatedRecorderNodesaveName))
			{
				MechanicData.audioClip = AudioRecorderNode.audioClipDictionary[AssociatedRecorderNodesaveName];
			} 
			else if (!string.IsNullOrEmpty(AssociatedRecorderNodesaveName))
			{
				List<BaseNode> allRecorders = Graph.nodes.FindAll(t => t.GetType() == typeof(AudioRecorderNode));
				AudioRecorderNode theNode = (AudioRecorderNode)allRecorders.Find(t => ((AudioRecorderNode)t).saveName.Equals(AssociatedRecorderNodesaveName));
				if (theNode != null)
					MechanicData.audioClip = theNode.MechanicData.audioClip;
			}

			if (AssociatedPlaybackAudioProp != null && AssociatedPlaybackAudioProp.GetProp() != null)
			{
				MechanicData.audioSource = AssociatedPlaybackAudioProp.GetProp().GetAudioSource();
			}
			else
			{
				MechanicData.audioSource = PropManager.GetAudioSource();
			}

			base.OnStart();
		}
   	 }
}
