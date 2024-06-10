using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillsVR.EventExtenstion;
using SkillsVR.OdinPlaceholder; 

public class SnapPointItem : MonoBehaviour
{
    [ReadOnly, ShowInInspector]
    public SnapPoint currentSnapPoint { protected set; get; }
    [ReadOnly, ShowInInspector]
    public SnapPoint inRangedSnapPoint { set; get; }

    [ReadOnly, ShowInInspector]
    public SnapPoint originSnapPoint { protected set; get; }

    public bool enableAutoSnap = true;

    public Vector3 snapOffsetPos = Vector3.zero;
    public Vector3 snapOffsetRot = Vector3.zero;

    [SerializeField]
    private UnityEventGameObject OnItemAttach = new UnityEventGameObject();
    [SerializeField]
    private UnityEventGameObject OnItemDetach = new UnityEventGameObject();


    [Button]
    public void SetAttachTo(SnapPoint newSnapPoint)
    {
        if (null == newSnapPoint)
        {
            OnItemDetach?.Invoke(null == currentSnapPoint ? null : currentSnapPoint.gameObject);
            currentSnapPoint = newSnapPoint;
        }
        else
        {
            currentSnapPoint = newSnapPoint;
            OnItemAttach?.Invoke(newSnapPoint.gameObject);
        }

    }

    [Button]
    public void SetOriginSnapPoint(SnapPoint newSnapPoint)
    {
        originSnapPoint = newSnapPoint;
    }

    [Button]
    public void Snap()
    {
        if (enableAutoSnap)
        {
            SnapTo(null != currentSnapPoint ? currentSnapPoint : originSnapPoint);
        }
    }

    public void DelaySnap(float delay)
    {
        StartCoroutine(DelaySnapSeconds(delay));
    }

    IEnumerator DelaySnapSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        Snap();
    }

    private void SnapTo(SnapPoint point)
    {
        bool enabledInHeirachy = this.gameObject.activeInHierarchy;

        if (null == point || !enabledInHeirachy)
        {
            return;
        }

        if(lerpCoroutine != null)
		{
            StopCoroutine(lerpCoroutine);
            lerpCoroutine = null;
		}

        lerpCoroutine = StartCoroutine(LerpSnap(point));

        //this.transform.position = point.GetSnapPos() + snapOffsetPos;
        //this.transform.eulerAngles = point.GetSnapAngle() + snapOffsetRot;
    }

    Coroutine lerpCoroutine;

    public void CheckLerping()
	{
        if (lerpCoroutine != null)
        {
            StopCoroutine(lerpCoroutine);
            lerpCoroutine = null;
        }
    }

    IEnumerator LerpSnap(SnapPoint point)
    {
        float elapsedTime = 0;
        float waitTime = 2f;

        while (elapsedTime < waitTime)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, point.GetSnapPos() + snapOffsetPos, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;

            // Yield here
            yield return null;
        }

        this.transform.eulerAngles = point.GetSnapAngle() + snapOffsetRot;

        //Snap();
    }

}
