using CrazyMinnow.SALSA.Timeline;
using Samples.Editor.General;
using System.Collections;
using System.Reflection;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TrackAutoBinder
{
    GameObject charater;

    public TrackAutoBinder(GameObject _charater)
    {
        charater = _charater;
    }

    public void StartLateBind<T>(Object asset) where T : Component
    {
        EditorCoroutineUtility.StartCoroutine(LateBind<T>(asset), this);
    }
   
    private IEnumerator LateBind<T>(Object asset) where T : Component
    {
        yield return new WaitForEndOfFrame();

        if (asset != null)
        {
            if (null == TimelineAssistant.currentAsset)
            {
                TimelineAssistant.currentAsset = TimelineEditor.inspectedAsset;
            }
            if (null == TimelineAssistant.currentDirector)
            {
                TimelineAssistant.currentDirector = TimelineEditor.inspectedDirector;
            }

            foreach (var track in TimelineAssistant.currentAsset.GetOutputTracks())
            {
                if (track is EmoterControlTrack && asset is EmoterControl)
                {
                    BindComponent<T>(track);

                }
                else if (track is LookAtTrack && asset is LookAtAsset)
                {
                    BindComponent<T>(track);

                }
                else if (track is SalsaControlTrack && asset is SalsaControl)
                {

                    BindComponent<T>(track);


                }
                else if (track is AnimationTrack && (asset is AnimationStateAsset || asset is AnimationPlayableAsset))
                {
                    (track as AnimationTrack).trackOffset = TrackOffset.ApplySceneOffsets;
                    (track as AnimationTrack).position = charater.transform.position;
                    //TimelineAssistant.currentDirector.ClearGenericBinding(track);
                    yield return null;
					CustomBindComponent<T>(track);
                }
            }
        }
    }

    private void BindComponent<T>(TrackAsset track) where T : Component
    {
        var binding = TimelineAssistant.currentDirector.GetGenericBinding(track);
        var componentBinding = binding as T;

        if (componentBinding == null || componentBinding.gameObject == charater)
            TimelineAssistant.currentDirector.SetGenericBinding(track, charater.GetComponent<T>());
    }

	private void CustomBindComponent<T>(TrackAsset track) where T : Component
	{
		var binding = TimelineAssistant.currentDirector.GetGenericBinding(track);
		var componentBinding = binding as T;

		if (componentBinding == null || componentBinding.gameObject == charater)
		{
			// The director.SetGenericBinding() only make bind but not update editor cache for track preview.
			// so use UnityEditor.Timeline.BindingUtility.BindWithInteractiveEditorValidation() instead.
			TimelineAssistant.currentDirector.BindWithInteractiveEditorValidation(track, charater.GetComponent<T>());
		}
	}

}

static class DirectorEx
{
    public static void BindWithInteractiveEditorValidation(this PlayableDirector director, TrackAsset track, UnityEngine.Object bindObj)
    {
        director
            .GetBindingUtility()
            .CallStaticMethod("BindWithInteractiveEditorValidation", director, track, bindObj);
    }

    private static System.Type BindingUtilityClassType;
    private static System.Type GetBindingUtility(this PlayableDirector director)
    {
        if (null != BindingUtilityClassType)
        {
            return BindingUtilityClassType;
        }
        var assesmbies = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assesmbies)
        {
            var t = assembly.GetType("UnityEditor.Timeline.BindingUtility");
            if (null == t)
            {
                continue;
            }
            BindingUtilityClassType = t;
            return t;
        }
        return null;
    }
    private static object CallStaticMethod(this System.Type type, string name, params object[] args)
    {
        MethodInfo methodInfo = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        return methodInfo.Invoke(null, args);
    }
}