using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XRTransition : MonoBehaviour
{
    [SerializeField]
    public Image image;

    public Color targetColor;

    [SerializeField]
    private Canvas renderingCanvas;

    private readonly List<System.Action> fadeToWhiteOnCompleteStack = new();
    private readonly List<System.Action> fadeToClearOnCompleteStack = new();

    private Coroutine fadeToClearCoroutine = null;
    private Coroutine fadeToWhiteCoroutine = null;


    public void FadeToWhite(System.Action onComplete = null, float time = 0.1f)
    {
        if (onComplete != null)
        {
            fadeToWhiteOnCompleteStack.Add(onComplete);
        }

        if (image.color == Color.black)
        {
            fadeToWhiteOnCompleteStack.ForEach(k => k.Invoke());
            fadeToWhiteOnCompleteStack.Clear();
            return;
        }

        fadeToWhiteCoroutine ??= StartCoroutine(FadeToWhiteCoroutine(time));
    }
    
    public void ScreenFadeToColor(object[] param)
    {
        FadeToColor(param);
    }

    public void FadeToClear(System.Action onComplete = null, float time = 0.1f)
    {
        if (onComplete != null)
        {
            fadeToClearOnCompleteStack.Add(onComplete);
        }

        if (image.color == Color.clear)
        {
            fadeToClearOnCompleteStack.ForEach(k => k.Invoke());
            fadeToClearOnCompleteStack.Clear();
            return;
        }

        fadeToClearCoroutine ??= StartCoroutine(FadeToClearCoroutine(time));
    }

    private IEnumerator FadeToWhiteCoroutine(float time = 0.5f)
    {
        float colorFade = time;
        image.enabled = true;

        if (renderingCanvas)
        {
            renderingCanvas.enabled = image.enabled;
        }

        while (colorFade > 0)
        {
            colorFade -= Mathf.Min(Time.fixedDeltaTime, Time.deltaTime);
            float r = colorFade / time;
            image.color = Color.Lerp(Color.black, Color.clear, r);
            yield return new WaitForEndOfFrame();
        }

        image.color = Color.Lerp(Color.black, Color.clear, 0);
        fadeToWhiteOnCompleteStack.ForEach(k => k.Invoke());
        fadeToWhiteOnCompleteStack.Clear();
        fadeToWhiteCoroutine = null;
    }

    private IEnumerator FadeToClearCoroutine(float time = 0.5f)
    {
        if (renderingCanvas)
        {
            renderingCanvas.enabled = image.enabled;
        }

        yield return new WaitForEndOfFrame();

        float colorFade = time;
        while (colorFade > 0)
        {
            colorFade -= Mathf.Min(Time.fixedDeltaTime, Time.deltaTime);
            float r = colorFade / time;
            image.color = Color.Lerp(Color.black, Color.clear, 1 - r);
            yield return new WaitForEndOfFrame();
        }

        image.color = Color.Lerp(Color.black, Color.clear, 1);
        image.enabled = false;

        if (renderingCanvas)
        {
            renderingCanvas.enabled = image.enabled;
        }

        fadeToClearOnCompleteStack.ForEach(k => k.Invoke());
        fadeToClearOnCompleteStack.Clear();
        fadeToClearCoroutine = null;
    }

    public void FadeToColor(object[] param)
    {
        FadeToColor((float)param[0], (Color)param[1], (System.Action)param[2]);
    }

    public void FadeToColor(float time, Color color, System.Action onComplete)
    {
        StartCoroutine(FadeToColorCoroutine(time,color, onComplete));            
    }

    // is not aware of current state
    // will just run fade no matter what
    private IEnumerator FadeToColorCoroutine(float time, Color color, System.Action onComplete)
    {
        yield return new WaitForEndOfFrame();
        targetColor = color;
        image.enabled = true;
        Color current = image.color;

        if (renderingCanvas)
        {
            renderingCanvas.enabled = image.enabled;
        }

        float colorFade = time;
        while (colorFade > 0)
        {
            colorFade -= Mathf.Min(Time.fixedDeltaTime, Time.deltaTime);
            float r = colorFade / time;
            image.color = Color.Lerp(color, current, r);
            yield return new WaitForEndOfFrame();
        }

        image.color = Color.Lerp(current, color, 1);
        image.enabled = image.color.a != 0;

        if (renderingCanvas)
        {
            renderingCanvas.enabled = image.enabled;
        }

        onComplete?.Invoke();
    }
}
