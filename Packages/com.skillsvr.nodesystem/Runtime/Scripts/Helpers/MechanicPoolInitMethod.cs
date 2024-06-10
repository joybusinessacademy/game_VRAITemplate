using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class MechanicPoolInitMethod
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitMechanicPool()
    {
        var poolInitController = GameObject.FindObjectOfType<MechanicPoolInitController>();
        if (null == poolInitController)
        {
            GameObject rootObj = new GameObject(nameof(MechanicPoolInitController));
            poolInitController = rootObj.AddComponent<MechanicPoolInitController>();
        }
        if (!poolInitController.initOnAwake && !poolInitController.isInProcess)
        {
            poolInitController.InitPool();
        }
    }
}