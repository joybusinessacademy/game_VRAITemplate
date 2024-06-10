using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RegisterMechanicEventParameterType(OnChoiceSelected, typeof(int))]
[RegisterMechanicEventParameterType(OnChoiceUnselected, typeof(int))]
[RegisterMechanicEventParameterType(OnSelectionResult, typeof(Dictionary<int, bool>))]
public enum MCQEvent
{
	None,
	OnChoiceSelected,
	FinishedMCQ,
	CorrectButton,
	InCorrectButton,
	MultipleSelectionCorrect,
	MultipleSelectionIncorrect,
	OnChoiceUnselected,
	OnSelectionResult,
}

[RegisterMechanicEvent(enumEventType = typeof(MCQEvent))]
public interface IMultipleChoiceQuestionSystem : IMechanicSystem<MultipleChoiceQuestionScriptable>
{
}
