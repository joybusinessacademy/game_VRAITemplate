using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using UnityEngine;

namespace SkillsVR.Mechanic.MechanicSystems.PanoImage
{
	public class PanoImageSystem : AbstractMechanicSystemBehivour<PanoImageData>, IPanoImageSystem
	{
		[Header("References")]
		public Material material;

		float currentTime = 0;

		protected override void OnReceiveEvent(IMechanicSystemEvent systemEvent)
		{
			switch (systemEvent.eventKey)
			{
				case MechSysEvent.BeforeStart:
					currentTime = 0;
					break;
			}
		}

		public override void SetMechanicData()
		{
			base.SetMechanicData();

			if(material!= null && mechanicData?.image != null) 
				material.SetTexture("_BaseMap", mechanicData.image);
		}

		public override void SetVisualState(bool show)
		{
			gameObject.transform.GetChild(0).gameObject.SetActive(show);
			base.SetVisualState(show);
		}

		private void FinishedImage()
		{
			TriggerEvent(PanoImageEvent.ImageFinished);
			StopMechanic();
		}

		protected override void Update()
		{
			base.Update();

			currentTime += Time.deltaTime;

			if (mechanicData.imageDuration <= currentTime)
			{
				FinishedImage();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
		}
	}
}
