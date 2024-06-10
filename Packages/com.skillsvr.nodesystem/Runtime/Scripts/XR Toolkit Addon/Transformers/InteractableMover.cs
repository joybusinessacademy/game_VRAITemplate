using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractableMover : MonoBehaviour
{
    private Coroutine moveRoutine;
    public void StartMoveRoutine(XRGrabInteractable interactable, Vector3 worldPosition, float pickupSpeed)
    {
        StopMoveRoutine();

          moveRoutine = StartCoroutine(MoveIntractableToPosition(interactable, worldPosition, pickupSpeed));
    }

    public void StopMoveRoutine()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }
    private IEnumerator MoveIntractableToPosition(XRGrabInteractable interactable, Vector3 worldPosition, float pickupSpeed)
    {


        float epsilon = 0.01f;

        while (Vector3.Distance(transform.position, worldPosition) > epsilon)
        {
            transform.position = Vector3.Lerp(interactable.transform.position, worldPosition, Time.deltaTime * pickupSpeed);

            if (interactable.movementType == XRBaseInteractable.MovementType.Instantaneous)
            {
                transform.position = worldPosition;
            }


            yield return new WaitForEndOfFrame();
        }

        transform.position = worldPosition;

    }
}
