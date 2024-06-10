using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankOrderVisualStates : MonoBehaviour
{
	public Image rankOrderImage;
	public Outline outline;

	public Color baseColor;
	public Color selectedColor;

	private void Awake()
	{
		outline.enabled = false;
		SetSelectState(false);
	}

	private void OnEnable()
	{
		outline.enabled = false;
		SetSelectState(false);
	}

	public void SetHoverState(bool state)
	{
		outline.enabled = state ? true : false;
	}

	public void SetSelectState(bool state)
	{
		outline.enabled = false;
		rankOrderImage.color = state ? selectedColor : baseColor;
	}
}
