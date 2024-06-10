using System.Collections.Generic;
using SceneNavigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

namespace Scripts.SceneNavigation
{
	public class CameraNavVisualElement : VisualElement
	{
		public CameraNavVisualElement()
		{
			styleSheets.Add(Resources.Load<StyleSheet>("StyleSheets/CameraNav"));
			//Add(new Label("Camera Navigation"));
			
			VisualElement buttonContainer = new VisualElement();
			SetCameraButtons(buttonContainer);
			Add(buttonContainer);
			
			RegisterCallback<DetachFromPanelEvent>(_ => RemoveCameraAssist());
			RegisterCallback<AttachToPanelEvent>(_ => SetupCameraAssist());
		}

		private static void SetCameraButtons(VisualElement buttonContainer)
		{
			Button playerViewButton = new Button(SceneCameraAssistant.SetCameraToPlayer);
			playerViewButton.text = "Player View";
			buttonContainer.Add(playerViewButton);

			Button topDownButton = new Button(SceneCameraAssistant.SetCameraToTopdown);
			topDownButton.text = "Top Down";
			buttonContainer.Add(topDownButton);

			Button thirdPersonButton = new Button(SceneCameraAssistant.SetCameraTo3rdPerson);
			thirdPersonButton.text = "3rd Person";
			buttonContainer.Add(thirdPersonButton);

			Toggle toggle = new Toggle();
			toggle.label = "Pivot Cam";
			toggle.value = SceneCameraAssistant.OverrideCam;
			toggle.RegisterCallback<ChangeEvent<bool>>(evt => SceneCameraAssistant.OverrideCam = evt.newValue);
			
			buttonContainer.Add(toggle);
		}
		
		public static void SetupCameraAssist()
		{
			SceneView.duringSceneGui += SceneCameraAssistant.EventIntercept;
		}
		
		public static void RemoveCameraAssist()
		{
			SceneView.duringSceneGui -= SceneCameraAssistant.EventIntercept;
		}
	}
}