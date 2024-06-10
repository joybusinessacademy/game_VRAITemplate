using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathingUIB : MonoBehaviour
{
    [SerializeField]
    private float radius;
    [SerializeField]
    private float breatheIn = 2;
    [SerializeField]
    private float breatheOut = 3;
    [SerializeField]
    private Transform timerMarker;
    [SerializeField]
    private Transform breatheInMarker;
    [SerializeField]
    private Transform breatheOutMarker;
    [SerializeField]
    private Transform breatheCircleMain;

    [SerializeField]
    private Transform breatheCircle1_1;
    [SerializeField]
    private Transform breatheCircle1_2;
    [SerializeField]
    private AnimationCurve breatheInAnimCurve;
    [SerializeField]
    private AnimationCurve breatheOutAnimCurve;
    [SerializeField]
    private TMPro.TextMeshProUGUI breatheText;

    private float polled = 0;

    private float Cycle => (breatheIn + breatheOut);

    void Start()
    {
        // set breathe in position
        // this 0 position up
        float rate = (Cycle * .75f) / Cycle;
        float angle = rate * 360;

        breatheInMarker.localPosition = GetPositionFromAngle(radius, angle);
        breatheInMarker.localEulerAngles = new Vector3(0,0, 360 - angle - 90);

        // set breathe out position
        rate = breatheIn / Cycle + (Cycle * .75f) / Cycle;
        angle = rate * 360;

        breatheOutMarker.localPosition = GetPositionFromAngle(radius, angle);
        breatheOutMarker.localEulerAngles = new Vector3(0,0, 360 - angle - 90);
    }

    // Update is called once per frame
    void Update()
    {
        polled += Time.deltaTime;   
        polled = Mathf.Repeat(polled, breatheIn + breatheOut);

        float rate = polled / (breatheIn + breatheOut);
        float angle = rate * 360;

        timerMarker.localPosition = GetPositionFromAngle(radius, angle - 90);

        AnimationCurve currentCurve =  polled < breatheIn ? breatheInAnimCurve : breatheOutAnimCurve;
        float breathingPolled =  polled < breatheIn ? polled / breatheIn : 1 - ((polled - breatheIn) / breatheOut);

        breatheCircle1_1.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, currentCurve.Evaluate(breathingPolled));
        breatheCircle1_2.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, currentCurve.Evaluate(breathingPolled));
        breatheCircleMain.transform.localScale = Vector3.Lerp(Vector3.one *.95f, Vector3.one * 1.15f, currentCurve.Evaluate(breathingPolled));

        breatheText.text =  polled < breatheIn ? "breathe in" : "breathe out";
    }

    private Vector3 GetPositionFromAngle(float radius, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        float x = radius * Mathf.Cos(-radians);
        float y = radius * Mathf.Sin(-radians);

        // Assuming 2D space, z-coordinate is set to 0
        return new Vector3(x, y, 0f);
    }
}
