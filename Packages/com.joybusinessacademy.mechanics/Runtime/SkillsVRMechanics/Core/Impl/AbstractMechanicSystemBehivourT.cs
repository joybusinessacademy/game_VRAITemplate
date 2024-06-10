using UnityEngine;
using SkillsVR.Mechanic.Core.Impl;
using System;
using SkillsVR.Mechanic.Events;

namespace SkillsVR.Mechanic.Core.Impl
{
    [RegisterMechanicEvent(enumEventType = typeof(MechSysMonoEvents))]
    public abstract class AbstractMechanicSystemBehivour<DATA_TYPE> : MonoBehaviour, IMechanicSystem<DATA_TYPE>
    {
        protected abstract void OnReceiveEvent(IMechanicSystemEvent systemEvent);

        public MechanicEventsHandler mechanicEvents = new MechanicEventsHandler();

        public bool active { get; protected set; }

        protected MechanicSystemEventDelegate mechanicSystemEvent;

        public bool visualState { get; protected set; }
        public bool enableUpdateEvent { get; set; }

        [SerializeField]
        private DATA_TYPE _mechanicData;
        public virtual DATA_TYPE mechanicData
        {
            get => _mechanicData;
            set => _mechanicData = value;
        }

        public Component component => this;

        private bool gameObjectActive = false;

        public virtual void AddListerner(MechanicSystemEventDelegate listener)
        {
            mechanicSystemEvent += listener;
        }

        public virtual void RemoveListener(MechanicSystemEventDelegate listener)
        {
            mechanicSystemEvent -= listener;
        }

        public virtual void SetActive(bool isActive)
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


        public void StartMechanic()
        {
            if (active)
            {
                return;
            }
            Reset();
            SetMechanicData();
            TriggerEvent(MechSysEvent.BeforeStart);
            active = true;
            TriggerEvent(MechSysEvent.OnStart);
            SetVisualState(true);
            TriggerEvent(MechSysEvent.OnActiveStateChanged, active);
        }


        public void StopMechanic()
        {
            if (!active)
            {
                return;
            }
            TriggerEvent(MechSysEvent.BeforeStop);
            active = false;
            TriggerEvent(MechSysEvent.OnStop);
            SetVisualState(false);
            TriggerEvent(MechSysEvent.OnActiveStateChanged, active);
            TriggerEvent(MechSysEvent.AfterFullStop);
        }


        public virtual void SetMechanicData()
		{
            TriggerEvent(MechSysEvent.OnSetData);
        }

        public virtual void SetVisualState(bool show)
        {
            if (show == visualState)
            {
                return;
            }
            TriggerEvent(show ? MechSysVisualEvents.BeforeShow : MechSysVisualEvents.BeforeHide);
            visualState = show;
            TriggerEvent(show ? MechSysVisualEvents.OnShow : MechSysVisualEvents.OnHide);
            TriggerEvent(MechSysVisualEvents.OnVisualStateChanged, show);
        }

        protected virtual void Awake()
        {
            gameObjectActive = this.gameObject.activeInHierarchy;
			Reset();
            FixTMP_FontIssue();
            SetActive(false);
            SetVisualState(false);
            TriggerEvent(MechSysMonoEvents.OnAwake);
            TriggerEvent(MechSysMonoEvents.OnGameObjectActiveChanged, gameObjectActive);
        }

        void FixTMP_FontIssue()
        {
            var items = GetComponentsInChildren<TMPro.TextMeshProUGUI>();
            foreach(var item in items)
            {
                try
                {
                    string fontName = item.font.name;
                    var resFont = Resources.Load<TMPro.TMP_FontAsset>(fontName + ".asset");
                    item.font = resFont;
                }
                catch(Exception e)
                {
                    Debug.LogError("Cannot auto fix TMP Font issue for " + item.gameObject.GetPathString() + ": " + e.Message);
                }
            }
        }
        protected virtual void Start()
        {
            TriggerEvent(MechSysMonoEvents.OnStart);
        }
        protected virtual void OnDestroy()
        {
            TriggerEvent(MechSysMonoEvents.OnDestroy);
        }

        protected virtual void OnEnable()
        {
            TriggerEvent(MechSysMonoEvents.OnEnable);
            CheckGameObjectActive();
		}

        protected virtual void OnDisable()
        {
            TriggerEvent(MechSysMonoEvents.OnDisable);
			CheckGameObjectActive();
		}

		protected virtual void CheckGameObjectActive()
		{
			if (this.gameObject.activeInHierarchy == gameObjectActive)
			{
				return;
			}
			gameObjectActive = this.gameObject.activeInHierarchy;
			TriggerEvent(MechSysMonoEvents.OnGameObjectActiveChanged, gameObjectActive);
		}


		protected virtual void Update()
        {
            if (enableUpdateEvent)
            {
                TriggerEvent(MechSysMonoEvents.OnUpdate);
            }
        }

        protected virtual void LateUpdate()
        {
            if (enableUpdateEvent)
            {
                TriggerEvent(MechSysMonoEvents.OnLateUpdate);
            }
        }

        public virtual void Reset()
        {
            StopMechanic();
            SetActive(false);
            SetVisualState(false);
            if (null == mechanicEvents)
            {
                mechanicEvents = new MechanicEventsHandler();
            }
            mechanicEvents.mechanicType = this.GetType();
            TriggerEvent(MechSysEvent.OnReset);
        }

        public virtual IMechanicSystemEvent CreateEventMessage(object eventType, object data = null, params object[] args)
        {
            return new MechanicSystemEvent(this, eventType, data, args);
        }

        public virtual void TriggerEvent(IMechanicSystemEvent systemEvent)
        {
            if (null == systemEvent)
            {
                return;
            }
            OnReceiveEvent(systemEvent);
            mechanicSystemEvent?.Invoke(systemEvent);
            mechanicEvents?.Invoke(systemEvent);
        }

        public virtual void TriggerEvent(object eventType, object data = null, params object[] args)
        {
            var msg = CreateEventMessage(eventType, data, args);
            TriggerEvent(msg);
        }
       
        public override string ToString()
        {
            return GetString("{p}.{t} <{dt}>");
        }



        /// <summary>
        /// Get string with format.
        /// </summary>
        /// <param name="format">output string format, {t} - type, {dt} - data type, {p} - game object path</param>
        /// <returns></returns>
        public virtual string GetString(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return ToString();
            }
            return format
                .Replace("{t}", this.GetTypeName())
                .Replace("{dt}", typeof(DATA_TYPE).Name)
                .Replace("{p}", this.gameObject.GetPathString());
        }

    }
}
