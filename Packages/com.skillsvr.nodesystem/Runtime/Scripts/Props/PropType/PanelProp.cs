using System;
using Props.PropInterfaces;
using UnityEngine;

namespace Props
{
	[Serializable]
	public class PanelProp : PropType, IPropPanel, IPropAudioSource
	{
		public override string name => "Panel Prop";
		
		public Transform position;
		public AudioSource audioSource;
		
		public PanelProp(PropComponent propComponent) : base(propComponent)
		{
			if (GetAudioSource() != null)
			{
				GetAudioSource().playOnAwake = false;
			}
		}

		public Transform GetTransform()
		{
			if (position == null)
			{
				position = propComponent.transform;
			}
			return position;	
		}
		
		public override void AutoConfigProp()
		{
			audioSource = propComponent.GetComponentInChildren<AudioSource>();
			audioSource ??= propComponent.gameObject.AddComponent<AudioSource>();
			
			if (GetAudioSource() != null)
			{
				GetAudioSource().playOnAwake = false;
			}

			position = propComponent.transform.Find("Panel Position");
			position ??= propComponent.transform;
		}

		public AudioSource GetAudioSource()
		{
			AudioSource audioSource = propComponent.GetComponent<AudioSource>();

			if(audioSource == null)
			{
				audioSource = propComponent.gameObject.AddComponent<AudioSource>();
				audioSource.loop = false;
				audioSource.playOnAwake = false;
				audioSource.spatialBlend = 1;
			}

			return audioSource;
		}
	}
}