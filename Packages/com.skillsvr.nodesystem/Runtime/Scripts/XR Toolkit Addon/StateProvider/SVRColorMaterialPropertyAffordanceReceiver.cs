using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;

public class SVRColorMaterialPropertyAffordanceReceiver : ColorMaterialPropertyAffordanceReceiver
{
    private Coroutine colorRoutine;
    private Color defaultColor;
    private bool defaultSet= false;


    public void TriggerColorState(Color color, float durationToReset)
    {
        if (!defaultSet)
            defaultColor = GetCurrentValueForCapture();
        defaultSet = true;

        if (colorRoutine != null)
        {
            StopCoroutine(colorRoutine);
            CancelInvoke("ReturnColorToDefult");
        }

        colorRoutine = StartCoroutine(ColorRoutine(color, ()=> {
            colorRoutine = null;
            if (durationToReset > 0)
                Invoke("ReturnColorToDefult", durationToReset);
            }
        ));
    }

    public void ReturnColorToDefult()
    {
        TriggerColorState(defaultColor,0);
    }

    IEnumerator ColorRoutine(Color targetColor, Action onDone)
    {
        float timer = 0;
        Color startColor = GetCurrentValueForCapture();
        while(timer < affordanceStateProvider.transitionDuration)
        {
            OnAffordanceValueUpdated(Color.Lerp(startColor, targetColor, timer/ affordanceStateProvider.transitionDuration));
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        onDone.Invoke();
    }


  
}
