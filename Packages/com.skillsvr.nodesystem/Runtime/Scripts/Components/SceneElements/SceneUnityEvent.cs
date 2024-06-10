using System;
using UnityEngine;
using UnityEngine.Events;

namespace SkillsVRNodes
{
	public class SceneUnityEvent : SceneElement
	{
		public UnityEvent<Action> unityEvent;

		private Action doneCallback;

		public void Run(Action doneCallback)
		{
			this.doneCallback = doneCallback;
			unityEvent.Invoke(doneCallback);
		}

		public void Finish()
		{
			if (this.doneCallback != null)
				this.doneCallback.Invoke();

		}
	}
}