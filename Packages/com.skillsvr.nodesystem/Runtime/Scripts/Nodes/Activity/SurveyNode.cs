using GraphProcessor;
using SkillsVR.Mechanic.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Learning/Slider", typeof(SceneGraph)), NodeMenuItem("Learning/Slider", typeof(SubGraph))]
	public class SurveyNode : SpawnerNode<SpawnerSurvey, ISurveySystem, SurveyData>
	{
		public override string name => "Survey";
		public override string icon => "Quiz";
		public override Color color => NodeColours.Learning;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#rank-sort-node";
		public override int Width => MEDIUM_WIDTH;

		protected override void MechanicListener(IMechanicSystemEvent mechanicSystemEvent)
		{
			base.MechanicListener(mechanicSystemEvent);
		}
	}
}