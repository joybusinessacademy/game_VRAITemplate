using Props;
using Scripts.SceneNavigation;
using SkillsVRNodes.Scripts.Hierarchy;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Props
{
	[Overlay(typeof(SceneView), OverlayID, "Snap-to")]
	public class CameraAssistantOverlay : Overlay
	{
		public const string OverlayID = "camera-assistant-overlay";
        
		public override VisualElement CreatePanelContent()
		{
			PropManager.Validate();
			VisualElement root = new VisualElement();
			root.Add(new CameraNavVisualElement());
			//root.Add(new Button(PropsHierarchyWindow.OpenWindow) {text = "Props Hierarchy"}); 

			return root;
		}
    }
}

