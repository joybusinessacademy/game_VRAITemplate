using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.Mechanic.Core
{
	public static class UnityExtensions
	{
		// Call this method to shuffle the children of the Transform
		public static void ShuffleChildren(this Transform root)
		{
			if (null == root)
			{
				return;
			}
			int childCount = root.childCount;
			List<Transform> children = new List<Transform>();

			// Store the children in a temporary list
			for (int i = 0; i < childCount; i++)
			{
				children.Add(root.GetChild(i));
			}

			// Shuffle the list using the Fisher-Yates shuffle algorithm
			for (int i = 0; i < childCount; i++)
			{
				int randomIndex = UnityEngine.Random.Range(i, childCount);
				Transform temp = children[randomIndex];
				children[randomIndex] = children[i];
				children[i] = temp;
			}

			// Reparent the shuffled children back to the original Transform
			for (int i = 0; i < childCount; i++)
			{
				children[i].SetSiblingIndex(i);
			}
		}
	}
}
