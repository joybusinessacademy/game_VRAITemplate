using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using SkillsVR.OdinPlaceholder; 
using System;
using TMPro;
using SkillsVR.EventExtenstion;

public class CardSelectionUIItem : MonoBehaviour
{
    public int index { get; protected set; }

    [ShowInInspector]
    public UnityEngine.Object unityObjectRef { get; set; }

    public UnityEventInt OnIndexChange = new UnityEventInt();

    public UnityEventBool OnStateChange = new UnityEventBool();

    public UnityEvent Pickup = new UnityEvent();
    public UnityEvent DropOff = new UnityEvent();

    [SerializeField]
    private Transform objRefParent;

    [SerializeField]
    private BoxCollider boxCollider;

    public int slotNumberID = -1;
    public Image itemImage;
    public TextMeshProUGUI text;
    public TextMeshProUGUI customTitleText;
    public Image customImage;

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SetIndex(int newIndex)
    {
        index = newIndex;
        OnIndexChange?.Invoke(newIndex);
    }
}