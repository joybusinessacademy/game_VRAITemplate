using SkillsVRNodes.Scripts.Nodes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace SkillsVRNodes
{
	[RequireComponent(typeof(PlayableDirector))]
	public class SceneTimeline : SceneElement
	{
		private PlayableDirector director;
		public PlayableDirector Director
		{
			get
			{
				if (!director)
				{
					director = GetComponent<PlayableDirector>();
				}
				return director;
			}
		}

  		public override void Reset()
		{
			base.Reset();
			Director.playOnAwake = false;
		}
	}
}
