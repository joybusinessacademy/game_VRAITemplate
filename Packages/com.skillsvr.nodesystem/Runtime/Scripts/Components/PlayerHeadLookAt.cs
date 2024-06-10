using System;
using Props;
using UnityEngine;

namespace SkillsVRNodes
{
	[RequireComponent(typeof(PropComponent))]
	public class PlayerHeadLookAt : MonoBehaviour
	{
		private void Update()
		{
			if (Camera.main != null)
			{
				transform.position = Camera.main.transform.position;
			}
		}

		private void Reset()
		{
			PropComponent prop = GetComponent<PropComponent>();
			prop.name = "Head Look At";

			prop.propType = new LookAtProp(prop)
			{
				lookAtPoint = transform
			};
		}
	}
}