using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SkillsVR.EventExtenstion;
using SkillsVR.OdinPlaceholder; 

public class SnapPoint : MonoBehaviour
{
    [ReadOnly, ShowInInspector]
    public SnapPointItem currentItem;

    [ReadOnly, ShowInInspector]
    protected HashSet<SnapPointItem> inRangeItems = new HashSet<SnapPointItem>();

    public UnityEvent onSelectEnter = new UnityEvent();
    public UnityEvent onSelectExit = new UnityEvent();

    public UnityEventGameObject onItemAttach = new UnityEventGameObject();
    public UnityEventGameObject onItemDetach = new UnityEventGameObject();

    public UnityEventGameObject onItemSlotted = new UnityEventGameObject();

    public delegate void OnIncorrectSlotted();
    public OnIncorrectSlotted incorrectSlotted;

    public UnityEvent onAnyItemSlotted = new UnityEvent();

    private bool enableDetach = true;

    public int snapPointID = -1;

    public List<GameObject> stackedObjects = new List<GameObject>();

    public bool requireSameID = false;

    public Vector3 GetSnapPos()
    {
        return this.transform.position;
    }

    public Vector3 GetSnapAngle()
    {
        return this.transform.eulerAngles;
    }

    public void CheckCanDetachCurrentItem(GameObject itemPicked)
    {
        if (null == currentItem)
        {
            enableDetach = true;
            return;
        }
        if (null == itemPicked)
        {
            return;
        }
        var item = itemPicked.GetComponentInChildren<SnapPointItem>();
        if (null == item)
        {
            return;
        }
        enableDetach = item == currentItem;
    }

    public void EnableDetach(bool detachEnabled)
    {
        enableDetach = detachEnabled;
    }

    public void AddItemToRange(SnapPointItem item)
    {
        if (item == currentItem)
        {
            return;
        }
        if (null != item && !inRangeItems.Contains(item))
        {
            item.inRangedSnapPoint = this;
            inRangeItems.Add(item);
            stackedObjects.Add(item.gameObject);
            TriggerSelectEvents();
        }
    }

    public void RemoveItemFromRange(SnapPointItem item)
    {
        if (null != item && inRangeItems.Contains(item))
        {
            item.inRangedSnapPoint = null;
            inRangeItems.Remove(item);

            if(stackedObjects.Contains(item.gameObject))
                stackedObjects.Remove(item.gameObject);

            TriggerSelectEvents();
        }
    }

    [Button]
    public void OnColliderEnter(Collider collider)
    {
        if (null == collider)
        {
            return;
        }
        SnapPointItem item = collider.GetComponent<SnapPointItem>();
        OnSnapPointItemEnter(item);
    }

    public void OnColliderStay(Collider collider)
    {
        if (null == collider)
        {
            return;
        }
        SnapPointItem item = collider.GetComponent<SnapPointItem>();
        OnSnapPointItemStay(item);
    }

    [Button]
    public void OnColliderExit(Collider collider)
    {
        if (null == collider)
        {
            return;
        }
        SnapPointItem item = collider.GetComponent<SnapPointItem>();
        OnSnapPointItemExit(item);
    }

    public void OnCollider2DEnter(Collider2D collider)
    {
        if (null == collider)
        {
            return;
        }
        SnapPointItem item = collider.GetComponent<SnapPointItem>();
        OnSnapPointItemEnter(item);
    }

    public void OnCollider2DStay(Collider2D collider)
    {
        if (null == collider)
        {
            return;
        }
        SnapPointItem item = collider.GetComponent<SnapPointItem>();
        OnSnapPointItemStay(item);

    }

    [Button]
    public void OnCollider2DExit(Collider2D collider)
    {
        if (null == collider)
        {
            return;
        }
        SnapPointItem item = collider.GetComponent<SnapPointItem>();
        OnSnapPointItemExit(item);
    }

    private void OnSnapPointItemEnter(SnapPointItem item)
    {
        if (null == item)
        {
            return;
        }

        if (null != item.inRangedSnapPoint)
        {
            item.inRangedSnapPoint.RemoveItemFromRange(item);
        }
        AddItemToRange(item);
    }

    private void OnSnapPointItemStay(SnapPointItem item)
    {
        if (null == item)
        {
            return;
        }
        if (null == item.inRangedSnapPoint)
        {
            AddItemToRange(item);
        }

    }

    private void OnSnapPointItemExit(SnapPointItem item)
    {
        if (null == item)
        {
            return;
        }

        RemoveItemFromRange(item);

        if (currentItem == item)
        {
            DetachItem(currentItem, false);
        }
    }

    [Button]
    private void TriggerSelectEvents()
    {
        if (inRangeItems.Count > 0)
        {
            onSelectEnter?.Invoke();
        }
        else
        {
            onSelectExit?.Invoke();
        }
    }

    [Button]
    public void OnReceiveItemSelectExit(GameObject targetObject)
    {
        if (null == targetObject)
        {
            return;
        }

        SnapPointItem item = targetObject.GetComponent<SnapPointItem>();
        if (null == item)
        {
            return;
        }

        if (!inRangeItems.Contains(item))
        {
            return;
        }

        if (null != currentItem && item != currentItem)
        {
            DetachItem(currentItem, true, true);
        }

        AttachItem(item);
    }

    private void AttachItem(SnapPointItem item)
    {
        if (null == item)
        {
            return;
        }

        if (requireSameID && (item.GetComponent<CardSelectionUIItem>().slotNumberID != snapPointID))
		{
            incorrectSlotted?.Invoke();
            return;
		}

        if (item.currentSnapPoint != null)
        {
            return;
        }

        onItemSlotted?.Invoke(item.gameObject);

        onAnyItemSlotted?.Invoke();

        currentItem = item;

        currentItem.SetAttachTo(this);
        currentItem.Snap();
        onItemAttach?.Invoke(currentItem.gameObject);
        inRangeItems.Remove(item);
        TriggerSelectEvents();
    }

    public void DetachItem(SnapPointItem item, bool snapItem, bool forceDetach = false)
    {
        if (null == item || item != currentItem || null == currentItem)
        {
            return;
        }
        if (!enableDetach && !forceDetach)
        {
            return;
        }
        item.SetAttachTo(null);
        onItemDetach?.Invoke(item.gameObject);
        if (snapItem)
        {
            item.Snap();
        }
        currentItem = null;
    }
}
