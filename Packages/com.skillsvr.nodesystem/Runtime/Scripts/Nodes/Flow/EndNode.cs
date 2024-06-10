using GraphProcessor;
using UnityEngine;
using UnityEngine.Serialization;

namespace SkillsVRNodes.Scripts.Nodes
{
	[System.Serializable, NodeMenuItem("Flow/End")]
	public class EndNode : ExecutableNode
	{
		[Input(name = "End", allowMultiple = true)]
		public new ConditionalLink executed;
		
		[Output(name = " ")]
		private int Complete = new();

		public override string name => "End";
		public override string icon => "Exit";
		public override Color color => NodeColours.End;
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/base-nodes#end-node";
		public override bool SolidColor => true;

		public override string layoutStyle => "EndNode";
		[HideInInspector] public string targetTransitionName = "ScreenFadeToColor";


		public bool fade = true;
		public float fadeDuration = 0.5f;
		public Color fadeColor = Color.black;

		protected override void OnStart()
		{
			if (graph is SceneGraph)
			{
				MainGraphExecutorComponent mainGraphExecutorComponent = GameObject.FindObjectOfType<MainGraphExecutorComponent>();

				if(mainGraphExecutorComponent != null)
					mainGraphExecutorComponent.FadeOutAudio(0.5f);
			}
			if (!fade)
			{
				OnEnd();
				return;
			}
			
			fadeColor.a = 1;
			PlayerDistributer.LocalPlayer.SendMessage(targetTransitionName, new object[] { fadeDuration, fadeColor, null }, SendMessageOptions.DontRequireReceiver);
			WaitMonoBehaviour.Process(fadeDuration, OnEnd);
		}

		public void OnEnd()
		{
			nodeExecutor.onEndAction.Invoke();
		}
	}
}