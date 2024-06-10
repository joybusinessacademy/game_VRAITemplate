using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace SkillsVR.Mechanic.Core.Impl
{
    public class MechanicPoolInitController : MonoBehaviour
    {
        const int DEFAULT_INIT_COUNT = 0;

        public bool initOnAwake = true;
        public int initCount = DEFAULT_INIT_COUNT;

        [Tooltip("Off - Finished mechanic object destroyed with scene.\r\nOn - Finished mechanic object send back to pool.")]
        public bool reusePoolObjects = false;

        [Tooltip("Max wait seconds before init process done. Disabled when value is 0 or less.")]
        public float timeout = 15.0f;

        [Tooltip("Event triggered on init complete or timeout.")]
        public UnityEvent OnInitProcessFinish = new UnityEvent();

        [Tooltip("Event triggered on init timeout.")]
        public UnityEvent OnTimeout = new UnityEvent();

        [Tooltip("Mechanic types will be ignored if its name in the exclude list.")]
        public List<string> excludeMechanicTypes = new List<string>() { "SpawnerGestureRangedVisualizer", "SpawnerGestureControlledDeepBreath\r\n" };

        [Serializable]
        public class MechanicTypeInitConfig
        {
            public string mechanicTypeName;
            public int initCount;
        }

        [Tooltip("Mechanics in list will use indivdual init count instead of general setting.")]
        public List<MechanicTypeInitConfig> typeSpecialInitCountConfigs = new List<MechanicTypeInitConfig>();

        public bool isInProcess { get; protected set; }

        private IPooledMechanicProvider pool;

        private void Awake()
        {
            if (initOnAwake)
            {
                InitPool();
            }
        }

        public void InitPool()
        {
            if (isInProcess)
            {
                Debug.Log("Pool init is already start. Please wait init complete and try again.");
                return;
            }
            PreparePool();
            if (null == pool)
            {
                return;
            }

            isInProcess = true;

            var allSpawners = pool.GetAllMechanicTypes<IMechanicSpawner>();
            string info = "";
            initCount = Math.Max(0, initCount);
            foreach (var spawnerType in allSpawners)
            {
                if (null == spawnerType)
                {
                    continue;
                }
                if (excludeMechanicTypes.Any(x=> null != x && x == spawnerType.Name))
                {
                    continue;
                }

                var cfg = typeSpecialInitCountConfigs.Find(x => spawnerType.Name == x.mechanicTypeName);
                int count = null == cfg ? initCount : Math.Max(0, cfg.initCount);
                if (0 >= count)
                {
                    continue;
                }
                pool.PreparePoolObjects(spawnerType, count);
                info += "Pre init " + count + " x " + spawnerType.Name + "\r\n";
            }
            if (!string.IsNullOrWhiteSpace(info))
            {
                Debug.Log(info);
            }
            pool.SetReusePoolObject(reusePoolObjects);

            StartCoroutine(ProcessTimeout());
            pool.AddOneTimeAllObjectsReadyListener(OnInitFinish);
        }

        private void PreparePool()
        {
            pool = MechanicProvider.Current.ConvertToInterface<IPooledMechanicProvider>();
            if (null == pool)
            {
                var chainProvider = MechanicProvider.Current.ConvertToInterface<IChainedMechanicProvider>();
                if (null == chainProvider)
                {
                    Debug.LogError("Mechani Pool is not support. MechanicProvider.Current is not IPooledMechanicProvider or IChainedMechanicProvider");
                    isInProcess = false;
                    return;
                }

                pool = new PooledMechanicProvider();
                chainProvider.providers.Insert(0, pool);
            }
        }

        private IEnumerator ProcessTimeout()
        {
            if (0.0f >= timeout)
            {
                yield break;
            }
            yield return new WaitForSeconds(timeout);
            if (!isInProcess)
            {
                yield break;
            }
            Debug.LogError("Init pool timeout");
			pool?.RemoveAllObjectsReadyListener(OnInitFinish);
            OnInitFinish();
            OnTimeout?.Invoke();
        }

        private void OnInitFinish()
        {
            isInProcess = false;
            OnInitProcessFinish?.Invoke();
        }

        public void PrintLog(string info)
        {
            Debug.Log(null == info ? "null" : info);
        }

        public void PrintErrorLog(string info)
        {
            Debug.LogError(null == info ? "null" : info);
        }

		private void OnDestroy()
		{
			if (pool == null)
				return;

			(pool as PooledMechanicProvider).pool.ForEach(i => {
				if (i != null && i.component != null)
				{
					var container = i.component.transform.parent.gameObject;
					GameObject.DestroyImmediate(container);
				}
			});

		}
	}
}
