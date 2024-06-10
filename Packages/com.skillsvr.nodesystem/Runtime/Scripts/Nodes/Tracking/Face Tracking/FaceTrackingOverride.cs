using System;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes.Nodes
{
	public class FaceTrackingOverride : MonoBehaviour
	{
		public bool pass = true;
		public Action onInput;

		private void Update()
		{
			if (Input.GetKey(KeyCode.Alpha1))
			{
				pass = true;
				onInput.Invoke();

            }
			else if (Input.GetKey(KeyCode.Alpha2))
			{
				pass = false;
				onInput.Invoke();

            }
		}
	}
}