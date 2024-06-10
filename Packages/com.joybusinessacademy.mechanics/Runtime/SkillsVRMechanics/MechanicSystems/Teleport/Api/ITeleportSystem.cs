using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RegisterMechanicEventParameterType(TeleportEnabled, typeof(int))]
public enum TeleportEvent
{
	None,
	TeleportEnabled,
	TeleportDisabled,
	TeleportContinue,
	Teleported,
}

[RegisterMechanicEvent(enumEventType = typeof(TeleportEvent))]
public interface ITeleportSystem : IMechanicSystem<TeleportData>
{
}