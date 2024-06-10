using System.Reflection;
using UnityEngine.UIElements;
using DialogExporter;
using SkillsVR;

namespace SkillsVRNodes.Editor.NodeViews
{

    public class DialogueNodeExportSettingsView : VisualElement
    {
        private DropdownField apiDropdown;
        private int activeSettingsIndex;
        
        private static VisualElement apiContainer;

        public DialogueNodeExportSettingsView()
        {
            apiDropdown = new DropdownField();
            apiDropdown.label = "API: ";
            apiContainer = new VisualElement();

            SetApiDropdownDetails();
            
            Add(apiDropdown);
            Add(apiContainer);
        }

        public void Refresh(bool isActive)
        {
            apiDropdown.style.height = isActive ? 20 : 0;
            apiDropdown.visible = isActive;
        }

        public ITextToSpeechAPI ReturnActiveApi()
        {
            return APIManager.CurrentAPI;
        }
        
        private void SetApiDropdownDetails()
        {
            apiDropdown = new DropdownField();

            if (APIManager.AllAPIs.Count == 0)
            {
                apiDropdown.choices.Add("null");
                return;
            }

            foreach (ITextToSpeechAPI api in APIManager.AllAPIs)
            {
                apiDropdown.choices.Add(api.GetTypeName());
            }
            
            apiDropdown.RegisterCallback<ChangeEvent<string>>(evt =>
            { 
                APIManager.CurrentAPI = APIManager.AllAPIs.Find(t => t.GetTypeName() == evt.newValue);
                Refresh();
            });
            
            apiDropdown.value = APIManager.CurrentAPI.GetTypeName();
            Refresh();
        }

        private void Refresh()
        {
            apiContainer.Clear();

            MethodInfo method = APIManager.CurrentAPI.GetType().GetMethod("VisualElement");
            
            if (method != null && method.ReturnType == typeof(VisualElement))
            {
                object visualElement = method.Invoke(APIManager.CurrentAPI, new object[] { });
                apiContainer.Add(visualElement as VisualElement);
            }
        }
    }
}
