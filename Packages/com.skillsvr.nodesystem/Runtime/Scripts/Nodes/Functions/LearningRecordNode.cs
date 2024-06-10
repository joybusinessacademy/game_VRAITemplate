using System;
using GraphProcessor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Learning/Learning Record", typeof(SceneGraph)), NodeMenuItem("Learning/Learning Record", typeof(SubGraph))]
	public class LearningRecordNode : ExecutableNode
	{
		public override string name => "Learning Record";
		public override string icon => "Quiz";
		public override string layoutStyle => "LearningRecordNode";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#learning-record-node";
		public override int Width => MEDIUM_WIDTH;

		public override Color color => NodeColours.Learning;

		public string learningRecordPortalId = string.Empty;
		public string learningRecordId = string.Empty;
		public bool idTargetValue = true;

		private static GameObject restCoreObject = null;
		private const string targetMethodName = "ECSetGameScoreBoolFromNode";

		protected override void OnStart()
		{
			if (restCoreObject == null)
				restCoreObject = GameObject.Find("RESTService");

			// mark record on our config			
			restCoreObject.SendMessage(targetMethodName, new object[]{ learningRecordPortalId, idTargetValue }, SendMessageOptions.DontRequireReceiver);
			
			// make complete action usable
			CompleteNode();
		}
	}
}