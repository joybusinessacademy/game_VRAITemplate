using SkillsVR.UnityExtenstion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdentifier : MonoBehaviour
{
    public string Id { private set; get; }

    public Transform PlayerHead { get => playerHead; }
    private Transform playerHead;  
    public Transform LeftHand { get => leftHand; }
    private Transform leftHand;  
    public Transform RightHand { get => rightHand; }
    private Transform rightHand;

    //private void Awake()
    //{
    //    DontDestroyOnLoad(gameObject);
    //}

    public void Set(string id)
    {
        Id = id;
        playerHead = gameObject.GetChildRecursive("Main Camera").transform;
        leftHand = gameObject.GetChildRecursive("LeftBaseController").transform;
        rightHand = gameObject.GetChildRecursive("RightBaseController").transform;
    }

    // release
    private void OnDestroy()
    {
        PlayerDistributer.OnPlayerDestroyed(Id);
    }
}