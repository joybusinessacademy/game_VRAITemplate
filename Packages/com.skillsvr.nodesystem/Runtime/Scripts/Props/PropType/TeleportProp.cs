using System;
using Props.PropInterfaces;
using UnityEngine;

namespace Props
{
	[Serializable]

	public class TeleportProp : PropType, IPropPlayerPosition
	{
		public Transform position;
		public override string name => "Teleport Prop";

		public Transform GetTransform()
		{
			if (position == null)
			{
				position = propComponent.transform;
			}
			return propComponent.transform;	
		}

		public TeleportProp(PropComponent propComponent) : base(propComponent)
		{
		}

		public override void AutoConfigProp()
		{
			position = propComponent.transform.Find("Teleport Position");
			position ??= propComponent.transform;
		}
	}
}