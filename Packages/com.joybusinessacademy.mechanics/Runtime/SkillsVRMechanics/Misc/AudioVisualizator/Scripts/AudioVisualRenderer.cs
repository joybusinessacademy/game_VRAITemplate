using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillsVR.Mechanic.Audio.Visualizator
{
    public class AudioVisualRenderer : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float minValue = 0.05f;
        [Range(0.0f, 1.0f)]
        public float maxValue = 1.0f;

        public List<Transform> managedVisualItems = new List<Transform>();
        public bool autoGenerateVisualItems = true;
        public Transform visualItemTemplate;

        public void OnReceiveData(float[] data)
        {
            if (null == data)
            {
                return;
            }

            if (autoGenerateVisualItems && managedVisualItems.Count != data.Length)
            {
                ResetItems(data.Length);
            }

            int minLen = data.Length > managedVisualItems.Count ? managedVisualItems.Count : data.Length;
            for (int i = 0; i < minLen; i++)
            {
                var item = managedVisualItems[i];
                var value = Mathf.Clamp(data[i], minValue, maxValue);
                var setter = item.GetComponent<IAudioVisualizatorValueSetter>();
                if (null == setter)
                {
                    var itemScale = item.localScale;
                    itemScale.y = value;
                    item.localScale = itemScale;
                }
                else
                {
                    setter.SetAudioVisualizatorValue(value);
                }
            }
        }

        void ResetItems(int count)
        {
            foreach (var item in managedVisualItems)
            {
                if (null == item)
                {
                    continue;
                }
                GameObject.Destroy(item.gameObject);
            }

            managedVisualItems.Clear();

            if (null == visualItemTemplate)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                var item = GameObject.Instantiate<Transform>(visualItemTemplate, visualItemTemplate.parent, false);
                item.gameObject.SetActive(true);
                managedVisualItems.Add(item);
            }
        }
    }
}
