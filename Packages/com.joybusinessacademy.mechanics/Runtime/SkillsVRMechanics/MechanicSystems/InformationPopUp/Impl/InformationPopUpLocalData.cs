using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InformationPopUpLocalData
{
	[Header("Attachment Reference")]
	public bool attachedToObject = false;
	public Transform objectAttachment;
	public Vector3 attachmentPosition;

	[Header("Line Linker Offsets")]
	public Vector3 offsetPosition1;
	public Vector3 offsetPosition2;
	public Vector3 offsetPosition3;
}
