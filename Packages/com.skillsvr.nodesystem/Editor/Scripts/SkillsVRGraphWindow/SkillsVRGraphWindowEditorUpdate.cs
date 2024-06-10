using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditorInternal;

[InitializeOnLoad]
public static class SkillsVRGraphWindowEditorUpdate
{
    
    private static Dictionary<GameObject,long> previewsObjects = new Dictionary<GameObject, long>();
    private static long lastUpdateTimeStamp;
    private const long timeStep = 1000000L; // .1 seconds
    private const long timeStepSecond = 10000000L;

    static SkillsVRGraphWindowEditorUpdate()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.update += Update;
    }

    public static void RegisterGameObject(GameObject obj, float length = 5)
    {
        previewsObjects.TryAdd(obj, System.DateTime.Now.Ticks + (long)(timeStepSecond * length));
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange stateChange)
    {
        switch (stateChange)
        {
            case PlayModeStateChange.EnteredPlayMode:
                // Perform object deletion here
                previewsObjects.ToList().ForEach(o => RemoveUpdatePreview(o.Key, true));
                RemoveAllPreviews();
                break;
            
            case PlayModeStateChange.ExitingPlayMode:
                // Exiting play mode
                //Debug.Log("Exiting Play Mode");
                break;
            
            case PlayModeStateChange.EnteredEditMode:
                // Entered edit mode from play mode
                // Debug.Log("Entered Edit Mode");
                break;
            
            case PlayModeStateChange.ExitingEditMode:
                // Perform object deletion here
                previewsObjects.ToList().ForEach(o => RemoveUpdatePreview(o.Key, true));
                RemoveAllPreviews();
                break;
        }
    }

    private static void RemoveAllPreviews()
    {
        // we scan remaining tagged as preview if we missed it
        var previews = GameObject.FindGameObjectsWithTag("EditorOnly").ToList();
        previews.ForEach(preview =>
        {
            if (preview.name.Contains("_PREVIEW")) 
                GameObject.DestroyImmediate(preview);
        });
    }
    
    public static void RemoveUpdatePreview(GameObject g, bool destroy = false)
    {
        previewsObjects.Remove(g);
        if (destroy)
        {
            Object.DestroyImmediate(g);
        }
    }

    private static void Update()
    {
        long currentTicks = System.DateTime.Now.Ticks;
        if (lastUpdateTimeStamp + timeStep > currentTicks)
        {
            return;
        }

        //// add repaint interval
        if (previewsObjects.Count == 0)
        {
            return;
        }

        SceneView.RepaintAll();
		InternalEditorUtility.RepaintAllViews();

		List<KeyValuePair<GameObject, long>> list = previewsObjects.ToList().FindAll(k => k.Value < currentTicks);
        list.ForEach(o => RemoveUpdatePreview(o.Key));

        lastUpdateTimeStamp = System.DateTime.Now.Ticks;
    }
}
