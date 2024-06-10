using SkillsVR.OdinPlaceholder; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBoxColliderResize : MonoBehaviour
{
	private BoxCollider boxCollider;

	[Button]
	public void SetNewBoxColliderSize()
	{
		boxCollider = this.GetComponent<BoxCollider>();

		RectTransform rectTransform = this.transform as RectTransform;
		Vector3 newBoxSize = new Vector3(Mathf.FloorToInt(rectTransform.sizeDelta.x), Mathf.FloorToInt(rectTransform.sizeDelta.y), 5);
		boxCollider.size = newBoxSize;
	}
}
