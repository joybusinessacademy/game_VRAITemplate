using SkillsVR.Impl;
using SkillsVR.Mechanic.Core.Impl;
using SkillsVR.Mechanic.Events;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using SkillsVR.UnityExtenstion;

#if UNITY_EDITOR
using UnityEditor;
#endif

using SkillsVR.OdinPlaceholder;
using VRMechanics;

namespace SkillsVR.Mechanic.Core
{
    [RegisterMechanicEvent(enumEventType = typeof(MechSysSpawnStateEvent))]
    [RegisterMechanicEvent(enumEventType = typeof(MechSysMonoEvents))]
    public abstract class AbstractMechanicSpawner<INTERFACE_TYPE, DATA_TYPE> : MonoBehaviour
        , IMechanicSpawner, IMechanicSystem<DATA_TYPE>
        where INTERFACE_TYPE : IMechanicSystem<DATA_TYPE>
    {
        protected abstract void OnReceiveEvent(IMechanicSystemEvent systemEvent);
        
        public abstract string mechanicKey { get; }

        [SerializeField]
        public string presetKey;

        public MechanicEventsHandler mechanicEvents = new MechanicEventsHandler();

        [HideInInspector]
        // Not used for now.
        public string instanceId;

        public bool spawnOnAwake = true;
        public bool startMechanicWhenReady = false;

        public bool ready => null != targetSystem;
        public bool throwUsageWhenNotReady = false;

        protected INTERFACE_TYPE targetSystem;

        public IMechanicAssetModule assetModule;

        [SerializeField]
        [Tooltip("The parent game object that spawned system attached to")]
        protected GameObject instanceRootObject;

        private bool isGameOnline;

        private IEnumerator CheckInternetConnection(Action<bool> action)
        {
            yield return new WaitForSeconds(1);

            WWW www = new WWW("http://google.com");
            yield return www;
            if (www.error != null)
            {
                action(false);
            }
            else
            {
                action(true);
            }
        }

        public void SetThrowUsageWhenNotReady(bool throwException)
        {
            throwUsageWhenNotReady = throwException;
        }

        #region Spawn System & Asset Load
        protected virtual void LoadAsset<T>(string key, Action<T> callback, params object[] optionalArgs) where T : UnityEngine.Object
        {
            TriggerEvent(MechSysSpawnStateEvent.StartLoadAsset, key, typeof(T), key, callback, optionalArgs);

			T loadedAsset = Resources.Load<T>(key);

			if (loadedAsset != null)
			{
                callback?.Invoke(loadedAsset);

				TriggerEvent(MechSysSpawnStateEvent.FinishLoadAsset, loadedAsset);
			}
			else
				TriggerEvent(MechSysSpawnStateEvent.Error);

		}

        protected virtual void SpawnMechanic<T>(string key, string name, Action<IMechanicSystemResult> callback, params object[] optionalArgs)
            where T : IMechanicSystem
        {
            TriggerEvent(MechSysSpawnStateEvent.StartLoadMechanic, key, typeof(T), key, name, callback, optionalArgs);
            if (string.IsNullOrWhiteSpace(key))
            {
                var resultMessage = new MechanicSystemResult(new Exception("SpawnMechanic key cannot be null or empty."), this, 1, nameof(SpawnMechanic));
                resultMessage.SetArgs(typeof(T), key, callback, optionalArgs);
                callback?.Invoke(resultMessage);
                TriggerEvent(MechSysSpawnStateEvent.FiniahLoadMechanic, resultMessage);
                TriggerEvent(MechSysSpawnStateEvent.Error, resultMessage);
                return;
            }

			GameObject mechanicFromResources = Resources.Load<GameObject>(key);

            if (mechanicFromResources != null)
            {
                GameObject spawnedObject = Instantiate(mechanicFromResources);

				if (spawnedObject == null)
					Debug.LogError("Spawning Mechanics Object went wrong - Can not find prefab");

				spawnedObject.name = key;

				TriggerEvent(MechSysSpawnStateEvent.FinishLoadAsset, spawnedObject);

                IMechanicSystem mechanicSystem = spawnedObject.GetComponent<IMechanicSystem>();

                if(mechanicSystem != null)
				    targetSystem = (INTERFACE_TYPE)mechanicSystem;

                SetTargetSystemData();

				TriggerEvent(MechSysSpawnStateEvent.Ready);
				
                if (startMechanicWhenReady)
                {
					SetActive(true);
                }
			}
			else
				TriggerEvent(MechSysSpawnStateEvent.Error);
		}

        private void SetTargetSystemData()
        {
			if (null == targetSystem)
			{
				TriggerEvent(MechSysSpawnStateEvent.Error);
				return;
			}

			targetSystem.enableUpdateEvent = cachedEnableUpdateEvent;
			targetSystem.component.transform.SetParent(instanceRootObject);
			targetSystem.AddListerner(OnReceiveInnerEvent);
			targetSystem.mechanicData = data;
		}

        protected void SpawnTargetMechanic()
        {
            SpawnMechanic<INTERFACE_TYPE>(string.IsNullOrWhiteSpace(presetKey) ? mechanicKey : presetKey, instanceId, OnLoadTargetSystemResult);
        }

        protected virtual void OnLoadTargetSystemResult(IMechanicSystemResult result)
        {
            if (!result.success)
            {
                return;
            }
            if (null == result.resultObject)
            {
                return;
            }
            if (result.resultObject is INTERFACE_TYPE)
            {
                targetSystem = (INTERFACE_TYPE)result.resultObject;
            }
            if (null == targetSystem)
            {
                var errorResult = new MechanicSystemResult(new NullReferenceException(nameof(targetSystem)),
                    result.resultObject, this, result.success ? 1 : result.code, result.message);
                errorResult.CopyArgs(result);
                TriggerEvent(MechSysSpawnStateEvent.Error, errorResult);
                return;
            }
            targetSystem.enableUpdateEvent = cachedEnableUpdateEvent;
            targetSystem.component.transform.SetParent(instanceRootObject);
            targetSystem.AddListerner(OnReceiveInnerEvent);
            targetSystem.mechanicData = data;
            TriggerEvent(MechSysSpawnStateEvent.Ready, result);
            if (startMechanicWhenReady)
            {
                SetActive(true);
            }
        }

        #endregion Spawn System & Asset Load

        #region MonoBehaviour Interfaces
        protected virtual void Awake()
        {
            instanceRootObject = null == instanceRootObject ? this.gameObject : instanceRootObject;

            TriggerEvent(MechSysSpawnStateEvent.Idle);
	    
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                isGameOnline = false;

                if (spawnOnAwake)
                {
                    SpawnTargetMechanic();
                }
                return;
            }
#endif
            StartCoroutine(CheckInternetConnection((isConnected) =>
            {
                isGameOnline = isConnected;

                if (spawnOnAwake)
                {
                    SpawnTargetMechanic();
                }

            }));
        }

