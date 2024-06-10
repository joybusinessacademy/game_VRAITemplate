using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportCheckHandler : MonoBehaviour
{
	private XRSimpleInteractable interactable;

	public Action OnTeleported { get; internal set; }

	private void Awake()
	{
		interactable = GetComponent<XRSimpleInteractable>();
	}

	private void OnEnable()
	{
		interactable.onSelectEntered.AddListener(OnSelectEntered);
	}

	private void OnSelectEntered(XRBaseInteractor arg0)
	{
		OnTeleported?.Invoke();
	}

	private void OnDisable()
	{
		interactable.onSelectEntered.RemoveListener(OnSelectEntered);
	}
}
