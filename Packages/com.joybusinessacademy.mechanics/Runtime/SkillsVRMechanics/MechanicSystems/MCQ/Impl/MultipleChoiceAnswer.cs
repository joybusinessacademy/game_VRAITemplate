using System;
using DialogExporter;

[Serializable]
public class MultipleChoiceAnswer
{
	public LocalisedString answerText = new("Answer Text");

	public bool isCorrectAnswer = false;
}