using SkillsVR.Mechanic.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnerRankOrder : AbstractMechanicSpawner<IRankOrderSystem, RankOrderSystemData>, IRankOrderSystem
{
    public override string mechanicKey => "RankOrderQuestionSystem";

	public Dictionary<int, List<RankOrderItemData>> slottedItems = new Dictionary<int, List<RankOrderItemData>>();

	protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
	{
		switch (systemEvent.eventKey)
		{
			case RankOrderSystemEvent.OnGetSlottedItems: this.slottedItems = systemEvent.GetData<Dictionary<int, List<RankOrderItemData>>>(); break;
		}
	}
}
