using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControl : MonoBehaviour
{
    public GameObject top;
    public GameObject bottom;
    public GameObject target;

    Vector3 lastMousePos;
    private void Awake()
    {
        lastMousePos = Input.mousePosition;
    }
    private void Update()
    {
        var pos = target.transform.position;
        pos.y += -(lastMousePos.y - Input.mousePosition.y) * 0.1f;
        pos.y = Mathf.Clamp(pos.y, bottom.transform.position.y, top.transform.position.y);
        target.transform.position = pos;
        lastMousePos = Input.mousePosition;
    }
}
