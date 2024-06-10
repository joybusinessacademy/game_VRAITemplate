using UnityEngine;

namespace SkillsVRNodes.ScriptableObjects
{
	public abstract class VariableSO : ScriptableObject
	{
		public virtual string typeName => "Variable";
	}
}