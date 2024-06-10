using UnityEngine;

namespace Props.PropInterfaces
{
	public interface IPropAnimator : IBaseProp
	{
		public Animator GetAnimator();
	}
}