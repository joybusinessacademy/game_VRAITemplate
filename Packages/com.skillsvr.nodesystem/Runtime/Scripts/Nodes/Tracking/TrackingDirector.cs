using SkillsVRNodes.Scripts.Nodes;
using SkillsVRNodes.Scripts.Nodes.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackingDirector : MonoBehaviour
{
    [SerializeField] private GameObject oculusParent;
    [SerializeField] private GameObject oculusGazeObject;

    [SerializeField] private GameObject picoParent;
    [SerializeField] private GameObject picoGazeObject;

    [SerializeField] private GameObject faceObject;

    [SerializeField] private GameObject debugLine;
    [SerializeField] private GameObject picoSprite;
    [SerializeField] private GameObject ocSprite;

    private static Transform eyeTrackingForward;
    public static Transform EyeTrackingForward { get => eyeTrackingForward; }
    private Camera mainPlayerCamera;

    private List<FaceTrackingNode> faceNodesActive = new List<FaceTrackingNode>();
    private List<EyeTrackingNode> eyeNodesActive = new List<EyeTrackingNode>();

    public void End(ExecutableNode caller)
    {
        if (caller as EyeTrackingNode != null)
        {
            EyeTrackingNode eyeTrackingNode = caller as EyeTrackingNode;
            bool showUi = false;

            eyeNodesActive.Remove(eyeTrackingNode);

            for (int i = 0; i < eyeNodesActive.Count; i++)
            {
                if (eyeNodesActive[i].showLookingAtUI)
                {
                    showUi = true;
                }
            }

            picoSprite.SetActive(showUi);
            ocSprite.SetActive(showUi);

            if (eyeNodesActive.Count == 0)
            {
                oculusParent.SetActive(false);
                picoParent.SetActive(false);
                eyeNodesActive.Clear();
                eyeNodesActive = new List<EyeTrackingNode>();
            }
        }

        if (caller as FaceTrackingNode != null)
        {
            FaceTrackingNode faceTrackingNode = caller as FaceTrackingNode;
            faceNodesActive.Remove(faceTrackingNode);
            if (faceNodesActive.Count == 0)
            {
                faceObject.SetActive(false);
                faceNodesActive.Clear();
                faceNodesActive = new List<FaceTrackingNode>();
            }
        }
    }

    public void EndNodeless()
    {
        oculusParent.SetActive(false);
        picoParent.SetActive(false);
        picoSprite.SetActive(false);
        ocSprite.SetActive(false);
        faceObject.SetActive(false);
    }

    public void Begin(Action<EyeTrackingFrame> CollectDataEye, EyeTrackingNode caller)
    {
        CheckInitialize();
        if (CollectDataEye != null)
            TrackingFrameComponent.CollectDataEye += CollectDataEye;

        picoSprite.SetActive(caller.showLookingAtUI);
        ocSprite.SetActive(caller.showLookingAtUI);
        eyeNodesActive.Add(caller);
    }

    public void Begin(Action<FaceTrackingFrame> CollectDataFace, FaceTrackingNode caller)
    {
        CheckInitialize();
        if (CollectDataFace != null)
        {
            TrackingFrameComponent.CollectDataFace += CollectDataFace;
        }

        faceNodesActive.Add(caller);
    }
    /// <summary>
    /// Used in the situation where we are not recording from a node - ie from the emote recoring project
    /// </summary>
    public void BeginNodeless(Action<FaceTrackingFrame> CollectDataFace)
    {
        CheckInitialize();
        if (CollectDataFace != null)
            TrackingFrameComponent.CollectDataFace += CollectDataFace;
    }

    private void CheckInitialize()
    {
        if (mainPlayerCamera == null)
            Initialize();

        ActivateTrackingComponents();

        if (eyeTrackingForward == null)
            QueryLookAt();
    }

    private void Initialize()
    {
        mainPlayerCamera = Camera.main;
        transform.parent = mainPlayerCamera.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        HeadsetDetection.Initialize();
    }

    private void QueryLookAt()
    {
        eyeTrackingForward = mainPlayerCamera.transform;

        if (HeadsetDetection.headsetType == HEADSET_TYPE.QUEST ||
            HeadsetDetection.headsetType == HEADSET_TYPE.PICO)
        {
            eyeTrackingForward = mainPlayerCamera.transform;
        }
        else
        {
            if (HeadsetDetection.headsetType == HEADSET_TYPE.QUEST_EYE || HeadsetDetection.headsetType == HEADSET_TYPE.QUEST_EYE_FACE)
            {
                eyeTrackingForward = oculusGazeObject.transform;
            }

            if (HeadsetDetection.headsetType == HEADSET_TYPE.PICO_EYE || HeadsetDetection.headsetType == HEADSET_TYPE.PICO_EYE_FACE)
            {
                eyeTrackingForward = picoGazeObject.transform;
            }
        }
    }


    private void ActivateTrackingComponents()
    {
        oculusParent.SetActive(HeadsetDetection.headsetType == HEADSET_TYPE.QUEST_EYE || HeadsetDetection.headsetType == HEADSET_TYPE.QUEST_EYE_FACE);
        picoParent.SetActive(HeadsetDetection.headsetType == HEADSET_TYPE.PICO_EYE || HeadsetDetection.headsetType == HEADSET_TYPE.PICO_EYE_FACE);
        faceObject.SetActive(HeadsetDetection.headsetType == HEADSET_TYPE.PICO_EYE_FACE || HeadsetDetection.headsetType == HEADSET_TYPE.QUEST_EYE_FACE);
    }

    private void Update()
    {
        if (eyeTrackingForward == null)
            QueryLookAt();

        if (Application.isEditor) //TODO integrate this with controllers too
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                debugLine.SetActive(!debugLine.activeInHierarchy);
            }
        }
    }

}