        protected virtual void OnDestroy()
        {
            if (null != targetSystem)
            {
                targetSystem.RemoveListener(OnReceiveInnerEvent);
            }
        }
        # endregion MonoBehaviour Interfaces

        #region IMechanicSystem & IMechanicSystem<T> Interfaces
        public bool active => null == targetSystem ? false : targetSystem.active;

        public bool visualState => null == targetSystem ? false : targetSystem.visualState;

        private bool cachedEnableUpdateEvent = false;
        public bool enableUpdateEvent
        {
            get => null == targetSystem ? cachedEnableUpdateEvent : targetSystem.visualState;
            set
            {
                if (null != targetSystem)
                {
                    targetSystem.enableUpdateEvent = value;
                }
                else
                {
                    cachedEnableUpdateEvent = value;
                    LogNotReady(nameof(enableUpdateEvent));
                }
            }
        }

        [SerializeField]
        private DATA_TYPE data;
        public virtual DATA_TYPE mechanicData
        {
            get
            {
                if (null == targetSystem || !(targetSystem is IMechanicSystem<DATA_TYPE>))
                {
                    return data;
                }
                return ((IMechanicSystem<DATA_TYPE>)targetSystem).mechanicData;
            }
            set
            {
                data = value;
                if (null != targetSystem && (targetSystem is IMechanicSystem<DATA_TYPE>))
                {
                    ((IMechanicSystem<DATA_TYPE>)targetSystem).mechanicData = data;
                }
            }
        }

        public Component component => null == targetSystem ? null : targetSystem.component;

        protected MechanicSystemEventDelegate mechanicSystemEvent;

        public virtual void AddListerner(MechanicSystemEventDelegate listener)
        {
            mechanicSystemEvent += listener;
        }

        public virtual void RemoveListener(MechanicSystemEventDelegate listener)
        {
            mechanicSystemEvent -= listener;
        }

        public virtual IMechanicSystemEvent CreateEventMessage(object eventType, object data = null, params object[] args)
        {
            if (null == targetSystem)
            {
                return new MechanicSystemEvent(this, eventType, data, args);
            }
            else
            {
                var msg = targetSystem.CreateEventMessage(eventType, data, args);
                msg.ResetSender(this);
                return msg;
            }
        }

        public virtual void Reset()
        {
            targetSystem?.Reset();
            if (null != mechanicEvents)
            {
                mechanicEvents.mechanicType = this.GetType();
            }
            if (string.IsNullOrWhiteSpace(presetKey))
            {
                presetKey = mechanicKey;
            }


    }

    public virtual void SetActive(bool isActive)
        {
            if (null == targetSystem)
            {
                LogNotReady(nameof(SetActive));
            }
            else
            {
                if (isActive)
                {
                    StartMechanic();
                }
                else
                {
                    StopMechanic();
                }
            }
        }

