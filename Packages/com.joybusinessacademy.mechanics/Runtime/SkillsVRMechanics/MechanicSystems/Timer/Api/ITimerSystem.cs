using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RegisterMechanicEventParameterType(None, typeof(string))]
public enum TimerSystemEvent
{
	None,
	OnTimerFinished
}

[RegisterMechanicEvent(enumEventType = typeof(TimerSystemEvent))]
public interface ITimerSystem : IMechanicSystem<TimerData>
{

}