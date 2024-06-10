using SkillsVR.Mechanic.Core;
using SkillsVR.Mechanic.Core.Impl;
using SkillsVR.UnityExtenstion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkillsVR.Mechanic.MechanicSystems.PanelImage
{
	public class PanelImageSystem : AbstractMechanicSystemBehivour<PanelImageData>, IPanelImageSystem
	{
		[Header("References")]
		public RawImage rawImage;
		public Button nextButton;
		public TextMeshProUGUI nextButtonText;

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

			SetNextButton();
			
			rawImage.texture = mechanicData.image;
			nextButtonText.text = mechanicData.nextButtonText;
		}
		
		private void SetNextButton()
		{
			nextButton.gameObject.SetActive(mechanicData.showNextButton);
			nextButton.onClick.AddListener(OnSkipButtonPressed);
		}

		private void OnSkipButtonPressed()
		{
			FinishedImage();
		}

		public override void SetVisualState(bool show)
		{
			gameObject.transform.GetChild(0).gameObject.SetActive(show);
			base.SetVisualState(show);
		}

		private void FinishedImage()
		{
			TriggerEvent(PanelImageEvent.ImageFinished);
			StopMechanic();
		}

		protected override void Update()
		{
			base.Update();

			if (mechanicData.showNextButton)
			{
				return;
			}
			

			currentTime += Time.deltaTime;

			if (mechanicData.imageDuration <= currentTime)
			{
				FinishedImage();
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			
			if (nextButton != null)
			{
				nextButton.onClick.RemoveListener(OnSkipButtonPressed);
			}
		}
	}
}