        public virtual void SetVisualState(bool show)
        {
            LogIfNotReady(nameof(SetVisualState));
            targetSystem?.SetVisualState(show);
        }

        public virtual void StartMechanic()
        {
            if (null == targetSystem)
            {
                LogNotReady(nameof(StartMechanic) + "(" + typeof(DATA_TYPE).Name + ")");
                return;
            }
            else
            {
                targetSystem.StartMechanic();
            }
        }

        public virtual void StopMechanic()
        {
            if (null == targetSystem)
            {
                LogNotReady(nameof(StopMechanic) + "(" + typeof(DATA_TYPE).Name + ")");
                return;
            }
            else
            {
                targetSystem.StopMechanic();
            }
        }

        public virtual void TriggerEvent(object eventType, object data = null, params object[] args)
        {
            if (null == targetSystem)
            {
                var msg = CreateEventMessage(eventType, data, args);
                TriggerEvent(msg);
            }
            else
            {
                targetSystem.TriggerEvent(eventType, data, args);
            }
        }

        public virtual void TriggerEvent(IMechanicSystemEvent systemEvent)
        {
            if (null == systemEvent)
            {
                return;
            }
            if (null == targetSystem)
            {
                OnReceiveInnerEvent(systemEvent);
            }
            else
            {
                targetSystem.TriggerEvent(systemEvent);
            }
        }

        protected void OnReceiveInnerEvent(IMechanicSystemEvent systemEvent)
        {
            if (null == systemEvent)
            {
                return;
            }
            OnReceiveEvent(systemEvent);
            mechanicSystemEvent?.Invoke(systemEvent);
            mechanicEvents?.Invoke(systemEvent);
        }
        #endregion IMechanicSystem & IMechanicSystem<T> Interfaces

        protected void LogIfNotReady(string methodName, bool throwException = false)
        {
            if (null == targetSystem)
            {
                LogNotReady(methodName);
                if (throwUsageWhenNotReady || throwException)
                {
                    throw new Exception(GetNotReadyString(methodName));
                }
            }
        }

        protected void LogNotReady(string methodName)
        {
            Debug.LogError(GetNotReadyString(methodName));
        }

        protected string GetNotReadyString(string methodName)
        {
            return string.Join(" ", "Spawner method [", methodName, "] called before mechanic system ready.",
                "\r\n", this,
                "\r\n\r\nTo Solve this, register and listen", nameof(MechSysSpawnStateEvent.Ready), "event before use",
                "\r\nTo register", nameof(MechSysSpawnStateEvent.Ready), "event",
                "\r\nIf not spawner:",
                "\r\n   ", nameof(AddListerner), "(YourCallBackMethod);",
                "\r\n   ", "void YourCallBackMethod(IMechanicSystemEvent eventArgs)",
                "\r\n   ", "{",
                "\r\n       ", "switch(eventArgs.eventKey)",
                "\r\n       ", "{",
                "\r\n           ", "case MechSysSpawnStateEvent.Ready: SetReadyToUse(); break;",
                "\r\n       ", "}",
                "\r\n   ", "}",
                "\r\n   ", "",
                "\r\nIf is spawner",
                "\r\n   ", "protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)",
                "\r\n   ", "{",
                "\r\n       ", "switch(systemEvent.eventKey)",
                "\r\n       ", "{",
                "\r\n           ", "case MechSysSpawnStateEvent.Ready: SetReadyToUse(); break;",
                "\r\n       ", "}",
                "\r\n   ", "}",
                ""
                );
        }

        public override string ToString()
        {
            return GetString("{p}.{t} <{st}, {dt}>");
        }

        /// <summary>
        /// Get string with format.
        /// </summary>
        /// <param name="format">output string format, 
        /// {t} - type, 
        /// {dt} - data type, 
        /// {st} - target system interface type
        /// {p} - game object path</param>
        /// <returns></returns>
        public virtual string GetString(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return ToString();
            }
            return format
                .Replace("{t}", (this).GetTypeName())
                .Replace("{st}", typeof(INTERFACE_TYPE).Name)
                .Replace("{dt}", typeof(DATA_TYPE).Name)
                .Replace("{p}", this.gameObject.GetPathString());
        }

#if UNITY_EDITOR

        [Button]
        public void GenerateScriptable()
        {
            if (mechanicData != null)
                return;

            ScriptableObject asset = ScriptableObject.CreateInstance(typeof(DATA_TYPE));

            string assetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/" + typeof(DATA_TYPE).Name + ".asset");
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            DATA_TYPE result = (DATA_TYPE)Convert.ChangeType(asset, typeof(DATA_TYPE));

            mechanicData = result;

            return;
        }

#endif

    }
}
