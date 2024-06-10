using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BreathingData
{
    public RectTransform rectTransform;
    public float speed;
    public bool changeColor = false;
    public Color colourChange01 = Color.black;
    public Color colourChange02 = Color.black;
}

public class BreathingUIAnimation : MonoBehaviour
{
    public List<BreathingData> breathingDatas = new List<BreathingData>();
    public List<BreathingData> orbitDatas = new List<BreathingData>();

    // game objects references to breathing UI animation and the max scale when breathing in
    [SerializeField]
    private RectTransform breathingUI;

    // section that determines breathing durations
    float breathInLength = 1.0f;

    [SerializeField] 
    private AnimationCurve breathInCurve;

    float breathOutLength = 1.0f;

    [SerializeField] 
    private AnimationCurve breathOutCurve;

    // timing
    [Range(0.0f, 1.0f)]
    public float targetScale;

    // Update is called once per frame
    void Update()
    {
        breathingDatas.ForEach(x => PlayBreathing(x.rectTransform, x.speed, x.changeColor));
        orbitDatas.ForEach(x => PlayOrbit(x.rectTransform, x.speed, x.changeColor));
    }

    void PlayBreathing(RectTransform ui, float uiScale, bool doColorLogic = false)
    {
        float scale = 1.0f - targetScale;
        float breathInPercentage = scale / breathInLength;
        float breathOutPercentage = (scale - breathInLength) / breathOutLength;

        if (scale < breathInLength)
        {
            ui.transform.localScale = Vector3.Lerp(Vector3.one * uiScale, Vector3.one, breathOutCurve.Evaluate(breathInPercentage));
        }

        if (scale > breathInLength && scale < (breathOutLength + breathInLength))
        {
            ui.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * uiScale, breathInCurve.Evaluate(breathOutPercentage));
        }

        if(scale > (breathOutLength + breathInLength))
        {
            scale = scale - (breathOutLength + breathInLength);
        }

        if (doColorLogic)
        {
            //Do Color Logic here
            Debug.Log("Changing Colors");
        }
    }

    void PlayOrbit(RectTransform orbit, float oSpeed, bool doColorLogic = false)
    {
        orbit.transform.Rotate(new Vector3(0, 0, oSpeed) * Time.deltaTime);

        if (doColorLogic)
        {
            //Do Color Logic here
            Debug.Log("Changing Colors");
        }
    }
}
