using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControllerLabel : MonoBehaviour
{
    [SerializeField] private float minDistance;
    [SerializeField] private float fadeSpeedMulti;
    [SerializeField] private GameObject positionalReferenceObject;
    [Space]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform beginLine, endLine;
    [SerializeField] private Text lableText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;

    private bool isClose;
    Coroutine fadeRoutine;

    private void Start()
    {
        isClose = false;
        lineRenderer.SetPositions(new Vector3[] { new Vector3(beginLine.localPosition.x, beginLine.localPosition.y, beginLine.localPosition.z), 
            new Vector3(endLine.localPosition.x, endLine.localPosition.y, endLine.localPosition.z) });
    }

    public void UpdateLableText(string s)
    {
        lableText.text = s;
    }

    private void Update()
    {
        if (isClose)
        {          
            if ((transform.position - positionalReferenceObject.transform.position).sqrMagnitude > minDistance)
            {

                if (fadeRoutine != null)
                    StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(FadeRoutine(false));
                isClose = false;
            }
        }
        else
        {
            if ((transform.position - positionalReferenceObject.transform.position).sqrMagnitude < minDistance)
            {

                if (fadeRoutine != null)
                    StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(FadeRoutine(true));
                isClose = true;
            }
        }
    }

    public Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    IEnumerator FadeRoutine(bool fadeIn)
    {
        Color startColor = backgroundImage.color;
        float startAlpha = startColor.a;

        if(fadeIn)
        {
            while (backgroundImage.color.a < 1)
            {
                startAlpha += Time.deltaTime * fadeSpeedMulti;

                backgroundImage.color = WithAlpha(Color.white, startAlpha);
                iconImage.color = WithAlpha(Color.white, startAlpha);
                lableText.color = WithAlpha(Color.black, startAlpha);
                lineRenderer.startColor = WithAlpha(Color.white, startAlpha);
                lineRenderer.endColor = WithAlpha(Color.white, startAlpha);

                yield return new WaitForFixedUpdate();
            }

            backgroundImage.color = WithAlpha(Color.white, 1);
            lableText.color = WithAlpha(Color.black, 1);
            lineRenderer.startColor = WithAlpha(Color.white, 1);
            lineRenderer.endColor = WithAlpha(Color.white, 1);
        }
        else
        {
            while (backgroundImage.color.a > 0)
            {
                startAlpha -= Time.deltaTime * fadeSpeedMulti;
                backgroundImage.color = WithAlpha(Color.white, startAlpha);
                iconImage.color = WithAlpha(Color.white, startAlpha);
                lableText.color = WithAlpha(Color.black, startAlpha);
                lineRenderer.startColor = WithAlpha(Color.white, startAlpha);
                lineRenderer.endColor = WithAlpha(Color.white, startAlpha);

                yield return new WaitForFixedUpdate();
            }

            backgroundImage.color = WithAlpha(Color.white, 0);
            lableText.color = WithAlpha(Color.black, 0);
            lineRenderer.startColor = WithAlpha(Color.white, 0);
            lineRenderer.endColor = WithAlpha(Color.white, 0);
        }
    }
}
