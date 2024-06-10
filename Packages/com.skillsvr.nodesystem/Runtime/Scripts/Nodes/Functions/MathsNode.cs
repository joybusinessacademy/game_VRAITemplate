using System;
using GraphProcessor;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Functions/Maths")]
	public class MathsNode : ExecutableNode
	{
		public enum EMaths
		{
			Set,
			Add,
			Remove,
			Multiply,
			Divide,
		}

		public FloatSO alteredVariableSO;

		public EMaths condition;

		public FloatSO modifierVariableSO;
		public float modifierVariable;


		public override string name => "Maths";
		public override string icon => "Maths";
		public override string layoutStyle => "MathsNode";
		public override Color color => NodeColours.Other;

		protected override void OnStart()
		{
			switch (condition)
			{
				case EMaths.Set:
					alteredVariableSO.value = modifierVariableSO ? modifierVariableSO.value : modifierVariable;
					break;
				case EMaths.Add:
					alteredVariableSO.value += modifierVariableSO ? modifierVariableSO.value : modifierVariable;
					break;
				case EMaths.Remove:
					alteredVariableSO.value -= modifierVariableSO ? modifierVariableSO.value : modifierVariable;
					break;
				case EMaths.Multiply:
					alteredVariableSO.value *= modifierVariableSO ? modifierVariableSO.value : modifierVariable;
					break;
				case EMaths.Divide:
					alteredVariableSO.value /= modifierVariableSO ? modifierVariableSO.value : modifierVariable;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			CompleteNode();
		}
	}
}
