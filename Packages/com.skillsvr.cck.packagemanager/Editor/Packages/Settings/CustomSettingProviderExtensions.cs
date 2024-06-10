using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.Settings
{
    public static class CustomSettingProviderExtensions
    {
        public static SettingsProvider CreateSettingsProvider(this CustomSettings customSettings)
        {
            var provider = new SettingsProvider(customSettings.Path, customSettings.Scopes, customSettings.Keywords)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = customSettings.Name,
                activateHandler = (searchContext, rootElement) =>
                {
                    // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                    // isn't called because the SettingsProvider uses the UIElements drawing framework.
                    var title = new Label()
                    {
                        text = customSettings.Name
                    };
                    title.AddToClassList("title");
                    rootElement.Add(title);
                    var properties = new VisualElement()
                    {
                        style =
                    {
                        flexDirection = FlexDirection.Column
                    }
                    };
                    properties.AddToClassList("property-list");
                    rootElement.Add(properties);

                    var fields = customSettings.GetType()
                        .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                        .Where(f => !f.IsDefined(typeof(HideInInspector), true));
                    if (!string.IsNullOrWhiteSpace(searchContext))
                    {
                        fields = fields.Where(f => f.Name.Contains(searchContext));
                    }

                    var settings = customSettings.CreateSerializedObject();
                    foreach (var f in fields)
                    {
                        properties.Add(new PropertyField(settings.FindProperty(f.Name)));
                    }
                    rootElement.Bind(settings);

                    rootElement.RegisterCallback<DetachFromPanelEvent>((evt) => {
                        customSettings.Save();
                        customSettings = null;
                    });
                    rootElement.RegisterCallback<KeyUpEvent>((evt) => {
                        if (KeyCode.S == evt.keyCode && evt.actionKey)
                        {
                            customSettings.Save();
                        }
                    });
                },
            };

            return provider;
        }
    }
}