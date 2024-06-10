using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SkillsVR.Mechanic.MechanicSystems.DeepBreath
{
    public class DeepBreathUI : MonoBehaviour
    {
        [Serializable]
        public class UnityEventFloat : UnityEvent<float>{}

        public const int SCALE_UP_DIR = 1;
        public const int SCALE_DOWN_DIR = -1;

        public float scaleSpeed = 1.0f;
        public float minScale = 0.2f;
        public float maxScale = 1.0f;
        [Range(SCALE_DOWN_DIR, SCALE_UP_DIR)]
        public int scaleDir;

        public int deepBreathCount { get; protected set; } = 0;
        public UnityEngine.UI.Image heartImage;

        private bool haveReachMax = false;
        private float _currScale;
        public float currScale
        {
            get
            {
                return _currScale;
            }
            set
            {
                float scale = Mathf.Clamp(value, minScale, maxScale);
                if (scale == _currScale)
                {
                    return;
                }
                _currScale = scale;
                SetObjectsToScale(_currScale);
                if (maxScale == _currScale)
                {
                    OnMaxScaleReached?.Invoke(maxScale);
                    haveReachMax = true;
                }
                else if (minScale == _currScale)
                {
                    deepBreathCount += haveReachMax ? 1 : 0;
                    haveReachMax = false;
                    OnMinScaleReached?.Invoke(minScale);
                }
            }
        }

        public UnityEventFloat OnMaxScaleReached = new UnityEventFloat();
        public UnityEventFloat OnMinScaleReached = new UnityEventFloat();

        public List<Transform> scaleTargetList = new List<Transform>();
        public List<BreathingUIAnimation> animList = new List<BreathingUIAnimation>();
        public void SetDuration(float duration)
        {
            duration = duration < 0.001f ? 3.0f : duration;
            scaleSpeed = (maxScale - minScale) / duration;
        }

        public void Reset()
        {
            deepBreathCount = 0;
            currScale = minScale;
            haveReachMax = false;
            scaleDir = SCALE_DOWN_DIR;
            SetObjectsToScale(minScale);
        }

        private void OnEnable()
        {
            Reset();
        }

        private void AnimateHeart()
        {
            StopAllCoroutines();
            StartCoroutine(PlayHeartAnimCoroutine(1.1f, 0.1f));
        }


        private IEnumerator PlayHeartAnimCoroutine(float scale, float duration)
        {
            if (null == heartImage)
            {
                yield break;
            }
            var oriScale = heartImage.transform.localScale;
            duration = Mathf.Max(0.001f, duration);
            float time = 0.0f;
            float factor = 0.0f;
            while (time < duration)
            {
                factor = Mathf.Clamp01(time / duration);
                heartImage.transform.localScale = Vector3.Lerp(oriScale, oriScale * scale, factor);
                yield return null;
                time += Time.deltaTime;
            }
            time = 0.0f;
            factor = 0.0f;
            while (time < duration)
            {
                factor = Mathf.Clamp01(time / duration);
                heartImage.transform.localScale = Vector3.Lerp(oriScale * scale, oriScale, factor);
                yield return null;
                time += Time.deltaTime;
            }
        }

        private void Update()
        {
            currScale += scaleDir * scaleSpeed * Time.deltaTime;

        }

        public void TriggerScaleUp(bool trigger)
        {
            scaleDir = trigger ? SCALE_UP_DIR : SCALE_DOWN_DIR;
        }

        private void SetObjectsToScale(float scale)
        {
            foreach (var item in scaleTargetList)
            {
                if (null == item)
                {
                    continue;
                }
                item.localScale = Vector3.one * scale;
            }
            foreach(var item in animList)
            {
                if (null == item)
                {
                    continue;
                }
                item.targetScale = scale;
            }
        }
    }
}
