using System;
using GraphProcessor;
using UnityEngine;


namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Functions/Condition")]
	public class ConditionNode : ExecutableNode
	{
		public enum ECondition
		{
			Equals,
			UnEquals,
			LessThan,
			LessThanEquals,
			GreaterThan,
			GreaterThanEquals,
		}


		[Output(name = "True")] public ConditionalLink trueCondition;

		[Output(name = "False")] public ConditionalLink falseCondition;


		public FloatSO firstVariableSO;
		public float firstVariable;

		public ECondition condition;

		public FloatSO secondVariableSO;
		public float secondVariable;


		public override string name => "Condition";
		public override string icon => "Condition";
		public override string layoutStyle => "ConditionNode";
		public override Color color => NodeColours.Other;


		protected override void OnStart()
		{
			RunLink(IsNodeTrue() ? nameof(trueCondition) : nameof(falseCondition));
            RunLink("Complete", false);
        }

		private bool IsNodeTrue()
		{
			return condition switch
			{
				ECondition.Equals => (firstVariableSO ? firstVariableSO.value : firstVariable) ==
				                     (secondVariableSO ? secondVariableSO.value : secondVariable),

				ECondition.UnEquals => (firstVariableSO ? firstVariableSO.value : firstVariable) !=
				                       (secondVariableSO ? secondVariableSO.value : secondVariable),

				ECondition.LessThan => (firstVariableSO ? firstVariableSO.value : firstVariable) <
				                       (secondVariableSO ? secondVariableSO.value : secondVariable),

				ECondition.LessThanEquals => (firstVariableSO ? firstVariableSO.value : firstVariable) <=
				                             (secondVariableSO ? secondVariableSO.value : secondVariable),

				ECondition.GreaterThan => (firstVariableSO ? firstVariableSO.value : firstVariable) >
				                          (secondVariableSO ? secondVariableSO.value : secondVariable),

				ECondition.GreaterThanEquals => (firstVariableSO ? firstVariableSO.value : firstVariable) >=
				                                (secondVariableSO ? secondVariableSO.value : secondVariable),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}


