using System;
using System.Collections.Generic;
using DialogExporter;
using UnityEngine;


[Serializable]
[CreateAssetMenu(menuName = "Mechanics/MCQMechanicData")]
public class MultipleChoiceQuestionScriptable : MechanicDataScriptable
{
	[Header("Visual State")]
	public bool isVisualOnSpawn = true;

	[Header("Question Data")]
	public LocalisedString questionTitle = new LocalisedString("New Question");

	public List<MultipleChoiceAnswer> questions = new List<MultipleChoiceAnswer>();
	public MultipleChoiceFeedback multipleChoiceFeedback;
	public bool endOnIncorrectAnswer = true;
	
	public bool allowMultipleSelection = false;
	//public bool allowMultipleSelection => minSelectionAmount > 1;
	public int minSelectionAmount = 1;
	

	[Header("Sound State")]
	public bool useSounds = false;

	[Header("Mechanic Extras")]
	public float delayTimeUntilTurnedOff = 5f;
	public bool shuffleAnswers = true;
}
