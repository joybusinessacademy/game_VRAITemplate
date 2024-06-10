using System;
using System.Reflection;
using GraphProcessor;
using SkillsVRNodes.Managers;

namespace SkillsVRNodes.Scripts.Nodes
{
	
	/// <summary>
	/// This is the base class for every node that is executed by the conditional processor, it takes an executed bool as input to 
	/// </summary>
	[Serializable]
	public abstract class ExecutableNode : BaseNode
	{
		// These booleans will controls whether or not the execution of the following nodes will be done or discarded.
		[Input(name = "Start", allowMultiple = true)]
		public ConditionalLink executed;
		[Output(name = "Complete")]
		public ConditionalLink Complete = new();

		// Assure that the executed field is always at the top of the node port section
		public override FieldInfo[] GetNodeFields()
		{
			FieldInfo[] fields = base.GetNodeFields();
			Array.Sort(fields, (f1, f2) => f1.Name == nameof(executed) ? -1 : 1);
			return fields;
		}

		protected NodeExecutor nodeExecutor;
		
		public void Initialise()
		{
			isInitialised = true;
			nodeActive = false;
			OnInitialise();
		}
		
		/// <summary>
		/// Executes for all nodes when the scene is started
		/// </summary>
		/// <descripton>
		/// Use this to initialise components that the node will need to run. e.g. Loading assets
		/// </descripton>
		protected virtual void OnInitialise()
		{
		}

		/// <summary>
		/// Will start the node 
		/// </summary>
		public void StartNode(NodeExecutor newRunNodeComponent)
		{
			if (!isInitialised)
			{
				Initialise();
			}
			
			nodeExecutor = newRunNodeComponent;
			nodeActive = true;
			customNodeEvent?.Invoke(this, nameof(NodeActive), NodeActive);
			OnStart();
		}

		/// <summary>
		/// Start node and identify the input edge.
		/// </summary>
		/// <param name="newRunNodeComponent">The executor that handle graph running.</param>
		/// <param name="inputEdge">The edge that trigger start node event</param>
		/// <returns>Does successfully start node.</returns>
		public virtual bool StartNodeFrom(NodeExecutor newRunNodeComponent, SerializableEdge inputEdge)
		{
			StartNode(newRunNodeComponent);
			return true;
		}

		/// <summary>
		/// Executes on node entry
		/// </summary>
		/// <descripton>
		/// Use this to start activities or call events 
		/// </descripton>
		protected virtual void OnStart()
		{
			if (shouldSkip)
				OnSkip();
		}

		/// <summary>
		/// Runs all the connected nodes from a link
		/// </summary>
		/// <returns>Success</returns>
		public bool RunLink(string nameOfConnection, bool isNodeComplete = true)
		{
			if (nodeExecutor != null)
			{
				nodeActive = !isNodeComplete;
				nodeExecutor.RunConnectedNodes(this, nameOfConnection);
				return true;
			}

			return false;
		}

		protected bool nodeActive;
		/// <summary>
		/// Is the node currently active
		/// </summary>
		public bool NodeActive => nodeActive;
		public void StopNode()
		{
			isInitialised = false;
			nodeActive = false;
			SkipNode();
		}

		protected bool shouldSkip;
		/// <summary>
		/// should the node be skipped or not
		/// </summary>
		public bool ShouldSkip { get { return shouldSkip; } set { shouldSkip = value; } }

		protected virtual void OnComplete()
		{
		}

		public void SetNodeActiveState(bool state)
		{
			nodeActive = state;
		}

		public void CompleteNode()
		{
			if (!NodeActive)
			{
				return;
			}
			nodeActive = false;
            BaseNode.customNodeEvent?.Invoke(this, nameof(NodeActive), NodeActive);
            OnComplete();
			RunLink(nameof(Complete));
		}

		private bool isInitialised = false;
		
		public void SkipNode()
		{
			OnSkip();
		}

		protected virtual void OnSkip()
		{
			
		}

	}
}