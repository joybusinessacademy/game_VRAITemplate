#if NODE_DEVELOPMENT
using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;
using System;
using UnityEngine;

[Serializable, NodeMenuItem("Functions/Debug Log")]
public class DebugLogNode : ExecutableNode
{
	public override string name => "Debug Log";
	public override string icon => "";
	public override Color color => NodeColours.Custom;

	public string logToConsole;
	public bool errorLog;

	protected override void OnStart()
	{
		if (errorLog)
			Debug.LogError("DEBUG LOG: " + logToConsole);
		else
			Debug.Log("DEBUG LOG: " + logToConsole);

		CompleteNode();
	}
}
#endif