using GraphProcessor;
using SkillsVRNodes.Managers;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Flow/Start",typeof(SceneGraph))]
	public class StartNode : BaseNode
	{
		[Output(name = "Start", allowMultiple = true)]
		public ConditionalLink executes = new();

		public override string name => "Start";
		public override string icon => "start";
		public override Color color => NodeColours.Start;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/base-nodes#start-node";
		public override string layoutStyle => "StartNode";
		public override bool SolidColor => true;

		public float fadeDuration = 0.5f;
		public bool autoFade = true;
		protected NodeExecutor nodeExecutor;
		[HideInInspector] public string targetTransitionName = "ScreenFadeToColor";

		public void OnStart(NodeExecutor newRunNodeComponent)
		{
			if (graph is SceneGraph)
			{
				GameObject.FindObjectOfType<MainGraphExecutorComponent>()?.FadeInAudio(0.5f);
			}
			
			nodeExecutor = newRunNodeComponent;
			if (newRunNodeComponent.GetType() == typeof(SceneNodeExecutor))
			{
				CompleteNode();
				return;
			}

			if (autoFade)
			{
				PlayerDistributer.LocalPlayer.SendMessage(targetTransitionName, new object[] { fadeDuration, Color.clear, null }, SendMessageOptions.DontRequireReceiver);
				WaitMonoBehaviour.Process(fadeDuration, CompleteNode);
			}
			else
			{
				CompleteNode();
			}
		}
		
		public void CompleteNode()
		{
			nodeExecutor.RunConnectedNodes(this, nameof(executes));
		}
    }
}
