using System;
using GraphProcessor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
    [Serializable, NodeMenuItem("Effects/Transition", typeof(SceneGraph)), NodeMenuItem("Effects/Transition", typeof(SubGraph))]
    public class PlayerTransitionNode : ExecutableNode
    {
        public override string name => "Transition";
        public override string icon => "Timer";
        public override string layoutStyle => "PlayerTransitionNode";
        public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/interaction-node-breakdown#transition-node";
        public override Color color => NodeColours.Effects;
        public override int Width => MEDIUM_WIDTH;

        public Color targetColor = Color.white;
        public float transitionTime = 1f;
        [HideInInspector] public string targetTransitionName = "ScreenFadeToColor";

        protected override void OnStart()
        {
            // need to improve this, low performance
            // lamda cant be encapsulated in object[]
            PlayerDistributer.LocalPlayer.SendMessage(targetTransitionName, new object[] { transitionTime, targetColor, null }, SendMessageOptions.DontRequireReceiver);
            WaitMonoBehaviour.Process(transitionTime, CompleteNode);
        }
    }
}
