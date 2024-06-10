using SkillsVR.Mechanic.MechanicSystems.DeepBreath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathDemoController : MonoBehaviour
{
    [SerializeReference]
    public SpawnerDeepBreath deepBreathSystem;
    public float autoTurnActiveTime = 1.0f;

    protected float timer;

    private void Update()
    {
        if (null == deepBreathSystem)
        {
            return;
        }
        if (!deepBreathSystem.ready)
        {
            return;
        }
        timer += Time.deltaTime;
        if (timer > autoTurnActiveTime)
        {
            timer = 0;
            deepBreathSystem.SetActive(!deepBreathSystem.active);
        }
    }
}
