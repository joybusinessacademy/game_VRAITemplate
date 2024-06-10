using GraphProcessor;
using Props;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews.Telemetry
{
    [NodeCustomEditor(typeof(EyeTrackingNode))]
    public class EyeTrackingNodeView : BaseNodeView
    {

        public Dictionary<string, int> completionPresets = new Dictionary<string, int>()
        {
            { "Look at Once" ,0 },
            { "Look at for a Time",1 },
        };

        public Dictionary<string, int> completionLogic = new Dictionary<string, int>()
        {
            { "All Objects" ,0 },
            { "Any Object",1 },
        };
        public EyeTrackingNode AttachedNode => nodeTarget as EyeTrackingNode;

        VisualElement mainbody = new VisualElement();
        VisualElement conditional = new VisualElement();
        ListDropdown<InteractableObjectData> objectData;

        public override VisualElement GetNodeVisualElement()
        {
            return null;
        }

        public override VisualElement GetInspectorVisualElement()
        {
            VisualElement inspectorVisuals = new VisualElement();

            var preset = CreateDropdownFromDictionary("Select Completion Preset: ", completionPresets, AttachedNode.currentCompletePreset,
            (evt) =>
            {
                AttachedNode.currentCompletePreset = completionPresets[evt.newValue];

                conditional.Add(SetConditional());
                inspectorVisuals.Add(conditional);
            });
            inspectorVisuals.Add(preset);

            var logic = CreateDropdownFromDictionary("Select Completion Logic: ", completionLogic, AttachedNode.currentCompleteLogic,
            (evt) =>
            {
                AttachedNode.currentCompleteLogic = completionLogic[evt.newValue];
            });
            inspectorVisuals.Add(logic);
            inspectorVisuals.Add(SetMainBody());
            conditional.Add(SetConditional());
            inspectorVisuals.Add(conditional);

            return inspectorVisuals;
        }

        private VisualElement SetMainBody()
        {
            mainbody = new VisualElement();
            mainbody.Add(new Divider());
            mainbody.Add(AttachedNode.CustomToggle(nameof(AttachedNode.UseRaycast)));
            mainbody.Add(AttachedNode.CustomToggle(nameof(AttachedNode.showLookingAtUI)));

            objectData = new ListDropdown<InteractableObjectData>("All Props", AttachedNode.interactableObjectDatas, ObjectBox);
            mainbody.Add(objectData);
            return mainbody;
        }

        private VisualElement SetConditional()
        {
            conditional.Clear();
            var localContainer = new VisualElement();
            switch (AttachedNode.currentCompletePreset)
            {
                case 0:

                    if (GetPortViewsFromFieldName("OnSuccess") != null)
                    {
                        GetPortViewsFromFieldName("OnSuccess").ForEach(k =>
                        {
                            k.SetEnabled(false);
                            k.GetEdges().ForEach(edge => edge.style.display = DisplayStyle.None);
                        });
                    }
                    if (GetPortViewsFromFieldName("OnFail") != null)
                    {
                        GetPortViewsFromFieldName("OnFail").ForEach(k =>
                        {
                            k.SetEnabled(false);
                            k.GetEdges().ForEach(edge => edge.style.display = DisplayStyle.None);
                        });
                    }

                    break;
                case 1:

                    localContainer.Add(AttachedNode.CustomFloatField(nameof(AttachedNode.targetLookingTime)));
                    localContainer.Add(AttachedNode.CustomFloatField(nameof(AttachedNode.totalTimeToLookAt)));
                    



                    if (GetPortViewsFromFieldName("OnSuccess") != null)
                    {
                        GetPortViewsFromFieldName("OnSuccess").ForEach(k =>
                        {
                            k.SetEnabled(true);
                            k.GetEdges().ForEach(edge => edge.style.display = DisplayStyle.Flex);
                        });
                    }

                    if (GetPortViewsFromFieldName("OnFail") != null)
                    {
                        GetPortViewsFromFieldName("OnFail").ForEach(k =>
                        {
                            k.SetEnabled(true);
                            k.GetEdges().ForEach(edge => edge.style.display = DisplayStyle.Flex);
                        });
                    }
                    break;

            }
            //mainbody.Add(conditional);
            return localContainer;
        }

        private VisualElement ObjectBox(InteractableObjectData interactableObjectData)
        {
            VisualElement objectContainer = new()
            {
                style =
                {
                    flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row)
                }
            };

            int index = AttachedNode.interactableObjectDatas.IndexOf(interactableObjectData);
            while (AttachedNode.objectInteractableInterfaces.Count <= index)
            {
                AttachedNode.objectInteractableInterfaces.Add(new PropGUID<IBaseProp>());
            }

            PropGUID<IBaseProp> currentValue = AttachedNode.objectInteractableInterfaces[index];
            PropDropdown<IBaseProp> sceneElementDropdown = new("Prop: ", currentValue, newValue =>
            {
                AttachedNode.objectInteractableInterfaces[index] = newValue;
            });

            objectContainer.Add(sceneElementDropdown);


            IconButton delete = new IconButton("Close")
            {
                tooltip = "Delete",
            };

            delete.clicked += () =>
            {
                AttachedNode.interactableObjectDatas.RemoveAt(index);
                AttachedNode.objectInteractableInterfaces.RemoveAt(index);
                objectData.Refresh();


            };

            objectContainer.Add(delete);
            return objectContainer;
        }

        private VisualElement CreateDropdownFromDictionary(string elementLabel, Dictionary<string, int> data, int associatedVar, EventCallback<ChangeEvent<string>> onDropdownChanged)
        {
            List<string> keyArray = data.Keys.ToList();

            DropdownField dropdown = new DropdownField(elementLabel, keyArray, associatedVar);
            int valueToFind = associatedVar;

            var foundItem = data.FirstOrDefault(item => item.Value == valueToFind);
            if (foundItem.Key != null)
                dropdown.value = foundItem.Key;

            dropdown.RegisterValueChangedCallback(onDropdownChanged);

            return dropdown;
        }
    }
}