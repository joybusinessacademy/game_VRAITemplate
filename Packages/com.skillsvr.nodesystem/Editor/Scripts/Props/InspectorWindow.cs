using GraphProcessor;
using Scripts.Props;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Props
{
    public class InspectorWindow : EditorWindow
    {
        public InspectorVisualElement inspectorVisualElement;

        public void CreateGUI()
        {
            titleContent = new UnityEngine.GUIContent("Inspector", Resources.Load<Texture2D>("Icon/Inspector"));

            inspectorVisualElement = new InspectorVisualElement();
            rootVisualElement.Add(inspectorVisualElement);
        }

        [MenuItem("SkillsVR CCK/Inspector")]
        public static void OpenWindow()
        {
            GetWindow<InspectorWindow>();
        }

        public static void SetSelectedNode(BaseNodeView node)
        {
            if (!HasOpenInstances<InspectorWindow>())
            {
                return;
            }

            InspectorWindow window = GetWindow<InspectorWindow>(false, "Inspector", false);
            window.inspectorVisualElement?.SetNode(node);
        }

        public static void InspectNode(BaseNodeView nodeTarget)
        {
            var window = GetWindow<InspectorWindow>();
            window.inspectorVisualElement.SetNode(nodeTarget);
            window.Focus();
        }

		private void OnDestroy()
		{
            if (inspectorVisualElement != null)
                inspectorVisualElement.Reset();
		}
	}
}