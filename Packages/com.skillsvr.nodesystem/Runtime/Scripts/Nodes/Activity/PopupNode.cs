using GraphProcessor;
using SkillsVR.Mechanic.Core;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Learning/Popup", typeof(SceneGraph)), NodeMenuItem("Learning/Popup", typeof(SubGraph))]
	public class PopupNode : SpawnerNode<SpawnerInformationPopUp, IInformationPopUpSystem, InfoPopUpDatas>
	{
		public override string name => "Popup";
		public override string icon => "Quiz";
		public override Color color => NodeColours.Learning;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#popup-node";
		public override int Width => MEDIUM_WIDTH;

	}
}
