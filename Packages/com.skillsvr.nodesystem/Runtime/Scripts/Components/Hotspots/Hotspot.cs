using System;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.HotSpots
{
	[SelectionBase]
	public class Hotspot : MonoBehaviour
	{
		public Vector3 Size = new Vector3(1, 2, 1);
		public Vector3 Position = new Vector3(0, 1, 0);
		
		private void OnDrawGizmos()
		{
			if (transform.childCount != 0)
			{
				return;
			}
			
			Gizmos.color = new Color(0.51f, 0.53f, 0.96f, 0.35f);
			Gizmos.matrix = this.transform.localToWorldMatrix;
			Gizmos.DrawCube(Position, Size);
		}
	}
}