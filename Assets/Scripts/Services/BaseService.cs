using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseService<T> : AbstractService<T> where T : Component
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        service = new GameObject(nameof(T)).AddComponent<T>();
        GameObject.DontDestroyOnLoad(service.gameObject);
    }
}
