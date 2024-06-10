using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using UnityEngine;

public class TeleportSystem : AbstractMechanicSystemBehivour<TeleportData>, ITeleportSystem
{
	public GameObject visualGameobject;
	public GameObject xrTeleport;
	public TeleportCheckHandler teleportCheck;

	protected override void Awake()
	{
		base.Awake();

		teleportCheck.OnTeleported += OnTeleportSelected;
	}

	protected override void Start()
	{
		base.Start();
	}

	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
		switch (systemEvent.eventKey)
		{
			case MechSysEvent.OnStart: OnMechanicStart(); break;
		}
	}

	protected void OnMechanicStart()
	{
		if (mechanicData != null)
		{
			SwitchOnTeleportType();
		}
	}

	protected void OnMechanicStop()
	{
		DisableTeleport();
	}

	/*
	  TeleportImmediately - 0
      EnableTeleporterAndWait - 1
      EnableTeleporterAndContinue - 2
	*/
	private void SwitchOnTeleportType()
	{
		switch (mechanicData.teleportType)
		{
			case 0:
				TriggerEvent(TeleportEvent.Teleported);
				DisableTeleport();
				break;
			case 1:
				EnableTeleport();
				break;
			case 2:
				EnableTeleport();
				TriggerEvent(TeleportEvent.TeleportContinue);
				break;

			default:
				break;
		}
	}

	public void EnableTeleport()
	{
		visualGameobject.SetActive(true);
		xrTeleport.SetActive(true);

		TriggerEvent(TeleportEvent.TeleportEnabled);
	}

	public void DisableTeleport()
	{
		visualGameobject.SetActive(false);
		xrTeleport.SetActive(false);

		TriggerEvent(TeleportEvent.TeleportDisabled);

		StopMechanic();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		teleportCheck.OnTeleported -= OnTeleportSelected;
	}

	private void OnTeleportSelected()
	{
		TriggerEvent(TeleportEvent.Teleported);
		DisableTeleport();
	}
}