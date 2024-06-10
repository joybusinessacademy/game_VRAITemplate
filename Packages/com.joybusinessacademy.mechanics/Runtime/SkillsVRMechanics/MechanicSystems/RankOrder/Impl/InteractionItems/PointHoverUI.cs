using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PointHoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent FirstHoverEnter = new UnityEvent();
    public UnityEvent LastHoverExit = new UnityEvent();

    public UnityEvent HoverEnter = new UnityEvent();
    public UnityEvent HoverExit = new UnityEvent();

    private int hoverCount = 0;

    public void OnPointerEnter(PointerEventData eventData)
    {
        hoverCount++;
        HoverEnter?.Invoke();
        if (1 == hoverCount)
        {
            FirstHoverEnter?.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverCount--;
        hoverCount = Mathf.Max(0, hoverCount);
        HoverExit?.Invoke();
        if (0 == hoverCount)
        {
            LastHoverExit?.Invoke();
        }
    }
}
