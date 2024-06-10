using SkillsVR.UnityExtenstion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechanicLineLinker : MonoBehaviour
{
	public GameObject startTarget;
	public GameObject endTarget;

	public LineRenderer lineRenderer;

	public Material lineRendererMaterial;

	public Vector3 offsetPosition1;
	public Vector3 offsetPosition2;
	public Vector3 offsetPosition3;

	public MechanicLineLinker() { }

	public MechanicLineLinker (GameObject startTarget, GameObject endTarget)
	{
		this.startTarget = startTarget;
		this.endTarget = endTarget;
	}

	public void Init(GameObject startTarget, GameObject endTarget, Vector3 offsetPosition1, Vector3 offsetPosition2, Vector3 offsetPosition3)
	{
		this.startTarget = startTarget;
		this.endTarget = endTarget;
		this.offsetPosition1 = offsetPosition1;
		this.offsetPosition2 = offsetPosition2;
		this.offsetPosition3 = offsetPosition3;
	}

	void Start()
	{
		lineRenderer = this.GetOrAddComponent<LineRenderer>();
		lineRenderer.positionCount = 3;
		lineRenderer.SetPosition(0, startTarget.transform.position + offsetPosition1);
		lineRenderer.SetPosition(1, startTarget.transform.position + offsetPosition2);
		lineRenderer.SetPosition(2, endTarget.transform.position + offsetPosition3);
		lineRenderer.material = lineRendererMaterial;
		lineRenderer.startWidth = lineRenderer.endWidth = 0.02f;
	}

	void Update()
	{
		lineRenderer.SetPosition(0, startTarget.transform.position + offsetPosition1);
		lineRenderer.SetPosition(1, startTarget.transform.position + offsetPosition2);
		lineRenderer.SetPosition(2, endTarget.transform.position + offsetPosition3);
	}
}
