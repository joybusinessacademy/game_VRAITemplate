
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;

public class XRPlayerInputListener : MonoBehaviour
{
    [NonSerialized]
    public string targetInput;
    [NonSerialized]
    public UnityEvent onDownEvent = new UnityEvent();
    [NonSerialized]
    public UnityEvent onReleaseEvent = new UnityEvent();
    private bool currentState = false;
    private float threshHold = 0.8f;
    public void Update()
    {
        var splitsOr = targetInput.Split("||");
        var splitsAnd = targetInput.Split("&&");

        Debug.Assert(splitsOr.Length != 0 || splitsAnd.Length != 0, this + " input must have atleast 1 value");

        ProcessInputAsOr();
        ProcessInputAsAnd();

    }

    void ProcessInputAsOr()
    {
        if (targetInput.Contains("&&"))
            return;

        var splits = targetInput.Split("||");

        int downCount =  0;
        splits.ToList().ForEach(k => {

        if ((k.Contains("Axis") && Input.GetAxis(k) > threshHold) ||
            (k.Contains("Button") && Input.GetButtonDown(k))
#if UNITY_EDITOR            
            || Input.GetKeyDown(KeyCode.Space)
#endif
            )

                downCount++;
        });

        if (currentState == true && downCount == 0)
        {
            currentState = false;
            onReleaseEvent.Invoke();
        }

        if (currentState == false && downCount != 0)
        {
            currentState = true;
            onDownEvent.Invoke();
        }
    }

    void ProcessInputAsAnd()
    {
        if (targetInput.Contains("&&") == false)
            return;

        var splits = targetInput.Split("&&");

        int downCount =  0;
        splits.ToList().ForEach(k => {

        if ((k.Contains("Axis") && Input.GetAxis(k) > threshHold) ||
            (k.Contains("Button") && Input.GetButtonDown(k))
#if UNITY_EDITOR || UNITY_STANDALONE            
            || Input.GetKeyDown(KeyCode.Space)
#endif
            )

                downCount++;
        });

        if (currentState == true && downCount != splits.Length)
        {
            currentState = false;
            onReleaseEvent.Invoke();
        }

        if (currentState == false && downCount == splits.Length)
        {
            currentState = true;
            onDownEvent.Invoke();
        }
    }
}

