using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RegisterMechanicEventParameterType(OnChoiceSelected, typeof(int))]
public enum MarkerEvent
{
	None,
	OnChoiceSelected,
	MarkerFinished,
	MarkerFinishedCorrect,
	MarkerFinishedIncorrect
}

[RegisterMechanicEvent(enumEventType = typeof(MarkerEvent))]
public interface IMarkerQuestionSystem : IMechanicSystem<MarkerQuestionsData>
{

}
