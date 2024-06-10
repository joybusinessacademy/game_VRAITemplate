using System;
using UnityEngine;

namespace Props
{
	public class PropGizmo : MonoBehaviour
	{
		public Color Color = new Color(0.12f, 0.68f, 1f, 0.42f);
		public Vector3 position;
		public Vector3 scale = Vector3.one * 0.1f;

		public bool drawAlways = false;

		private void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color;
			Gizmos.DrawCube(position, scale);
		}

		private void OnDrawGizmos()
		{
			if (!drawAlways)
				return;

			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color;
			Gizmos.DrawCube(position, scale);
		}
	}
}