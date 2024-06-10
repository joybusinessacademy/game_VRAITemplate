using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractService<T> : MonoBehaviour where T : Component
{

    public static T service;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        service = new GameObject(nameof(T)).AddComponent<T>();        
        GameObject.DontDestroyOnLoad(service.gameObject);
    }


}
