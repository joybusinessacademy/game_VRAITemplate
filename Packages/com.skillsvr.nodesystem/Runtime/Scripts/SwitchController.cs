using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if PICO_XR
using Unity.XR.PXR;
#endif

public class SwitchController : MonoBehaviour
{
    public float waitTime = 10;
    
    // Start is called before the first frame update
    void Start()
    {
        List<Transform> allItems = transform.GetComponentsInChildren<Transform>().ToList();
        if (allItems.Count == 0)
        {
            return;
        }
        
        allItems.RemoveAll(t => t.parent != transform);
        
        foreach (Transform gameObject in allItems)
        {
            if (gameObject == transform)
            {
                continue;
            }
            gameObject.gameObject.SetActive(false);
        }

        string deviceName = GetDeviceName();

        allItems.FirstOrDefault(t => t.name.Contains(deviceName))?.gameObject.SetActive(true);


        StartCoroutine(SleepAfterTime());
    }

    public IEnumerator SleepAfterTime()
    {
        yield return new WaitForSeconds(waitTime);
        
        gameObject.SetActive(false);
    }

    public string GetDeviceName()
    {
#if PICO_XR
        int controller = PXR_Plugin.Controller.UPxr_GetControllerType();

        return controller switch
        {
            3 => "G2Go",
            4 => "NeoG2Go",
            5 => "Neo3",
            _ => "Meta"
        };
#endif
        return "Meta";
    }
}
