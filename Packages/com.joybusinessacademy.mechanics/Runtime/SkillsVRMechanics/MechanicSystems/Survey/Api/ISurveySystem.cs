using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RegisterMechanicEventParameterType(None, typeof(string))]
public enum SurveySystemEvent
{
	None,
	OnTimerFinished
}

[RegisterMechanicEvent(enumEventType = typeof(SurveySystemEvent))]
public interface ISurveySystem : IMechanicSystem<SurveyData>
{

}