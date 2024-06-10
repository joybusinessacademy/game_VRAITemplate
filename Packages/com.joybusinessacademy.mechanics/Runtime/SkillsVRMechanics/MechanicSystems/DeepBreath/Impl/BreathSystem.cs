using UnityEngine;
using SkillsVR.Mechanic.Core.Impl;
using SkillsVR.Mechanic.Core;
using System.Collections.Generic;
using SkillsVR.UnityExtenstion;

namespace SkillsVR.Mechanic.MechanicSystems.DeepBreath.Impl
{

    public class BreathSystem : AbstractMechanicSystemBehivour<DeepBreathData>, IDeepBreathSystem
    {
        [SerializeField]
        protected DeepBreathUI breathUI;

        [SerializeField]
        protected List<GameObject> styleGameObjects = new List<GameObject>();

        public int fullBreathCount => null == breathUI ? 0 : breathUI.deepBreathCount;

        protected bool lockBreath = false;
        protected float timeoutTimer;

        protected List<ITelemetry> telemetries = new List<ITelemetry>();
        protected TimeElpasedTelemetry firstBreathStartTimeElpasedTelemetry;
        protected TrueFalseTelemetry successBreathTelemetry = new TrueFalseTelemetry();

        protected bool trackFirstBreath = false;

        protected MechanicTweenSystem tweenSystem = new MechanicTweenSystem();

        protected Coroutine tweeningCoroutine;
        protected Coroutine fadingCoroutine;

        [Header("Animation Items")]
        public float speedOfTween = 0.3f;
        public float fadeObjectsTime = 0.3f;

        public void StartCheckBreath(float duration = 6, float timeout = -1, bool autoHideAfterSuccess = true, bool autoBreathOut = true)
        {
            if (null == mechanicData)
            {
                mechanicData = new DeepBreathData();
            }
            mechanicData.duration = duration;
            mechanicData.timeout = timeout;
            mechanicData.autoHideAfterSuccess = autoHideAfterSuccess;
            mechanicData.autoBrathOut = autoBreathOut;
            StartMechanic();
        }

        public void StopCheckBreath()
        {
            StopMechanic();
        }

        protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
        {
            if (null == systemEvent)
            {
                return;
            }
            switch(systemEvent.eventKey)
            {
                case MechSysEvent.BeforeStart: BeforeStart(); break;
                case DeepBreathEvent.BreathOutMax: OnFinishBreath(); break;
                case MechSysEvent.OnStop: OnStop(); break;
            }
        }

        protected void SetupTelemetry()
        {
            firstBreathStartTimeElpasedTelemetry = this.transform.GetOrAddComponent<TimeElpasedTelemetry>();
            telemetries.Add(firstBreathStartTimeElpasedTelemetry);
            telemetries.Add(successBreathTelemetry);

            firstBreathStartTimeElpasedTelemetry.id = "Deep Breath - First Breath Start Time : " + this.gameObject.GetInstanceID().ToString();
            successBreathTelemetry.id = "Deep Breath - Success : " + this.gameObject.GetInstanceID().ToString();
        }

        protected void UpdateTelemetry()
        {
            foreach (var telemetry in telemetries)
            {
                if (!telemetry.isCompleted && telemetry.IsValidated())
                {
                    telemetry.SendEvents();
                }
            }
        }
        protected override void Awake()
        {
            SetupTelemetry();
            base.Awake();
            RegisterUIEvent();
            UpdateStyle();
        }

        protected override void OnDestroy()
        {
            UnRegisterUIEvent();
            base.OnDestroy();
        }

        protected override void Update()
        {
            base.Update();
            if (active && null != mechanicData && mechanicData.timeout > 0.0f)
            {
                if (timeoutTimer >= mechanicData.timeout)
                {
                    TriggerEvent(DeepBreathEvent.Timeout, timeoutTimer);
                    StopMechanic();
                    return;
                }
                else
                {
                    timeoutTimer += Time.deltaTime;
                }
            }
             
            UpdateTelemetry();
        }

