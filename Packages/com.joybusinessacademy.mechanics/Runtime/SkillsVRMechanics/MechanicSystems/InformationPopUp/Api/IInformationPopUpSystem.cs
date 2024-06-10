using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RegisterMechanicEventParameterType(OnTimeOut, typeof(float))]
public enum InformationPopUpEvent
{
	None,
	OnPopupFinished,
	OnSetCustomTime,
	OnTimeOut,
	OnButtonClick,
	OnMarkerButtonClick
}
[RegisterMechanicEvent(enumEventType = typeof(InformationPopUpEvent))]
public interface IInformationPopUpSystem : IMechanicSystem<InfoPopUpDatas>
{ 

}
