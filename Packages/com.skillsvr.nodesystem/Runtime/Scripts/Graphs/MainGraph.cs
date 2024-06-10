using SkillsVRNodes.Scripts.Nodes;
using System.Linq;
using UnityEngine;

namespace GraphProcessor
{
	[CreateAssetMenu(fileName = "newMainGraph", menuName = "SkillsVR/MainGraph", order = 1)]
	public class MainGraph : BaseGraph
	{
		public override void OnEnable()
		{
			BuildGraphElements();

			if (0 == nodes.Count())
			{
				AddNode(BaseNode.CreateFromType(typeof(MainStartNode), Vector2.zero));
				AddNode(BaseNode.CreateFromType(typeof(EndNode), Vector2.right * 400));
			}

			base.OnEnable();
		}
	}
}