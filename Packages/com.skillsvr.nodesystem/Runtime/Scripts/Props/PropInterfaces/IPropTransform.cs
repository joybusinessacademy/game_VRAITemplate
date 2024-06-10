using UnityEngine;

namespace Props.PropInterfaces
{
	public interface IPropTransform : IBaseProp
	{
		public Transform GetTransform();
		
		public Vector3 GetPosition()
		{
			return GetTransform().position;
		}
	}
}