using System;
using GraphProcessor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Functions/Switch", typeof(SceneGraph)), NodeMenuItem("Functions/Switch", typeof(SubGraph))]
	public class SwitchNode : ExecutableNode
	{
		public override string name => "Switch";
		public override string icon => "Condition";
		public override string layoutStyle => "SwitchNode";

		public override Color color => NodeColours.Other;

		
		public FloatSO variable;
		public int outputAmount = 4;
		
		[Output(name = "Case 0:")]
		public ConditionalLink Output0 = new();
		[Output(name = "Case 1:")]
		public ConditionalLink Output1 = new();
		[Output(name = "Case 2:")]
		public ConditionalLink Output2 = new();
		[Output(name = "Case 3:")]
		public ConditionalLink Output3 = new();
		[Output(name = "Case 4:")]
		public ConditionalLink Output4 = new();
		[Output(name = "Case 5:")]
		public ConditionalLink Output5 = new();
		[Output(name = "Case 6:")]
		public ConditionalLink Output6 = new();
		[Output(name = "Case 7:")]
		public ConditionalLink Output7 = new();
		[Output(name = "Case 8:")]
        public ConditionalLink Output8 = new ();
        [Output(name = "Case 9:")]
        public ConditionalLink Output9 = new();

        [Output(name = "Default")]
        public ConditionalLink Default = new ();

        protected override void OnStart()
		{
			if (variable.value > outputAmount)
			{
				RunLink(nameof(Default));
                RunLink("Complete", false);
                return;
			}
			
			switch ((int)variable.value)
			{
				case 0:
					RunLink(nameof(Output0));
					break;
				case 1:
					RunLink(nameof(Output1));
					break;
				case 2:
					RunLink(nameof(Output2));
					break;
				case 3:
					RunLink(nameof(Output3));
					break;
				case 4:
					RunLink(nameof(Output4));
					break;
				case 5:
					RunLink(nameof(Output5));
					break;
				case 6:
					RunLink(nameof(Output6));
					break;
				case 7:
					RunLink(nameof(Output7));
					break;
				case 8:
					RunLink(nameof(Output8));
					break;
				case 9:
					RunLink(nameof(Output9));
					break;
				default:
					RunLink(nameof(Default));
					break;
			}
			
            RunLink("Complete", false);
        }
	}
}
