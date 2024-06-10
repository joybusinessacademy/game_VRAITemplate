using System;
using System.Collections;
using System.Collections.Generic;

namespace SkillsVRNodes.Managers.Utility
{
	public static class RoutineExtensions
	{
		public static IEnumerator RunWithYieldReturnProxy(this IEnumerator enumerator, Action<IEnumerator> onBeforeYieldReturn
			, Action<IEnumerator> onAfterYieldReturn = null)
		{
			var root = enumerator;
			Stack<IEnumerator> stack = new Stack<IEnumerator>();
			stack.Push(root);
			while (stack.Count > 0)
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current is IEnumerator)
					{
						enumerator = enumerator.Current as IEnumerator;
						stack.Push(enumerator);
					}
					else
					{
						onBeforeYieldReturn?.Invoke(enumerator);
						yield return enumerator.Current;
						onAfterYieldReturn?.Invoke(enumerator);
					}
				}
				enumerator = stack.Pop();
			}
			
		}
	}
}
