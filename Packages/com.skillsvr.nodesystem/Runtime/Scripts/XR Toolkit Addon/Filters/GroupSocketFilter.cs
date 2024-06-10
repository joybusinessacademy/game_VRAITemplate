using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;



public class GroupSocketFilter : XRBaseTargetFilter
{
    public int indexInGroup;
    private List<GroupSocketFilter> orderedGroup = new List<GroupSocketFilter>();

    public InteractorSocketData filterTypes = new InteractorSocketData();
    //public List<TagSO> correctFilterTypes = new List<TagSO>();

    private bool isCorrect = false;
    public bool IsCorrect { get => isCorrect; }

    private bool hasObject = false;
    public bool HasObject { get => hasObject; }

    private bool firstSocketWasInOrder = false;
    public bool FirstSocketWasInOrder { get => firstSocketWasInOrder; }

    public delegate void OnGroupComplete();
    public OnGroupComplete onGroupComplete;

    public delegate void FeedbackSignal(IXRInteractable interactable, IXRInteractor interactor);
    public FeedbackSignal onObjectAccepted;
    public FeedbackSignal onObjectCorrect;
    public FeedbackSignal onObjectIncorrect;
    public FeedbackSignal onObjectDropped;

    private IXRSelectInteractable objectInSocket;

    public bool enterSocketOnRayhitPos;
    private Vector3 originalAttachPos;
    public void ResetFilter()
    {
        isCorrect = false;
        hasObject = false;
        firstSocketWasInOrder = false;
        orderedGroup = new List<GroupSocketFilter>();
    }

    public void SetOriginalAttachPoint(XRSocketInteractor mySocket)
    {
        if (enterSocketOnRayhitPos)
        {
            originalAttachPos = mySocket.attachTransform.position;
            Debug.Log(originalAttachPos);
        }
    }

    public override void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results)
    {

        results.Clear();

        foreach (var interactable in targets)
        {
            if (interactable.transform.TryGetComponent<SocketableTag>(out SocketableTag socketableTag))
            {
                if (filterTypes.ContainsTag( socketableTag.tag))
                {
                    results.Add(interactable);
                }
            }
        }
    }



    public void SetGrouping(List<GroupSocketFilter> group)
    {
        orderedGroup = group;
        indexInGroup = orderedGroup.IndexOf(this);
    }

    void ProcessSocketGroup()
    {
        for (int i = 0; i < orderedGroup.Count; i++)
        {
            if (i < indexInGroup)
            {
                if (!orderedGroup[i].HasObject || orderedGroup[i].FirstSocketWasInOrder)
                {
                    //set my own since someone before me was wrong
                    firstSocketWasInOrder = false;
                    break;
                }
            }
            else if (i == indexInGroup)
            {
                //if I made it to here, then I'm in order
                firstSocketWasInOrder = true;
            }
        }

        foreach (var socket in orderedGroup)
        {
            if (!socket.HasObject)
                return;
        }
        
        if(onGroupComplete!=null)
            onGroupComplete.Invoke();
    }

    void ProcessEvents(IXRInteractable interactable, IXRInteractor interactor)
    {
        if (hasObject && !IsCorrect)
        {
            onObjectAccepted.Invoke(interactable, interactor);
        }
        else if (hasObject && IsCorrect)
        {
            onObjectCorrect.Invoke(interactable, interactor);
        }
        else
        {
            onObjectIncorrect.Invoke(interactable, interactor);
        }
    }

    internal void ProcessOnExitSocket(SelectExitEventArgs exitEventArgs)
    {
        if ((exitEventArgs.interactorObject as XRSocketInteractor) == this.GetComponent<XRSocketInteractor>())
        {
            if (exitEventArgs.interactableObject == objectInSocket)
            {
                objectInSocket = null;
                isCorrect = false;
                hasObject = false;
            }

            // means we moved my attach, set it back
            if (enterSocketOnRayhitPos)
            {
                (exitEventArgs.interactorObject as XRSocketInteractor).attachTransform.position = originalAttachPos;
                Debug.Log((exitEventArgs.interactorObject as XRSocketInteractor).attachTransform.position);
            }
        }
    }

    internal void ProcessOnEnterSocket(SelectEnterEventArgs enterEventArgs)
    {
          if ((enterEventArgs.interactorObject as XRSocketInteractor) == this.GetComponent<XRSocketInteractor>())
        {
            if (enterEventArgs.interactableObject.transform.TryGetComponent<SocketableTag>(out SocketableTag socketableType))
            {
               
                if (filterTypes.ContainsTag(socketableType.tag))
                {
                    //case for dropping object on a socket so we register the positions for edge cases
                    onObjectDropped.Invoke(enterEventArgs.interactableObject, enterEventArgs.interactorObject);
                    hasObject = true;
                    objectInSocket = enterEventArgs.interactableObject;

                    var filterOnSocketResponding = filterTypes.tagsFiltered.Find(f => f.filterTag == socketableType.tag);
                    if (filterOnSocketResponding.isCorrect)
                    {
                        isCorrect = true;
                    }
                    //called here assuming order consideration does not fail
                    //on incorrect object attempt, only accepted, but NOT correct.
                    ProcessSocketGroup();
                    
                }
                else
                {
                    //case for dropping object on a socket which rejects the tag
                    onObjectDropped.Invoke(enterEventArgs.interactableObject, null);
                }

                ProcessEvents(enterEventArgs.interactableObject, enterEventArgs.interactorObject);
            }
        }
    }
}

