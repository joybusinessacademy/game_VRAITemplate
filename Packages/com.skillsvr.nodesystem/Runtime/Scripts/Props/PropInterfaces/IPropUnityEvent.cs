using System;
using UnityEngine.Events;

namespace Props.PropInterfaces
{
	public interface IPropUnityEvent : IBaseProp
	{
		public UnityEvent GetUnityEvent();

		public void Run(UnityAction doneCallback);

	}
}