using System;
using SkillsVRNodes;
using VisualElements;

namespace Scripts.VisualElements
{
	[Obsolete]
	public class UnitySceneElementDropdown : SceneElementDropdown<SceneUnityEvent>
	{
		public UnitySceneElementDropdown(string label, string outputValue, ChangeDropdown changeEvent)
			: base(label, outputValue, changeEvent)
		{
		}
	}
}