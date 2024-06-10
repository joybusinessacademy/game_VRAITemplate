using System;
using System.Collections;
using System.Collections.Generic;
using DialogExporter;
using GraphProcessor;
using Props;
using Props.PropInterfaces;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace SkillsVRNodes.Scripts.Nodes
{
    [Serializable, NodeMenuItem("Learning/Drag and Drop", typeof(SceneGraph))]
    public class DragDropNode : ExecutableNode
    {
        public override string name => "Drag and Drop";
        public override string icon => "dragdrop";
        public override string tooltipURL => "https://skillsvr-documentation.vercel.app/nodes/learner-experience-node-types#drag-and-drop-node";
        public override int Width => MEDIUM_WIDTH;

        public override Color color => NodeColours.Learning;

        [Output(name = "Success")]
        public ConditionalLink OnSuccess = new();
        [Output(name = "Failure")]
        public ConditionalLink OnFail = new();

        //Completion section
        [HideInInspector] public int currentSelectedCompletionType;
        [HideInInspector] public int currentSelectedFeedbackType;
        [HideInInspector] public int currentSelectedShowOnType;

        //timer type
        [HideInInspector] public float timer = 60;
        [HideInInspector] public float showFor;

        //objects section
        [HideInInspector] public float pickupSpeed = 20;
        [HideInInspector] public float rotationSpeed;
        [HideInInspector] public int currentSelectedDropType;
        [HideInInspector] public int currentSelectedRotationType;
        [HideInInspector] public int currentSelectedRotationAxis;
        [HideInInspector] public Vector3 holdPositionOffset = new Vector3();
        [HideInInspector] public float scale = 1; // todo
        [HideInInspector] public bool dropOnPointer;
        [HideInInspector] public List<InteractableObjectData> interactableObjectDatas = new List<InteractableObjectData>();
        [HideInInspector] public List<PropGUID<IPropGrabInteractable>> objectInteractableInterfaces = new();

        private List<InteractableProp> interactables = new List<InteractableProp>();

        //socket section
        [HideInInspector] public bool correctOnlyInOrder;
        private bool triggerSuccess;

        [HideInInspector] public List<InteractorSocketData> interactorSocketDatas = new List<InteractorSocketData>();
        [HideInInspector] public List<PropGUID<IPropSocketInteractor>> socketInteractorInterfaces = new();

        private List<SocketProp> sockets = new List<SocketProp>();
        private List<GroupSocketFilter> filters = new List<GroupSocketFilter>();

        protected override void OnInitialise()
        {
            base.OnInitialise();

            var allProps = PropManager.GetAllProps();
            foreach (var prop in allProps)
            {
                var tryCastInteractable = (prop.PropComponent.propType.GetPropType() as InteractableProp);
                if (tryCastInteractable != null)
                {
                    tryCastInteractable.OriginalWorldLocation = tryCastInteractable.grabInteractable.transform.position;
                }
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            PrepareObjsAndSockets();
            InitializeCompletion();
            triggerSuccess = false;
        }

        protected override void OnComplete()
        {
            base.OnComplete();
        }

        private void PrepareObjsAndSockets()
        {

            ToggleMuteOfAllPropsBeingUsed(false);

            int indexCount = 0;
            interactables = new List<InteractableProp>();

            foreach (var propInterface in objectInteractableInterfaces)
            {
                interactables.Add(PropManager.GetProp<InteractableProp>(propInterface.propGUID));
            }

            foreach (var grabableProp in interactables)
            {
               
                grabableProp.BeingUsed = true;
                grabableProp.HasHadInteractionInstance = false;
                grabableProp.grabInteractable.interactionLayers = ~0;
                grabableProp.collider.enabled = true;

                //grabbing
                grabableProp.grabOffset.grabOffset = holdPositionOffset;

                if (pickupSpeed == 0)
                {
                    grabableProp.grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable.MovementType.Instantaneous;
                    grabableProp.grabOffset.attachEaseInSpeed = 0;
                }
                else
                {
                    grabableProp.grabInteractable.movementType = UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable.MovementType.Kinematic;
                    grabableProp.grabOffset.attachEaseInSpeed = pickupSpeed;
                }

                grabableProp.SetDropBehavior(currentSelectedDropType);

                //Ray selection
                grabableProp.raySocket.AssociatedCollider = grabableProp.collider.gameObject;
                grabableProp.raySocket.onObjectDropped += InteractableWasDropped;
                grabableProp.raySocket.enterSocketOnRayhitPos = dropOnPointer;

                //rotation
                grabableProp.grabRotation.autoRotate = currentSelectedRotationType == 0 ? true : false;
                grabableProp.grabRotation.rotationSpeed = rotationSpeed;
                grabableProp.grabRotation.rotateAround = (RotationAxis)currentSelectedRotationAxis;

                //scale
                grabableProp.grabScale.scaleOnGrab = (grabableProp.grabInteractable.transform.localScale * scale);

                //filter
                grabableProp.socketableTag.tag = interactableObjectDatas[indexCount].tag;
                indexCount++;
            }

            //sockets
            indexCount = 0;
            filters = new List<GroupSocketFilter>();
            sockets = new List<SocketProp>();

            foreach (var socketInterface in socketInteractorInterfaces)
            {
                SocketProp socketProp = PropManager.GetProp<SocketProp>(socketInterface.propGUID);
                socketProp.BeingUsed = true;
                socketProp.socket.enabled = true;
                socketProp.collider.enabled = true;
                socketProp.socket.interactionLayers = ~0;
                socketProp.socket.startingTargetFilter = socketProp.filter;

                sockets.Add(socketProp);
                filters.Add(socketProp.filter);
            }

            foreach (var filter in filters)
            {
                filter.ResetFilter();

                filter.enterSocketOnRayhitPos = dropOnPointer; 
                filter.SetOriginalAttachPoint(sockets[indexCount].socket);

                filter.filterTypes = interactorSocketDatas[indexCount];
                filter.SetGrouping(filters);
                filter.onObjectAccepted += AcceptedObjectSocket;
                filter.onObjectCorrect += CorrectObjectSocket;
                filter.onObjectIncorrect += IncorrectObjectSocket;
                filter.onObjectDropped += InteractableWasDropped;

                //we aren't waiting for a timer or for all props to be grabed once, so we listen for group of sockets to be filled
                if (currentSelectedCompletionType != 0 && currentSelectedCompletionType != 1)
                    filter.onGroupComplete += TriggerPreCompletion;

                //every interactable of this instance should fire enter/exit events for each filter
                foreach (var grabableProp in interactables)
                {
                    grabableProp.grabInteractable.selectEntered.AddListener(filter.ProcessOnEnterSocket);
                    grabableProp.grabInteractable.selectExited.AddListener(filter.ProcessOnExitSocket);
                }
                indexCount++;
            }
        }

        private void InteractableWasDropped(IXRInteractable interactable, IXRInteractor interactor)
        {
            var interactablePropDropped = interactables.Find(i => i.grabInteractable == (XRGrabInteractable)interactable);
            if (interactablePropDropped != null)
            {
                if (interactor != null)
                {
                    interactablePropDropped.mover.StopMoveRoutine(); 
                    //if was dropped with a socket toward a socket from the ray, stash sockets pos for forced movement edge-case
                    if ((interactor as XRSocketInteractor) != null)
                        interactablePropDropped.IntendedWorldLocation = (interactor as XRSocketInteractor).attachTransform.position;
                }
                else if (currentSelectedDropType == 0) //we return to original location
                {
                    interactablePropDropped.mover.StartMoveRoutine(interactablePropDropped.grabInteractable,
                        interactablePropDropped.OriginalWorldLocation, Math.Clamp(pickupSpeed, 5, 100));
                }
            }
        }

        private void IncorrectObjectSocket(IXRInteractable interactable, IXRInteractor interactor)
        {
            //show immediately && anything other than no feedback
            if (currentSelectedShowOnType == 0 && currentSelectedFeedbackType != 0)
            {
                var socketPropTried = sockets.Find(s => s.socket == (XRSocketInteractor)interactor);
                if (socketPropTried != null)
                    socketPropTried.ShowIncorrect(showFor);

                //atempt to show correct choice as well
                if (currentSelectedFeedbackType == 2)
                {
                    var interactablePropTried = interactables.Find(i => i.grabInteractable == (XRGrabInteractable)interactable);
                    if (interactablePropTried != null)
                    {
                        var corectChoice = interactablePropTried.socketableTag.tag;
                        foreach (var socket in sockets)
                        {
                            if (socket.filter.filterTypes.ContainsTag(corectChoice))
                            {
                                socket.ShowCorrect(showFor);
                            }
                        }
                    }
                }
            }
        }

        private void CorrectObjectSocket(IXRInteractable interactable, IXRInteractor interactor)
        {
            if (currentSelectedShowOnType == 0 && currentSelectedFeedbackType != 0)
            {
                var socketPropTried = sockets.Find(s => s.socket == (XRSocketInteractor)interactor);
                if (socketPropTried != null)
                    socketPropTried.ShowCorrect(showFor);
            }
        }

        private void AcceptedObjectSocket(IXRInteractable interactable, IXRInteractor interactor)
        {
            //show immediately && anything other than no feedback
            if (currentSelectedShowOnType == 0 && currentSelectedFeedbackType != 0)
            {
                var socketPropTried = sockets.Find(s => s.socket == (XRSocketInteractor)interactor);
                if (socketPropTried != null)
                    socketPropTried.ShowIncorrect(showFor);

                //atempt to show correct choice as well
                if (currentSelectedFeedbackType == 2)
                {
                    var interactablePropTried = interactables.Find(i => i.grabInteractable == (XRGrabInteractable)interactable);
                    if (interactablePropTried != null)
                    {
                        var corectChoice = interactablePropTried.socketableTag.tag;
                        foreach (var socket in sockets)
                        {
                            if (socket.filter.filterTypes.ContainsTag(corectChoice))
                            {
                                socket.ShowCorrect(showFor);
                            }
                        }
                    }
                }
            }
        }

        private void InitializeCompletion()
        {
            if (currentSelectedCompletionType == 0)
            {
                WaitMonoBehaviour.Process(timer, TriggerPreCompletion);
            }
            else if (currentSelectedCompletionType == 1)
            {
                foreach (var grabable in interactables)
                {
                    grabable.grabInteractable.selectExited.AddListener(OnObjectReleased);
                    grabable.HasHadInteractionInstance = false;
                }
            }
        }

        private void OnObjectReleased(SelectExitEventArgs arg0)
        {
            var interactablePropSelected = interactables.Find(i => i.grabInteractable == (XRGrabInteractable)arg0.interactableObject);
            interactablePropSelected.HasHadInteractionInstance = true;

            foreach (var grabable in interactables)
            {
                if (!grabable.HasHadInteractionInstance)
                    return;
            }

            WaitMonoBehaviour.Process(1, TriggerPreCompletion);
        }

        private void TriggerPreCompletion()
        {
            //show on complete
            if (currentSelectedShowOnType == 1)
            {
                bool allCorrect = true;
                triggerSuccess = true;

                //check correctness
                foreach (var filter in filters)
                {
                    if ((filter.FirstSocketWasInOrder && correctOnlyInOrder)
                        || (filter.IsCorrect && !correctOnlyInOrder))
                    {
                        sockets[filters.IndexOf(filter)].ShowCorrect(showFor);
                        triggerSuccess = true;
                    }
                    else
                    {
                        sockets[filters.IndexOf(filter)].ShowIncorrect(showFor);
                        triggerSuccess = false;
                        //only flag that not all are correct IF we dont care about order,
                        //order has priority over a Full correct requirement
                        if (!correctOnlyInOrder)
                            allCorrect = false;
                    }
                }

                //we must ensure all correct
                if (currentSelectedCompletionType == 3 && !allCorrect)
                    return;

                ReleaseAllPropsBeingUsed();

                //disable here to allow feedback to happen without interactablility
                ToggleMuteOfAllPropsBeingUsed(true);
                WaitMonoBehaviour.Process(showFor + 1, TriggerCompletion);
                return;

            }

            ReleaseAllPropsBeingUsed();

            //disable here to allow feedback to happen without interactablility
            ToggleMuteOfAllPropsBeingUsed(true);
            WaitMonoBehaviour.Process(1, TriggerCompletion);
        }

        private void ReleaseAllPropsBeingUsed()
        {
            foreach (var item in interactables)
            {
                item.BeingUsed = false;
                if (item.IntendedWorldLocation != Vector3.zero)
                    item.mover.StartMoveRoutine(item.grabInteractable, item.IntendedWorldLocation, pickupSpeed);
            }
            foreach (var item in sockets)
            {
                item.BeingUsed = false;
            }
        }

        private void ToggleMuteOfAllPropsBeingUsed(bool mute)
        {
            foreach (var grabableProp in interactables)
            {
                if (mute)
                    grabableProp.MuteInScene(this);
                else
                    grabableProp.UnmuteInScene(this);
            }

            foreach (var socket in sockets)
            {
                if (mute)
                    socket.MuteInScene(this);
                else
                    socket.UnmuteInScene(this);
            }
        }

        private void TriggerCompletion()
        {
            foreach (var filter in filters)
            {
                filter.onGroupComplete -= InitializeCompletion;

                filter.onObjectAccepted -= AcceptedObjectSocket;
                filter.onObjectCorrect -= CorrectObjectSocket;
                filter.onObjectIncorrect -= IncorrectObjectSocket;
                filter.onObjectDropped -= InteractableWasDropped;

                foreach (var grabableProp in interactables)
                {

                    grabableProp.grabInteractable.selectEntered.RemoveListener(filter.ProcessOnEnterSocket);
                    grabableProp.grabInteractable.selectExited.RemoveListener(filter.ProcessOnExitSocket);

                }
            }

            foreach (var grabableProp in interactables)
            {
                grabableProp.grabInteractable.selectExited.RemoveListener(OnObjectReleased);
                grabableProp.HasHadInteractionInstance = false;
                grabableProp.raySocket.onObjectDropped -= InteractableWasDropped;
            }

            CompleteNode();
            if (triggerSuccess)
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

[Serializable]
public class InteractableObjectData
{
    public TagSO tag;
}

[Serializable]
public class InteractorSocketData
{
    public List<InteractorSocketDataInstance> tagsFiltered = new List<InteractorSocketDataInstance>();

    public bool ContainsTag(TagSO tag)
    {
        foreach (var item in tagsFiltered)
        {
            if (item.filterTag == tag)
                return true;
        }

        return false;
    }
}

[Serializable]
public class InteractorSocketDataInstance
{
    public TagSO filterTag;
    public bool isCorrect;
}


