using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RegisterMechanicEventParameterType(OnItemSlotted, typeof(GameObject))]
public enum RankOrderSystemEvent
{
	None,
	OnItemSlotted,
	OnIncorrectSlotted,
	OnGetSlottedItems,
	OnFinished,
	OnCorrectFinished,
	OnIncorrectFinished
}

[RegisterMechanicEvent(enumEventType = typeof(RankOrderSystemEvent))]
public interface IRankOrderSystem : IMechanicSystem<RankOrderSystemData>
{

}
