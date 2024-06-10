using GraphProcessor;
using SkillsVR.Mechanic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Learning/Sorting", typeof(SceneGraph)), NodeMenuItem("Learning/Sorting", typeof(SubGraph))]
	public class RankOrderNode : SpawnerNode<SpawnerRankOrder, IRankOrderSystem, RankOrderSystemData>
	{
		public override string name => "Sorting";
		public override string icon => "Quiz";
		public override Color color => NodeColours.Learning;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#rank-sort-node";
		public override int Width => MEDIUM_WIDTH;

		[Output(name = "Correct")] public ConditionalLink Correct = new();

		[Output(name = "Incorrect")] public ConditionalLink Incorrect = new();

		private bool mechanicFinishedCorrect;

		[HideInInspector]
		public int currentSelectedRankType;

		[SerializeField]
		public List<SpriteRankData> rankItemCustomSprite = new List<SpriteRankData>();


		protected override void OnInitialise()
		{
			MechanicData.isVisualOnStart = true;
		}
		
		protected override void MechanicListener(IMechanicSystemEvent mechanicSystemEvent)
		{
			switch (mechanicSystemEvent.eventKey)
			{
				case MechSysEvent.AfterFullStop:
					RunLink(mechanicFinishedCorrect ? nameof(Correct) : nameof(Incorrect));
					// need this
					// complete node is not call since active node is marked false
					RunLink(nameof(Complete), false);	
					break;
				case RankOrderSystemEvent.OnCorrectFinished:
					mechanicFinishedCorrect = true;
					break;
				case RankOrderSystemEvent.OnIncorrectFinished:
					mechanicFinishedCorrect = false;
					break;
			}

			base.MechanicListener(mechanicSystemEvent);
		}
	}

	[Serializable]
	public class SpriteRankData
	{
		public RankOrderItemData data;
	}

}
