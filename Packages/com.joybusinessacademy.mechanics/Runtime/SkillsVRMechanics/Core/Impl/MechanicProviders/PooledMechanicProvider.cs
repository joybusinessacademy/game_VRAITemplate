using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.Mechanic.Core.Impl
{
    public class PooledMechanicProvider : IPooledMechanicProvider
    {
        public List<IMechanicSystem> pool = new List<IMechanicSystem>();

        private IMechanicProvider newObjectProvider = new NewInstanceMechanicProvider();

        public bool reusePoolObject = false;


        protected List<IMechanicSystem> notReadyObjects = new List<IMechanicSystem>();

        protected event Action onAllObjectsReady;

        public T ConvertToInterface<T>() where T : class, IMechanicProvider
        {
            return this as T;
        }

        public void AddOneTimeAllObjectsReadyListener(Action callback)
        {
            if (null == callback)
            {
                return;
            }
            if (0 >= notReadyObjects.Count)
            {
                callback.Invoke();
                return;
            }

            onAllObjectsReady += callback;
        }

        public void RemoveAllObjectsReadyListener(Action callback)
        {
            if (null == callback)
            {
                return;
            }
            onAllObjectsReady -= callback;
        }

        protected void SetObjectReady(IMechanicSystem mechanic)
        {
            if (null ==  mechanic)
            {
                return;
            }
            bool success = notReadyObjects.Remove(mechanic);
            if (success && 0 >= notReadyObjects.Count)
            {
                onAllObjectsReady?.Invoke();
                onAllObjectsReady = null;
            }
        }

        public void PreparePoolObjects(Type type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = NewObject(type);
                if (null == item)
                {
                    return;
                }
                ProcessPreInitObject(item, i);
                pool.Add(item);
            }
        }

        public void PreparePoolObjects<T>(Type type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = NewObject<T>(type);
                if (null == item)
                {
                    return;
                }
                ProcessPreInitObject(item, i);
                pool.Add(item);
            }
        }

        private void ProcessPreInitObject(IMechanicSystem mechanic, int index)
        {
            if (null == mechanic)
            {
                return;
            }
            var gameObject = mechanic.GetGameobject();
            if (null != gameObject)
            {
                GameObject.DontDestroyOnLoad(gameObject);
            }

            gameObject.name += " " + index;
            var spawner = mechanic as IMechanicSpawner;
            if (null != spawner)
            {
                if (spawner.ready)
                {
                    mechanic.SendBackForReuse();
                }
                else
                {
                    notReadyObjects.Add(mechanic);
                    mechanic.AddListerner(OnMechanicReadyEvent);
                }
            }
            else
            {
                mechanic.SendBackForReuse();
            }
        }

        private void OnMechanicReadyEvent(IMechanicSystemEvent systemEvent)
        {
            switch(systemEvent.eventKey)
            {
                case MechSysSpawnStateEvent.Ready:
                    {
                        var spawner = systemEvent.sender.GetMechanicSpawnerAsMechanicSystem();
                        if (null == spawner)
                        {
                            break;
                        }
                        spawner.RemoveListener(OnMechanicReadyEvent);
                        spawner.SendBackForReuse();
                        SetObjectReady(spawner);
                        break;
                    }
                default: break;
            }
        }

        public IMechanicSystem GetMechanic(Type type)
        {
            var item = GetFromPool(type);
            if (null == item)
            {
                item = NewObject(type);
                if (null == item)
                {
                    return null;
                }
                pool.Add(item);
            }
            item.RemoveListener(OnMechanicEvent);
            item.AddListerner(OnMechanicEvent);
            return item;
        }

        public IMechanicSystem<T> GetMechanic<T>(Type type)
        {
            var item = GetFromPool(type) as IMechanicSystem<T>;
            if (null == item)
            {
                item = NewObject<T>(type);
                if (null == item)
                {
                    return null;
                }
                pool.Add(item);
            }
            item.RemoveListener(OnMechanicEvent);
            item.AddListerner(OnMechanicEvent);
            return item;
        }

        public IMechanicSystem GetFromPool(Type type)
        {
            var item = pool.Where(x => null != x && x.GetType() == type && null != x as Component && !((Component)x).gameObject.activeInHierarchy).FirstOrDefault();
            if (null != item && null != item as Component)
            {
                Component c = item as Component;
                c.gameObject.SetActive(true);
            }
            return item;
        }

        private IMechanicSystem<T> NewObject<T>(Type type)
        {
            var item = newObjectProvider.GetMechanic<T>(type);
            return item;
        }

        private IMechanicSystem NewObject(Type type)
        {
            var item = newObjectProvider.GetMechanic(type);
            return item;
        }

        private void OnMechanicEvent(IMechanicSystemEvent systemEvent)
        {
            switch(systemEvent.eventKey)
            {
                case MechSysEvent.BeforeStart:
                    if (!reusePoolObject)
                    {
                        RemoveFromPool(systemEvent.sender);
                    }
                    break;
                case MechSysEvent.AfterFullStop:
                    {
                        if(!reusePoolObject)
                        {
                            break;
                        }
                        var spawner = systemEvent.sender.GetMechanicSpawnerAsMechanicSystem();
                        var target = null == spawner ? systemEvent.sender : spawner;
                        target.SendBackForReuse();
                        break;
                    }
                default:break;
            }
        }

        public void SetReusePoolObject(bool enable)
        {
            reusePoolObject = enable;
        }

        public IMechanicSystem RemoveFromPool(IMechanicSystem mechanic)
        {
            if (null == mechanic)
            {
                return null;
            }
            bool success = false;
            var spawner = mechanic.GetMechanicSpawnerAsMechanicSystem();
            if (null != spawner)
            {
                success = pool.Remove(spawner);
                if (success)
                {
                    spawner.RemoveListener(OnMechanicEvent);
                    spawner.RemoveListener(OnMechanicReadyEvent);
                    return spawner;
                }
            }
            success = pool.Remove(mechanic);
            if (success)
            {
                mechanic.RemoveListener(OnMechanicEvent);
                mechanic.RemoveListener(OnMechanicReadyEvent);
                return mechanic;
            }
            
            return null;
        }
    }
}
