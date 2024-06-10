using UnityEngine;

namespace SkillsVR.Mechanic.Core.Impl
{
	static class ProviderInitMethod
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
#endif
		private static void InitProvider()
		{
			if (null != MechanicProvider.Current)
			{
				return;
			}
			var providerChain = new ChainedMechanicProvider();
			providerChain.providers.Add(new PooledMechanicProvider());
			MechanicProvider.Current = providerChain;
		}
	}
}
