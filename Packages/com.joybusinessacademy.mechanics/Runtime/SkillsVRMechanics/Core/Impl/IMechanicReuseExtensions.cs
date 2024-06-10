using UnityEngine;

namespace SkillsVR.Mechanic.Core
{
	public static class IMechanicReuseExtensions
	{
		public static void SendBackForReuse(this IMechanicSystem mechanic)
		{
			if (null == mechanic)
			{
				return;
			}
			var component = mechanic as Component;
			if (null == component)
			{
				return;
			}
			if (null != component.transform.parent)
			{
				Transform nullTransform = null;
				component.transform.SetParent(nullTransform);
			}
			component.gameObject.SetActive(false);
			GameObject.DontDestroyOnLoad(component.gameObject);
		}

        public static GameObject GetGameobject(this IMechanicSystem mechanic)
        {
            if (null == mechanic)
            {
                return null;
            }
            var component = mechanic as Component;
            if (null == component)
            {
                return null;
            }
			return component.gameObject;
        }

		public static IMechanicSpawner GetMechanicSpawner(this IMechanicSystem mechanic)
		{
			if (null == mechanic)
			{
				return null;
			}

			var spawner = mechanic as IMechanicSpawner;
			if (null != spawner)
			{
				return spawner;
			}

			var go = mechanic.GetGameobject();
			if (null == go)
			{
				return null;
			}

			var parent = go.transform.parent;
			if (null == parent)
			{
				return null;
			}
			return parent.gameObject.GetComponent<IMechanicSpawner>();
		}

		public static IMechanicSystem GetMechanicSpawnerAsMechanicSystem(this IMechanicSystem mechanic)
		{
			return GetMechanicSpawner(mechanic) as IMechanicSystem;
		}
    }
}
