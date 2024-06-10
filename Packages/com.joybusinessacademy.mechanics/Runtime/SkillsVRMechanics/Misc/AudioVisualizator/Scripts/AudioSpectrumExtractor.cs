using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SkillsVR.Mechanic.Core;

namespace SkillsVR.Mechanic.Audio.Visualizator
{

    [Serializable]
    public class AudioSpectrumSourceData
    {
        public bool useAudioSource = false;
        public AudioSource audioSource;
        [Range(6, 13)]
        public int sampleCountPowerOfTwo = 6;
        public FFTWindow fFTWindow = FFTWindow.BlackmanHarris;
        [Tooltip("How many seconds wait between each update. Larger value for higher performance. 0 for update every frame.")]
        public float minUpdateInterval = 0.2f;
        public bool autoPlay = true;
    }

    [Serializable]
    public class AudioSpectrumDataFilterSettings
    {
        [Tooltip("Scale of raw spectrum data.")]
        public float scale = 1.0f;

        [Tooltip("Size of output buff.")]
        public int buffSize = 32;

        [Tooltip("Max of filtered value. Final value = Mathf.Clamp01(rawSpectrumData * scale / max)")]
        public float normalizeMax = 1.0f;

        [Tooltip("Filter range start of raw samples. Samples before this percentage point will be cut off.")]
        [Range(0.0f, 1.0f)]
        public float startRange = 0.0f;
        [Tooltip("Filter range end of raw samples. Samples after this percentage point will be cut off.")]
        [Range(0.0f, 1.0f)]
        public float endRange = 1.0f;
    }


    [Serializable]
    public class AudioVisualizatorData
    {
        public AudioSpectrumSourceData input = new AudioSpectrumSourceData();
        public AudioSpectrumDataFilterSettings filter = new AudioSpectrumDataFilterSettings();
        public float[] rawSpectrumData;
        public float[] filteredData;
    }


    public class AudioSpectrumExtractor : MonoBehaviour
    {
        public AudioVisualizatorData config;

        protected float updateTimer;

        [Serializable]
        public class AudioDataEvent : UnityEngine.Events.UnityEvent<float[]> { }
        public AudioDataEvent onFilteredData = new AudioDataEvent();

        /*
        public Meta.WitAi.Data.AudioBuffer audioBuffer;
        void Start()
        {
            audioBuffer.Events.on
        }*/
        // Update is called once per frame
        protected void Update()
        {
            if (null == config)
            {
                config = new AudioVisualizatorData();
            }

            
            if (updateTimer < config.input.minUpdateInterval)
            {
                updateTimer += Time.deltaTime;
                return;
            }
            updateTimer = 0.0f;
            
            

            if (config.input.useAudioSource && null != config.input.audioSource)
            {
                GetData(ref config.rawSpectrumData, config.input);
                ProcessDataBuff(config.rawSpectrumData);
            }
        }

        public void OnReceiveAudioDataBuff(IMechanicSystemEvent mechanicSystemEvent)
        {
            if (null == mechanicSystemEvent || null == mechanicSystemEvent.eventKey)
            {
                return;
            }

            ProcessDataBuff(mechanicSystemEvent.GetData<float[]>());
        }

        public void InternalUpdate(float[] samples)
        {
            ProcessDataBuff(samples);
        }

        public void ProcessDataBuff(float[] dataBuff, float exScale = 1.0f)
        {
            if (null == dataBuff)
            {
                return;
            }
            SetupRawSpectrumBuff(config);
            SetupFilteredBuff(config);
            for(int i =0; i < dataBuff.Length; i++)
            {
                dataBuff[i] *= config.filter.scale * exScale;
            }
            config.rawSpectrumData = dataBuff;
            GetFilterData(config.filteredData, config.rawSpectrumData, config.filter);
            onFilteredData.Invoke(config.filteredData);
        }

        protected void SetupRawSpectrumBuff(AudioVisualizatorData config)
        {
            if (null == config)
            {
                return;
            }

            int smapleCountPowerOfTwo = Mathf.Clamp(config.input.sampleCountPowerOfTwo, 6, 13);
            int sampleCount = (int)Mathf.Pow(2, smapleCountPowerOfTwo);
            if (null == config.rawSpectrumData || config.rawSpectrumData.Length != sampleCount)
            {
                config.rawSpectrumData = new float[sampleCount];
            }
        }

        protected void SetupFilteredBuff(AudioVisualizatorData config)
        {
            if (null == config)
            {
                return;
            }

            int filteredDataCount = Mathf.Max(0, config.filter.buffSize);
            if (null == config.filteredData || config.filteredData.Length != filteredDataCount)
            {
                config.filteredData = new float[filteredDataCount];
            }
        }

        protected void GetData(ref float[] outputData, AudioSpectrumSourceData sourceData)
        {
            if (null == sourceData || null == sourceData.audioSource)
            {
                return;
            }

            if (sourceData.autoPlay && !sourceData.audioSource.isPlaying)
            {
                Debug.Log("I AM PLAYING NOW");
                sourceData.audioSource.Play();
            }

            sourceData.audioSource.GetSpectrumData(outputData, 0, sourceData.fFTWindow);

            float volumeNormalizeScale = 0.0f >= sourceData.audioSource.volume ? 0.0f : 1.0f / sourceData.audioSource.volume;
            for (int i = 0; i < outputData.Length; i++)
            {
                outputData[i] *= 1;
            }
        }

        protected void GetFilterData(float[] outputData, float[] rawData, AudioSpectrumDataFilterSettings settings)
        {
            if (null == settings)
            {
                return;
            }

            for (int i = 0; i < outputData.Length; i++)
            {
                outputData[i] = 0.0f;
            }

            if (null == rawData || 0 == rawData.Length || null == settings)
            {
                return;
            }

            int lastIndex = Mathf.Max(0, rawData.Length - 1);
            int startIndex = Mathf.Min(rawData.Length - 1, Mathf.RoundToInt(Mathf.Clamp01(settings.startRange) * rawData.Length));
            int endIndex = Mathf.Min(rawData.Length - 1, Mathf.RoundToInt(Mathf.Clamp01(settings.endRange) * rawData.Length));

            startIndex = endIndex > startIndex ? startIndex : endIndex;
            endIndex = endIndex > startIndex ? endIndex : startIndex;

            float indexDiff = (endIndex - startIndex) / (float)outputData.Length;

            settings.normalizeMax = Math.Max(0, settings.normalizeMax);

            for (int i = 0; i < outputData.Length; ++i)
            {
                int index = Mathf.Clamp(Mathf.RoundToInt(indexDiff * i) + startIndex, 0, rawData.Length - 1);
                var v = rawData[index] * settings.scale;
                v = Mathf.Min(settings.normalizeMax, v);
                v = 0 == settings.normalizeMax ? 0.0f : Mathf.Clamp01(v / settings.normalizeMax);
                outputData[i] = v;
            }
        }
    }
}
