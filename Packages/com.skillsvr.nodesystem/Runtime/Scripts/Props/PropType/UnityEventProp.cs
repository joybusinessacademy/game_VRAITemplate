using System;
using Props.PropInterfaces;
using UnityEngine.Events;

namespace Props
{
	public class UnityEventProp : PropType, IPropUnityEvent
	{
		public UnityEvent unityEvent;

		public UnityEventProp() : base(null)
		{
		}

		public UnityEventProp(PropComponent propComponent) : base(propComponent)
		{
		}

		public UnityEvent GetUnityEvent()
		{
			return unityEvent;
		}

		public void Run(UnityAction doneCallback)
		{
			unityEvent.AddListener(doneCallback);
			unityEvent.Invoke();
		}

		public override void AutoConfigProp()
		{
			PropEventHandler propEventHandler = GetPropComponent()?.GetComponent<PropEventHandler>();

			if (propEventHandler == null)
			{
				GetPropComponent().gameObject.AddComponent<PropEventHandler>();
			}
		}
	}
}