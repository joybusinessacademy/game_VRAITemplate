using System;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class WaitMonoBehaviour : MonoBehaviour
{
	private static WaitMonoBehaviour waitMonoBehaviour;

	public static Coroutine Process(float time, Action callback)
	{
		time = Mathf.Max(0, time);
		if (0 >= time)
		{
			callback?.Invoke();
			return null;
		}
		InitWaitMono();
		return waitMonoBehaviour.StartCoroutine(_Process(time, callback));
	}

	private static IEnumerator _Process(float time, Action callback)
	{
		yield return new WaitForSeconds(time);
		callback?.Invoke();
	}

	public static void Stop(Coroutine coroutine)
	{
		if (null == waitMonoBehaviour || null == coroutine)
		{
			return;
		}
		waitMonoBehaviour.StopCoroutine(coroutine);
	}

	public static Coroutine ProcessCoroutine(IEnumerator routine)
	{
		if (null == routine)
		{
			return null;
		}
		InitWaitMono();
		return waitMonoBehaviour.StartCoroutine(routine);
	}

	private static void InitWaitMono()
	{
		if (null == waitMonoBehaviour)
		{
			waitMonoBehaviour = GameObject.FindObjectOfType<WaitMonoBehaviour>();
		}
		if (null == waitMonoBehaviour)
		{
			GameObject go = new GameObject("WaitMonoBehaviour");
			waitMonoBehaviour = go.AddComponent<WaitMonoBehaviour>();
			if (Application.isPlaying)
			{
                GameObject.DontDestroyOnLoad(go);
            }
        }
	}
}