using UnityEngine;

namespace Props.PropInterfaces
{
	public interface IPropLookAt : IBaseProp
	{
		public Transform GetLookAtTransform();

	}
}