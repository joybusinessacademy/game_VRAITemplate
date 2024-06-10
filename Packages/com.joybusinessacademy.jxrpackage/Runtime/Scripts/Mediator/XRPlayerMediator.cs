using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRPlayerMediator : MonoBehaviour
{
    private List<MediatorAction> mediators = new List<MediatorAction>();
    
    private TeleportationProvider teleportationProvider;

    [System.NonSerialized]
    public Transform recenterTransform;
    private static int lastRecenterCount = 1;

    public void UnregisterMediator(GameObject target)
    {
        mediators.ForEach(k => DestroyImmediate(k.inputListener));
        mediators.RemoveAll(k => k.id.Equals(target.GetInstanceID()));
    }

    public void Breathing(object[] param)
    {
        GameObject target = param[0] as GameObject;
        string inputs = param[1] as string;


        var mediator = new BreathingMediator() {id = target.GetInstanceID()};      
        mediator.inputListener = target.AddComponent<XRPlayerInputListener>();
        
        // this should come from node
        // both buttons must be pressed
        mediator.inputListener.targetInput = inputs;

        mediator.inputListener.onDownEvent.AddListener(() => {
            mediator.state = !mediator.state;
            target.transform.GetChild(0).SendMessage(mediator.actionName, mediator.state, SendMessageOptions.DontRequireReceiver);
        });

        mediator.inputListener.onReleaseEvent.AddListener(() => {
            mediator.state = false;
            target.transform.GetChild(0).SendMessage(mediator.actionName, mediator.state, SendMessageOptions.DontRequireReceiver);
        });

        mediators.Add(mediator);
    }

    public void AudioRecorder(object[] param)
    {
        GameObject target = param[0] as GameObject;
        string inputs = param[1] as string;

        var mediator = new AudioRecordinMediator() {id = target.GetInstanceID()};      

        mediator.inputListener = target.AddComponent<XRPlayerInputListener>();
        
        // this should come from node
        // any of the button
        mediator.inputListener.targetInput = inputs;
        //inputs;

        mediator.inputListener.onDownEvent.AddListener(() => {
            mediator.state = !mediator.state;
            target.transform.GetChild(0).SendMessage(mediator.actionName, SendMessageOptions.DontRequireReceiver);
        });

        mediators.Add(mediator);
    }

    public void HandState(object[] param)
    {
        bool newHandState = false;
		newHandState = (bool)param[0];

        XRPlayerHandController xrPlayerHandController = this.GetComponent<XRPlayerHandController>();
        
        if(xrPlayerHandController == null)
        {
            Debug.LogError("Missing Hand Controller");
            return;
        }

        if (newHandState)
            xrPlayerHandController.SetPointerState();
        else
            xrPlayerHandController.SetDefaultState();
	}

    public void ScreenFadeToColor(object[] param)
    {
        var screenFade = GetComponentInChildren<XRTransition>(true);
        screenFade.FadeToColor(param);
    }

    private void TeleportTo(Transform teleportPosition)
    {
        teleportationProvider = GetComponent<TeleportationProvider>();

        TeleportRequest request = new TeleportRequest();
        request.destinationRotation = teleportPosition.rotation;
        request.destinationPosition = teleportPosition.position;
        request.matchOrientation = MatchOrientation.TargetUpAndForward;
        request.requestTime = Time.time;

        recenterTransform = teleportPosition;

        teleportationProvider?.QueueTeleportRequest(request);
    }

    
    protected virtual void Update()
    {

#if PICO_XR
        int current = Unity.XR.PXR.PXR_Manager.trackingRecenterCount;
#else
        int current = OVRPlugin.GetLocalTrackingSpaceRecenterCount();
#endif

        if (lastRecenterCount != current && recenterTransform != null)
        {
            transform.forward = recenterTransform.rotation * Vector3.forward;

            transform.position = recenterTransform.position - Camera.main.transform.localPosition;
        }

        lastRecenterCount = current;
    }

    private abstract class MediatorAction
    {
        public bool state = false;
        public XRPlayerInputListener inputListener;
        public abstract string actionName {get;}
        public int id;
    }

    // at the moment set as toggle
    private class BreathingMediator : MediatorAction
    {
        public override string actionName => "ActiveBreath";
    }

    // at the moment set as toggle
    private class AudioRecordinMediator : MediatorAction
    {
        public override string actionName => state ? "StartRecord" : "StopRecord";
    }

}
