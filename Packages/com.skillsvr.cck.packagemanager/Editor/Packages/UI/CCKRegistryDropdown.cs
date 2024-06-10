using SkillsVR.CCK.PackageManager.Managers;
using SkillsVR.CCK.PackageManager.Registry;
using SkillsVR.CCK.PackageManager.Settings;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI
{
    // Make a dropdown to show names of all registries added in project settings.
    // When select the registry name, 
    // Try to get cck registry from registry manager.
    public class CCKRegistryDropdown : VisualElement, INotifyValueChanged<CCKRegistry>
    {
        public new class UxmlFactory : UxmlFactory<CCKRegistryDropdown, UxmlTraits> { }

        public CCKRegistry RegistryValue { get; protected set; }
        public CCKRegistry value
        {
            get => RegistryValue;
            set
            {
                if (RegistryValue == value)
                {
                    return;
                }
                var prevValue = RegistryValue;
                var currValue = value;
                SetValueWithoutNotify(value);
                this.SendValueChangedEvent(prevValue, currValue);
                var prevName = null == prevValue ? null : prevValue.name;
                var currName = null == value ? null : value.name;
                this.SendValueChangedEvent(prevName, currName);
            }
        }

        protected DropdownField registryNameDropdown = new DropdownField();

        public CCKRegistryDropdown()
        {
            this.Add(registryNameDropdown);
            registryNameDropdown.RegisterValueChangedCallback<string>(OnSelectedRegistryNameChanged);
            ReloadOptions();
        }

        public void ReloadOptions()
        {
            var registrySources = CCKProjectSettingsPackageManager.GetSettings()
                .registries
                .Where(r => null != r
                        && !string.IsNullOrWhiteSpace(r.url)
                        && !string.IsNullOrWhiteSpace(r.name));
            var names = registrySources.Select(x => x.name).ToList();
            registryNameDropdown.choices = names;
        }

        public void SetValueWithoutNotify(CCKRegistry newValue)
        {
            if (null == newValue || null == newValue.Source)
            {
                RegistryValue = null;
                registryNameDropdown.SetValueWithoutNotify("null");
                return;
            }
            var registry = CCKRegistryManager.Main.AddRegistry(newValue.Source);
            RegistryValue = registry;
            registryNameDropdown.SetValueWithoutNotify(registry.Source.name);
        }

        public void SelectByName(string newName)
        {
            value = CCKRegistryManager.Main.GetRegistry(x => null != x.Source && x.Source.name == newName);
        }

        public void SelectByIndex(int index)
        {
            if (null == registryNameDropdown.choices || index < 0 || index >= registryNameDropdown.choices.Count)
            {
               return;
            }
            var item = registryNameDropdown.choices[index];
            SelectByName(item);
        }

        protected void OnSelectedRegistryNameChanged(ChangeEvent<string> evt)
        {
            SelectByName(evt.newValue);
        }
    }
}