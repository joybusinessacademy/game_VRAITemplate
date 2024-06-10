using System;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]

public class LookAtPlayable : PlayableBehaviour
{
	public PropGUID<IPropLookAt> lookAtPoint;
	
	public Transform GetLookAtPoint()
	{
		return PropManager.GetProp(lookAtPoint)?.GetLookAtTransform();
	}
}