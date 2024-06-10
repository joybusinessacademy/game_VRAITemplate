using SkillsVR.EventExtenstion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointDragUI : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public bool enableDrag = true;

    public UnityEventVector3 beginDrag = new UnityEventVector3();
    public UnityEventVector3 drag = new UnityEventVector3();
    public UnityEventVector3 endDrag = new UnityEventVector3();

    public UnityEvent onBeginDragEvent = new UnityEvent();

    public Camera targetCamera = null;

    public bool applyMoveForCurrent = true;

    [SerializeField]
    private bool enableExtendDragPanel = true;

    public delegate Camera GetCustomTargetCameraDelegate();

    public GetCustomTargetCameraDelegate getCustomCameraMethod;

    private GameObject extendDragPanel;

    void Awake()
    {
        if (enableExtendDragPanel)
        {
            CreateExtendDragPanel();
        }
    }

    private Camera GetTargetCamera()
    {

        if (null != targetCamera)
        {
            return targetCamera;
        }
        if (null != getCustomCameraMethod)
        {
            targetCamera = getCustomCameraMethod.Invoke();
        }
        return targetCamera;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!enableDrag)
        {
            return;
        }

        Input.multiTouchEnabled = false;
        
        Vector3 v = new Vector3();
        bool hit = RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), eventData.position, null == eventData.enterEventCamera ? GetTargetCamera() : eventData.enterEventCamera, out v);

        this.GetComponent<UIBoxColliderResize>().SetNewBoxColliderSize();

        Vector3 boxCollider = this.GetComponent<BoxCollider>().size;
        boxCollider.x *= 0.1f;
        boxCollider.y *= 0.1f;
        this.GetComponent<BoxCollider>().size = boxCollider;

        if (hit && applyMoveForCurrent)
        {
            this.transform.position = v;
        }

        extendDragPanel?.SetActive(true);
        beginDrag?.Invoke(v);
        onBeginDragEvent?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!enableDrag)
        {
            return;
        }
        Vector3 v = new Vector3();
        bool hit = RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), eventData.position, null == eventData.enterEventCamera ? GetTargetCamera() : eventData.enterEventCamera, out v);

        if (hit && applyMoveForCurrent)
        {
            this.transform.position = v;
        }
        
        drag?.Invoke(v);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!enableDrag)
        {
            return;
        }
        enableDrag = true;
        Vector3 v = new Vector3();
        bool hit = RectTransformUtility.ScreenPointToWorldPointInRectangle(GetComponent<RectTransform>(), eventData.position, null == eventData.enterEventCamera ? GetTargetCamera() : eventData.enterEventCamera, out v);

        this.GetComponent<UIBoxColliderResize>().SetNewBoxColliderSize();

        if (hit && applyMoveForCurrent)
        {
            this.transform.position = v;
        }

        extendDragPanel?.SetActive(false);
        Input.multiTouchEnabled = true;
        endDrag?.Invoke(v);
    }

    public void SetEnableDrag(bool dragEnabled)
    {
        enableDrag = dragEnabled;
    }

    private void CreateExtendDragPanel()
    {
        if (null == extendDragPanel)
        {
            extendDragPanel = new GameObject("DragBGPanel");
            extendDragPanel.transform.SetParent(this.transform);
            extendDragPanel.transform.SetAsFirstSibling();
            var rt = extendDragPanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.rotation = new Quaternion();
            rt.localScale = Vector3.one * 500;
            rt.offsetMin = Vector3.zero;
            rt.offsetMax = Vector3.zero;
            rt.anchoredPosition3D = Vector3.zero;
            extendDragPanel.AddComponent<CanvasRenderer>();
            var img = extendDragPanel.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.clear;
            img.raycastTarget = false;
            extendDragPanel.SetActive(false);
        }
    }
}
