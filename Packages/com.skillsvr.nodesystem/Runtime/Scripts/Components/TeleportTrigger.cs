using SkillsVRNodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [SerializeField] private LayerMask layersChecked;

    public delegate void OnInteract();
    public OnInteract onHover;
    public OnInteract onHoverExit;
    public OnInteract onTrigger;

    private Dictionary<int, GameObject> idsOfHovered = new Dictionary<int, GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == layersChecked)
        {
            if(!idsOfHovered.ContainsKey(other.GetInstanceID()))
            {
                idsOfHovered.Add(other.GetInstanceID(), other.gameObject);
            }
            
            onHover.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (idsOfHovered.ContainsKey(other.GetInstanceID()))
        {
            idsOfHovered.Remove(other.GetInstanceID());
            onHoverExit.Invoke();
        }
    }

    public void Trigger()
    {
        PlayerDistributer.GetPlayer(idsOfHovered[0].GetComponent<PlayerIdentifier>().Id).transform.position = this.gameObject.transform.position;
        PlayerDistributer.GetPlayer(idsOfHovered[0].GetComponent<PlayerIdentifier>().Id).transform.rotation = this.gameObject.transform.rotation;     		
        onTrigger.Invoke();
    }
}
