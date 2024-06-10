using System;
using GraphProcessor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Functions/Counter", typeof(SceneGraph)), NodeMenuItem("Functions/Counter", typeof(SubGraph))]

	public class CounterNode : ExecutableNode
	{
		public override string name => "Counter";
		public override string icon => "Condition";
		public override Color color => NodeColours.Other;

		public uint inputsBeforeOutput = 1;
		public bool limitOutputs = true;
		public uint outputsAmount = 1;

		private int amountOfInputs = 0;
		private int amountOfOutputs = 0;

        protected override void OnInitialise()
        {
            base.OnInitialise();
			amountOfInputs = 0;
			amountOfOutputs = 0;
		}

        protected override void OnStart()
		{
			if (amountOfInputs >= inputsBeforeOutput)
			{
				if (!limitOutputs || amountOfOutputs < outputsAmount)
				{
					CompleteNode();
					amountOfOutputs++;
				}
			}
			
			amountOfInputs++;
		}
	}
}