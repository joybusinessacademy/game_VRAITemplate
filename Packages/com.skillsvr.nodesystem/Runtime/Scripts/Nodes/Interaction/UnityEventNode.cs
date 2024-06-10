using System;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Events;

namespace SkillsVRNodes.Scripts.Nodes
{
	[Serializable, NodeMenuItem("Custom/Unity Event", typeof(SceneGraph)), NodeMenuItem("Custom/Unity Event", typeof(SubGraph))]
	public class UnityEventNode : ExecutableNode
	{
		public PropGUID<IPropUnityEvent> unityEventProp;
		public float delay = 0;
		public bool waitForEvent = false;
		public override string name => "Unity Event";
		public override string icon => "Unity";
		public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/interaction-node-breakdown#unity-event-node";
		public override int Width => MEDIUM_WIDTH;


		public override Color color => NodeColours.Custom;

		protected override void OnStart()
		{
			IPropUnityEvent unityEvent = PropManager.GetProp(unityEventProp);

			if (unityEvent == null)
			{
				Debug.LogError($"Could not find event: {unityEventProp.propGUID}");
				CompleteNode();
			}
			else if (!waitForEvent)
			{
				//Fires unity events then continues node
				unityEvent.GetUnityEvent().Invoke();
				WaitMonoBehaviour.Process(delay, CompleteNode);
			}
			else
			{
				//Fires unity events
				unityEvent.GetUnityEvent().Invoke();

				//Adds CompleteCallback and Waits for manual Firing of Event to finish the node
				unityEvent.GetUnityEvent().AddListener(CompleteCallBack);
			}
		}

		protected override void OnComplete()
		{
			base.OnComplete();

			IPropUnityEvent unityEvent = PropManager.GetProp(unityEventProp);

			if(unityEvent != null)
				unityEvent.GetUnityEvent().RemoveListener(CompleteCallBack);
		}

		public void CompleteCallBack()
		{
			CompleteNode();
		}
	}
}