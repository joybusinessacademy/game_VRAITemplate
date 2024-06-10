using System;
using GraphProcessor;
using SkillsVR.Mechanic.MechanicSystems.PanelVideo;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
    [Serializable, NodeMenuItem("Learning/2D Panel Video", typeof(SceneGraph)), NodeMenuItem("Learning/2D Panel Video", typeof(SubGraph))]
    public class PanelVideoNode : SpawnerNode<SpawnerPanelVideo, IPanelVideoSystem, PanelVideoData>
    {
        public override string name => "2D Panel Video";
        public override string icon => "Play";
        public override string layoutStyle => "PanoramaNode";
        public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#2d-panel-video-node";
        public override int Width => MEDIUM_WIDTH;

        public override Color color => NodeColours.Learning;
    }
}
