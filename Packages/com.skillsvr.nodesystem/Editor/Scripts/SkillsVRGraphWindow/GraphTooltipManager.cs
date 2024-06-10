using SkillsVR.UnityExtenstion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GraphTooltipManager
{
	private static List<string> unusedPriorityTips;
	private static int tipIndex = 0;

	public static List<string> genericHelperTips = new List<string>
	{
		"Use middle mouse button to move around the Graph",
		"Use (Space Bar) or (Right Click) to open the Node Creation Graph Menu",
		"(Alt + Left Click) to move around the graph",
		"Left click and drag to select multiple items at once",
		"Press (F Key) to center the graph"
	};

	public static List<string> priorityHelperTips = new List<string>();

	public static string GetRandomTip()
	{
		if (unusedPriorityTips == null)
			unusedPriorityTips = new List<string>(priorityHelperTips);

		string helperTip = "";

		if (unusedPriorityTips.Count > 0)
		{
			helperTip = unusedPriorityTips[0];
			unusedPriorityTips.RemoveAt(0);
		}

		tipIndex++;

		if(tipIndex >= genericHelperTips.Count)
		{
			genericHelperTips.Shuffle();
			tipIndex = 0;
			helperTip = genericHelperTips[0];
		}
		else
			helperTip = genericHelperTips[tipIndex];

		return helperTip;
	}
}
