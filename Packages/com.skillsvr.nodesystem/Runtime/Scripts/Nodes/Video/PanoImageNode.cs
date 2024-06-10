using System;
using GraphProcessor;
using SkillsVR.Mechanic.MechanicSystems.PanoImage;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Flow/360 Image", typeof(SceneGraph)), NodeMenuItem("Flow/360 Image", typeof(SubGraph))]
	public class PanoImageNode : SpawnerNode<SpawnerPanoImage, IPanoImageSystem, PanoImageData>
	{
		public override string name => "360 Image";
		public override string icon => "Play";
		public override string layoutStyle => "PanoramaNode";

		public override Color color => NodeColours.Other;

		public override int Width => MEDIUM_WIDTH;
	}
}
