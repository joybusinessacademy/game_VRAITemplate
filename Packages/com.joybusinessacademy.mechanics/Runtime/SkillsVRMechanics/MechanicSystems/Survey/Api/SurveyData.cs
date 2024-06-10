using DialogExporter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SurveyScaleType
{
	LIKERT,
	PERCENTAGE
}

[Serializable]
public class SurveyData
{
	public SurveyScaleType surveyScaleType = SurveyScaleType.LIKERT;
	public LocalisedString instructionText = new("");
	public List<SurveyLikertData> likertLabels = new List<SurveyLikertData>();
	public List<SurveyStatementPromptData> statementPrompts= new List<SurveyStatementPromptData>();
	public LocalisedString finishedButtonLabel = new("Done");

	public LocalisedString prefixLabel = new("");
	public LocalisedString postFixLabel = new("%");
}

[Serializable]
public class SurveyLikertData
{
	public LocalisedString likertString = "";
}

[Serializable]
public class SurveyStatementPromptData
{
	public LocalisedString statementString = "";
	public LocalisedString learnMoreString = "";
	public AudioClip learnMoreAudio;
}