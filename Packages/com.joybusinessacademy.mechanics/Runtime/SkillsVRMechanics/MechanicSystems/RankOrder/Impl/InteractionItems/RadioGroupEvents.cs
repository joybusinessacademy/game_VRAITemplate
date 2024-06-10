using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SkillsVR.EventExtenstion;
using SkillsVR.Messeneger;
using SkillsVR.OdinPlaceholder; 

[DisallowMultipleComponent]
public class RadioGroupEvents : MonoBehaviour
{
    public EventKeyObject group;

    public UnityEventGameObject OnSelfSelectEnter = new UnityEventGameObject();
    public UnityEventGameObject OnSelfSelectExit = new UnityEventGameObject();

    public UnityEventGameObject OnOtherSelectEnter = new UnityEventGameObject();
    public UnityEventGameObject OnOtherSelectExit = new UnityEventGameObject();

    [Button]
    public void TriggerSelectEvent(bool select)
    {
        GlobalMessenger.Instance?.Broadcast<GameObject, bool>(group, this.gameObject, select);
    }

    private void Awake()
    {
        OnReceiveSelectEvent(this.gameObject, false);
        OnReceiveSelectEvent(null, false);
        GlobalMessenger.Instance?.AddListener<GameObject, bool>(group, OnReceiveSelectEvent);
    }

    private void OnDestroy()
    {
        GlobalMessenger.Instance?.RemoveListener<GameObject, bool>(group, OnReceiveSelectEvent);
    }

    [Button]
    private void OnReceiveSelectEvent(GameObject trigger, bool selected)
    {
        if (trigger == this.gameObject)
        {
            if (selected)
            {
                OnSelfSelectEnter?.Invoke(trigger.gameObject);
            }
            else
            {
                OnSelfSelectExit?.Invoke(trigger.gameObject);
            }
        }
        else
        {
            if (selected)
            {
                OnOtherSelectEnter?.Invoke(null == trigger? null : trigger.gameObject);
            }
            else
            {
                OnOtherSelectExit?.Invoke(null == trigger ? null : trigger.gameObject);
            }
        }
    }
}
