using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCameraCanvas : MonoBehaviour
{
    private Camera targetCamera;
    private Canvas targetCanvas;

    private UnityEngine.XR.Interaction.Toolkit.XRRig xrRig;

    private int retryCount = 0;
    public void SetupCamera()
    {
        if (null != targetCamera)
        {
            return;
        }

        if (null == xrRig)
        {
            xrRig = GameObject.FindObjectOfType<UnityEngine.XR.Interaction.Toolkit.XRRig>();
        }

        targetCamera = null != xrRig ? (null != xrRig.cameraGameObject ? xrRig.cameraGameObject.GetComponent<Camera>() : Camera.main) : Camera.main;
        targetCanvas.worldCamera = targetCamera;

        if (null == targetCamera )
        {
            if (retryCount < 200)
            {
                ++retryCount;
                this.Invoke(nameof(SetupCamera), 500);
            }
            else
            {
                Debug.LogError("Set Canvas Camera Timeout: No XRRig or main camera found.");
            }
        }
    }

    void Awake()
    {
        targetCanvas = GetComponent<Canvas>();
        
    }

    private void OnEnable()
    {
        SetupCamera();
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(SetupCamera));
    }
}
