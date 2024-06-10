using System;
using GraphProcessor;
using SkillsVR.Mechanic.MechanicSystems.PanelImage;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
    [Serializable, NodeMenuItem("Learning/2D Panel Image", typeof(SceneGraph)), NodeMenuItem("Learning/2D Panel Image", typeof(SubGraph))]
    public class PanelImageNode : SpawnerNode<SpawnerPanelImage, IPanelImageSystem, PanelImageData>
    {
        public override string name => "2D panel Image";
        public override string icon => "Play";
        public override string layoutStyle => "PanoramaNode";
        public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#2d-panel-image-node";
        public override int Width => MEDIUM_WIDTH;

        public override Color color => NodeColours.Learning;
    }
}
