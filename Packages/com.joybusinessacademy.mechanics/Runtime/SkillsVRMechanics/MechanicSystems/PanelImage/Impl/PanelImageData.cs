using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SkillsVR.Mechanic.MechanicSystems.PanelImage
{
	[Serializable]
	public class PanelImageData
	{
		public Texture2D image;
		
		public float imageDuration = 10;
		public bool showNextButton = true;
		public string nextButtonText = "Next";
	}
}
