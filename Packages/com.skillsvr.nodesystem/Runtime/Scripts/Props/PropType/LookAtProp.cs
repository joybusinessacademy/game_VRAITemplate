using System;
using Props.PropInterfaces;
using UnityEngine;

namespace Props
{
	[Serializable]
	public class LookAtProp : PropType, IPropLookAt
	{		
		public Transform lookAtPoint;

		public Transform GetLookAtTransform()
		{
			return lookAtPoint;
		}
		
		public override void AutoConfigProp()
		{			
			lookAtPoint = propComponent.transform;
		}

		public LookAtProp(PropComponent propComponent) : base(propComponent)
		{
		}
	}
}