        private void UpdateStyle()
        {
            // switch style
            styleGameObjects.ForEach(k => k.SetActive((styleGameObjects.IndexOf(k)) == mechanicData.style));

            // style 2, remap style 2 timeout to system timeout
            if (mechanicData.style == 1)
            {
                mechanicData.timeout = mechanicData.style2Timeout;
            }
#if UNITY_EDITOR
            else
			{
                //Auto Timeout on Breathing Style 1
                mechanicData.timeout = 10;
			}
#endif
        }

        public override void SetVisualState(bool show)
        {
            base.SetVisualState(show);
            

            if (tweeningCoroutine != null)
            {
                StopCoroutine(tweeningCoroutine);
                tweeningCoroutine = null;
            }

            if (fadingCoroutine != null)
            {
                StopCoroutine(fadingCoroutine);
                fadingCoroutine = null;
            }

            if (show)
            {
                breathUI?.gameObject.SetActive(show);
                UpdateStyle();
            }            

            if (null != breathUI)
            {
                tweeningCoroutine = StartCoroutine(tweenSystem.ScaleUpOrDown(breathUI.transform, speedOfTween, show));
                fadingCoroutine = StartCoroutine(tweenSystem.FadeCanvas(breathUI.GetOrAddComponent<CanvasGroup>(), show, fadeObjectsTime));
            }
            
        }

        protected void BeforeStart()
        {
            
            successBreathTelemetry.hasResult = false;
            successBreathTelemetry.isCompleted = false;

            if(firstBreathStartTimeElpasedTelemetry)
            {
				firstBreathStartTimeElpasedTelemetry.timeElapsed = 0.0f;
				firstBreathStartTimeElpasedTelemetry.checkTimeElapsed = false;
				firstBreathStartTimeElpasedTelemetry.isCompleted = false;
				firstBreathStartTimeElpasedTelemetry.startCheckingTime = true;
			}

            trackFirstBreath = true;

            SetStartDataToUI(); 
        }

        protected void OnStop()
        {
            if (!successBreathTelemetry.isCompleted)
            {
                successBreathTelemetry.result = false;
                successBreathTelemetry.SendEvents();
            }
            if (!firstBreathStartTimeElpasedTelemetry.isCompleted)
            {
                firstBreathStartTimeElpasedTelemetry.checkTimeElapsed = true;
                firstBreathStartTimeElpasedTelemetry.SendEvents();
            }
        }

        protected void SetStartDataToUI()
        {
            if (null == mechanicData)
            {
                return;
            }
            float duration = mechanicData.duration * 0.5f;
            breathUI?.SetDuration(duration);
        }

        public override void Reset()
        {
            base.Reset();
            lockBreath = false;
			timeoutTimer = 0.0f;
            trackFirstBreath = false;
			breathUI?.Reset();
        }

        protected void RegisterUIEvent()
        {
            breathUI?.OnMaxScaleReached.AddListener(OnBreathInMax);
            breathUI?.OnMinScaleReached.AddListener(OnBreathOutMax);
        }

        protected void UnRegisterUIEvent()
        {
            breathUI?.OnMaxScaleReached.RemoveListener(OnBreathInMax);
            breathUI?.OnMinScaleReached.RemoveListener(OnBreathOutMax);
        }

        protected void OnBreathInMax(float maxScale)
        {
            TriggerEvent(DeepBreathEvent.BreathInMax, maxScale);
            if (null != mechanicData && mechanicData.autoBrathOut)
            {
                ActiveBreath(false);
                lockBreath = true;
            }
        }

        protected void OnBreathOutMax(float maxScale)
        {
            lockBreath = false;
            TriggerEvent(DeepBreathEvent.BreathOutMax, maxScale);
        }

        protected void OnFinishBreath()
        {
            successBreathTelemetry.result = true;
            if (fullBreathCount > 0 && null != mechanicData && mechanicData.autoHideAfterSuccess)
            {
                StopMechanic();
            }
        }

        public void ActiveBreath(bool active)
        {
            if (lockBreath)
            {
                return;
            }
            if (active && trackFirstBreath)
            {
                trackFirstBreath = false;
                firstBreathStartTimeElpasedTelemetry.checkTimeElapsed = true;
            }
            breathUI?.TriggerScaleUp(active);
            TriggerEvent(active ? DeepBreathEvent.BreathIn : DeepBreathEvent.BreathOut, breathUI.currScale);
            TriggerEvent(DeepBreathEvent.BreathStateChanged, active);
        }
    }
}
