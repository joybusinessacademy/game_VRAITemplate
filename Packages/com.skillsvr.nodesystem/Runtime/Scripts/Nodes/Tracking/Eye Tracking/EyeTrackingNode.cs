using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.Android;

namespace SkillsVRNodes.Scripts.Nodes
{
    [Serializable, NodeMenuItem("Telemetry/EyeTracking", typeof(SceneGraph)), NodeMenuItem("Telemetry/EyeTracking", typeof(SubGraph))]
    public class EyeTrackingNode : ExecutableNode
    {
        public override string name => "Eye Tracking";
        public override string icon => "Player";

        // 1 is looking at
        float angle = 0.98f;
        public float totalTimeToLookAt = 5f;
        public float targetLookingTime = 0f;
        float timer = 0f;


        public string lookAtTransform;


        public override Color color => NodeColours.UserRig;


        public int currentCompletePreset;
        public int currentCompleteLogic;

        [HideInInspector] public List<InteractableObjectData> interactableObjectDatas = new List<InteractableObjectData>();
        [HideInInspector] public List<PropGUID<IBaseProp>> objectInteractableInterfaces = new();
        [HideInInspector] public bool UseRaycast;
        [HideInInspector] public bool showLookingAtUI;

        [Output(name = "Success")]
        public ConditionalLink OnSuccess = new();
        [Output(name = "Failure")]
        public ConditionalLink OnFail = new();


        private List<Transform> propObjectTransforms = new List<Transform>();

        private List<LookAtInstance> lookAtInstances = new List<LookAtInstance>();

        GameObject trackingObject;
        string trackingObjectName = "Tracking Director";

        private class LookAtInstance
        {
            public Transform propTransform;
            public float timeLookedAt;
            public bool lookedAtflag;
        }

        protected override void OnStart()
        {
            //spawned the controller here is using spawner node
            base.OnStart();

            SetUpTrackingObject();

            timer = 0;
            targetLookingTime = Mathf.Clamp(targetLookingTime, 0, totalTimeToLookAt);

            var props = new List<PropType>();
            propObjectTransforms.Clear();
            foreach (var propInterface in objectInteractableInterfaces)
            {
                propObjectTransforms.Add(PropManager.GetProp<PropType>(propInterface.propGUID).GetPropComponent().gameObject.transform);
            }
            lookAtInstances.Clear();
            foreach (var propObject in propObjectTransforms)
            {             
                lookAtInstances.Add(new LookAtInstance { propTransform = propObject, timeLookedAt = 0, lookedAtflag = false }) ;
            }           
        }

        private void SetUpTrackingObject()
        {
            trackingObject = GameObject.Find(trackingObjectName +"(Clone)");
            if (trackingObject == null)
            {
                trackingObject = Resources.Load(trackingObjectName) as GameObject;
                if(trackingObject!=null)
                    trackingObject =  GameObject.Instantiate(trackingObject);
            }

            trackingObject.GetComponent<TrackingDirector>().Begin(UpdateData, this);        
        }

        float SignedDistanceFromLine(Vector3 headPos, Vector3 lookDirection, Vector3 objectToLookAt)
        {
            objectToLookAt -= headPos;
            // Extract the component of our offset pointing in this perpendicular direction.
            return Vector3.Dot(lookDirection.normalized, objectToLookAt.normalized);
        }


        public void UpdateData(EyeTrackingFrame frame)
        {
            if (UseRaycast)
            {
                ProcessDataRaycast(frame);
            }
            else
            {
                ProcessDataAngle(frame);
            }

            timer += Time.deltaTime;
            CompleteCheck();
        }
        private void ProcessDataAngle(EyeTrackingFrame frame)
        {
            foreach (var item in lookAtInstances)
            {
                float lookAngle = SignedDistanceFromLine(frame.lookAtPosition,
                    frame.lookAtRotation * Vector3.forward, item.propTransform.position);
                if (lookAngle > angle)
                {
                    item.lookedAtflag = true;
                    item.timeLookedAt += Time.deltaTime;
                }
            }
        }

        private void ProcessDataRaycast(EyeTrackingFrame frame)
        {
            Ray ray = new Ray(frame.lookAtPosition, frame.lookAtRotation * Vector3.forward);
            Physics.Raycast(ray, out RaycastHit hitInfo, 999);

            if (hitInfo.collider != null)
            { 
                foreach (var item in lookAtInstances)
                {
                    if(hitInfo.collider.attachedRigidbody.gameObject == item.propTransform.gameObject)
                    {
                        item.lookedAtflag = true;
                        item.timeLookedAt += Time.deltaTime;
                    }
                }
            }
        }

        private void CompleteCheck()
        {
            switch (currentCompletePreset)
            {
                case 0: // look at once
                    
                    int[] flags = new int[lookAtInstances.Count];
                    for (int i = 0; i < lookAtInstances.Count; i++)
                    {
                        flags[i] = (lookAtInstances[i].lookedAtflag ? 1 : 0);
                    }

                    if (currentCompleteLogic == 0) //all objects
                    {
                        if (flags.Sum() == lookAtInstances.Count)
                            FinishNode(true);
                    }

                    if(currentCompleteLogic == 1) // any object
                    {
                        if(flags.Sum() > 0)
                            FinishNode(true);
                    }
                    
                    break;
                case 1: // look at for a time

                    if (timer > totalTimeToLookAt)
                    {
                        int[] timeFlags = new int[lookAtInstances.Count];
                        for (int i = 0; i < lookAtInstances.Count; i++)
                        {
                            timeFlags[i] = (lookAtInstances[i].timeLookedAt > targetLookingTime ? 1 : 0);
                        }

                        if (currentCompleteLogic == 0) //all objects
                        {
                            if (timeFlags.Sum() == lookAtInstances.Count)
                                FinishNode(true);
                            else
                                FinishNode(false);  
                        }

                        if (currentCompleteLogic == 1) // any object
                        {
                            if (timeFlags.Sum() > 0)
                                FinishNode(true);
                            else
                                FinishNode(false);  
                        }
                    }
                break;
            }
        }


        public void FinishNode(bool success)
        {
            TrackingFrameComponent.CollectDataEye -= UpdateData;
            trackingObject.GetComponent<TrackingDirector>().End(this);
            CompleteNode();
            if (success)
            {
                RunLink(nameof(OnSuccess));
            }
            else
            {
                RunLink(nameof(OnFail));
            }
        }
    }
}