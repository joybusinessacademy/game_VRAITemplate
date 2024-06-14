using UnityEngine;

namespace GraphProcessor
{
	[CreateAssetMenu(fileName = "newSceneGraph", menuName = "SkillsVR/SceneGraph", order = 2)]
	public class SceneGraph : BaseGraph
	{
		public string assistantId;
		public string npcId;
		public string assistantInstruction;
	}
}