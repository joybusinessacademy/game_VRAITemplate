using System;
//using CrazyMinnow.SALSA;
using DialogExporter;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using UnityEngine;


namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Character & Props/Dialogue", typeof(SceneGraph)), NodeMenuItem("Character & Props/Dialogue", typeof(SubGraph))]
	public class DialogueNode : ExecutableNode
	{
		[SerializeField] public  LocalizedDialog dialogueAsset;
		[SerializeField] public PropGUID<IPropAudioSource> dialoguePosition;
		public override string name => "Dialogue";
		public override string icon => "Dialogue";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/interaction-node-breakdown#dialogue-node";

		public override Color color => NodeColours.CharactersAndProps;
		public override int Width => MEDIUM_WIDTH;

		public AudioClip audioClipBeingUsed;

		protected override void OnStart()
		{
			var sceneAudio = PropManager.GetProp<IPropAudioSource>(dialoguePosition);
			
			if (dialogueAsset == null)
			{
				Debug.LogWarning("No Dialogue was chosen");
				CompleteNode();
				return;
			}

			if (dialogueAsset.GetAudioClip == null)
			{
				Debug.LogError("No Audio Clip found");
				CompleteNode();
				return;
			}

			if (sceneAudio == null)
			{
				PropManager.DisplayCaption(dialogueAsset.Dialog, dialogueAsset.GetAudioClip.length);
				PropManager.PlayAudio(dialogueAsset.GetAudioClip);
			}
			else
			{
				PropManager.DisplayCaptionWithName(sceneAudio.GetPropComponent().PropName, dialogueAsset.Dialog, dialogueAsset.GetAudioClip.length);
				if (null == sceneAudio.GetAudioSource() || !sceneAudio.GetAudioSource().isPlaying)
				{
					// Only play audio clip when the audio source is free (in case if timeline already play a dialog).
					sceneAudio.PlayAudio(dialogueAsset.GetAudioClip);
				}
			}
#if FAST_TRACK
			WaitMonoBehaviour.Process(0.1f, CompleteNode);
#else
			WaitMonoBehaviour.Process(dialogueAsset.GetAudioClip.length + 0.3f, CompleteNode);
#endif


		}

		protected override void OnComplete()
		{
			StopSceneAudioIfIsPlayingMyClip();
			base.OnComplete();
		}

		protected void StopSceneAudioIfIsPlayingMyClip()
		{
			IPropAudioSource sceneAudio = PropManager.GetProp<IPropAudioSource>(dialoguePosition);
			if (null != sceneAudio
				&& null != sceneAudio.GetAudioSource()
				&& null != sceneAudio.GetAudioSource().clip
				&& null != dialogueAsset
				&& sceneAudio.GetAudioSource().clip == dialogueAsset.GetAudioClip
				&& sceneAudio.GetAudioSource().isPlaying)
			{
				sceneAudio.GetAudioSource().Stop();
			}
		}
	}
}


