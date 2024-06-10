using GraphProcessor;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using SkillsVRNodes.Editor.NodeViews;
using Scripts.VisualElements;
using VisualElements;
using Props;
using Props.PropInterfaces;
using UnityEditor.UIElements;


[NodeCustomEditor(typeof(DragDropNode))]
public class DragDropNodeView : BaseNodeView
{
    private DragDropNode AttachedNode => AttachedNode<DragDropNode>();
    private const string SCRIPTABLE_PATH = "Assets/Contexts/DragDrop";

    public readonly Dictionary<string, int> CompletionPresets = new()
    {
        { "Timer Ends", 0 },
        { "All Objects Selected Once", 1 },
        { "Sockets Filled", 2 },
        { "Sockets Filled Correctly", 3 },
    };

    public readonly Dictionary<string, int> CompletionFeedbackTypes = new()
    {
        { "On Nothing", 0 },
        { "On Socket", 1 },
        { "On Socket and Answer", 2 },
    };

    public readonly Dictionary<string, int> CompletionFeedbackShowOn = new()
    {
        { "On Socket Fill", 0 },
        { "On Node Complete", 1 },
    };

    public readonly Dictionary<string, int> DropBehaviors = new()
    {
        { "Return to Pickup Location", 0 },
        { "Hover", 1 },
        { "Physics Applied", 2 },
    };

    public readonly Dictionary<string, int> RotationBehaviors = new()
    {
        { "Auto", 0 },
        { "User Input", 1 },
    };

    public readonly Dictionary<string, int> RotationAxis = new()
    {
        { "World Up", 0 },
        { "Controller Up", 1 },
        { "Local Up", 2 }
    };
    
    private bool detailsDrawn;

    public override VisualElement GetNodeVisualElement()
    {
        return null;
    }

    public override VisualElement GetInspectorVisualElement()
    {
        detailsDrawn = false;

		var visualElement = new VisualElement();
        visualElement.Add(CreateCompletion());
        visualElement.Add(new Divider());
        visualElement.Add(CreateObjects());
        visualElement.Add(new Divider());
        visualElement.Add(CreateSockets());
        return visualElement;
    }

    private VisualElement CreateCompletion()
    {
        VisualElement completionContainer = new();


        UpdateCompletionSection(completionContainer);
        return completionContainer;
    }

    private VisualElement CreateDropdownFromDictionary(string elementLabel, Dictionary<string, int> data, int associatedVar, EventCallback<ChangeEvent<string>> onDropdownChanged)
    {
        List<string> keyArray = data.Keys.ToList();

        DropdownField dropdown = new DropdownField(elementLabel, keyArray, associatedVar);
        int valueToFind = associatedVar;

        KeyValuePair<string, int> foundItem = data.FirstOrDefault(item => item.Value == valueToFind);
        if (foundItem.Key != null)
        {
            dropdown.value = foundItem.Key;
        }

        dropdown.RegisterValueChangedCallback(onDropdownChanged);

        return dropdown;
    }

    private void UpdateCompletionSection(VisualElement completionContainer)
    {
        VisualElement container = new();

        VisualElement completionDetails = new();

        completionContainer.Clear();
        completionContainer.Add(new Label("Completion"));
        completionContainer.Add(CreateDropdownFromDictionary("Select Completion Preset: ", CompletionPresets, AttachedNode.currentSelectedCompletionType,
            (evt) =>
            {
                AttachedNode.currentSelectedCompletionType = CompletionPresets[evt.newValue];
                detailsDrawn = false;
                UpdateCompletionSection(completionContainer);
            }));
        
        
        if (AttachedNode.currentSelectedCompletionType == 0)
        {
            container.Add(AttachedNode.CustomFloatField(nameof(AttachedNode.timer)));
        }

        container.Add(CreateDropdownFromDictionary("Show Feedback Where: ", CompletionFeedbackTypes, AttachedNode.currentSelectedFeedbackType,
            (evt) =>
            {
                AttachedNode.currentSelectedFeedbackType = CompletionFeedbackTypes[evt.newValue];
                container.Add(AddCompletionDetails(completionDetails));
            }));

        container.Add(AddCompletionDetails(completionDetails));
        completionContainer.Add(container);
    }

    private VisualElement AddCompletionDetails(VisualElement ve)
    {
        if(AttachedNode.currentSelectedFeedbackType == 0 && detailsDrawn)
        {
            ve.Clear();
            detailsDrawn = false;
        }

        if (AttachedNode.currentSelectedFeedbackType != 0 && !detailsDrawn)
        {
            ve.Add(CreateDropdownFromDictionary("Show Feedback When: ", CompletionFeedbackShowOn, AttachedNode.currentSelectedShowOnType,
                (evt) =>
                {
                    AttachedNode.currentSelectedShowOnType = CompletionFeedbackShowOn[evt.newValue];
                }));

            ve.Add(AttachedNode.CustomFloatField(nameof(AttachedNode.showFor)));
            detailsDrawn = true; 
        }
        return ve;
    }

    private VisualElement CreateObjects()
    {
        VisualElement objectContainer = new();
        objectContainer.Add(new Label("Objects"));
        FloatField pickupSpeed = AttachedNode.CustomFloatField(nameof(AttachedNode.pickupSpeed));
        pickupSpeed.tooltip = "Speed at which the object moves to the hand on pickup. 0 for instant.";
        objectContainer.Add(pickupSpeed);
        objectContainer.Add(AttachedNode.CustomVectorField(nameof(AttachedNode.holdPositionOffset)));
        objectContainer.Add(CreateDropdownFromDictionary("Drop Behavior: ", DropBehaviors, AttachedNode.currentSelectedDropType, (evt) =>
        {
            AttachedNode.currentSelectedDropType = DropBehaviors[evt.newValue];
        }));


        objectContainer.Add(CreateDropdownFromDictionary("Rotation Type: ", RotationBehaviors, AttachedNode.currentSelectedRotationType, (evt) =>
        {
            AttachedNode.currentSelectedRotationType = RotationBehaviors[evt.newValue];
        }));

        objectContainer.Add(CreateDropdownFromDictionary("Rotation Axis: ", RotationAxis, AttachedNode.currentSelectedRotationAxis, (evt) =>
        {
            AttachedNode.currentSelectedRotationAxis = RotationAxis[evt.newValue];
        }));

        objectContainer.Add(AttachedNode.CustomFloatField(nameof(AttachedNode.rotationSpeed)));

        objectContainer.Add(AttachedNode.CustomFloatField(nameof(AttachedNode.scale)));

        objectContainer.Add(AttachedNode.CustomToggle(nameof(AttachedNode.dropOnPointer), "Objects Drop Where Pointing"));
        var objectData = new ListDropdown<InteractableObjectData>("Interactable Props", AttachedNode.interactableObjectDatas, ObjectBox);
        objectData.onAddButtonClicked += RefreshNode;

        objectContainer.Add(objectData);

        return objectContainer;
    }

    private VisualElement ObjectBox(InteractableObjectData interactableObjectData)
    {
        VisualElement objectContainer = new();

        int index = AttachedNode.interactableObjectDatas.IndexOf(interactableObjectData);
        while (AttachedNode.objectInteractableInterfaces.Count <= index)
        {
            AttachedNode.objectInteractableInterfaces.Add(new PropGUID<IPropGrabInteractable>());
        }
        
        PropGUID<IPropGrabInteractable> currentValue = AttachedNode.objectInteractableInterfaces[index];
        PropDropdown<IPropGrabInteractable> sceneElementDropdown = new("Interactables: ", currentValue, newValue =>
        {
            AttachedNode.objectInteractableInterfaces[index] = newValue;
        });

        objectContainer.Add(sceneElementDropdown);

        objectContainer.Add(new Divider());

        ScriptableObjectDropdown<TagSO> alteredVariableDropdown = new("Filter Layer: ", interactableObjectData.tag,
            evt => interactableObjectData.tag = evt,
            () => EditorGUIUtility.PingObject(interactableObjectData.tag));

        objectContainer.Add(alteredVariableDropdown);

        IconButton delete = new("Close")
        {
            tooltip = "Delete",
        };

        delete.clicked += () =>
        {
            AttachedNode.interactableObjectDatas.RemoveAt(index);
            AttachedNode.objectInteractableInterfaces.RemoveAt(index);
            RefreshNode();
        };

        objectContainer.Add(delete);
        return objectContainer;
    }

    private VisualElement CreateSockets()
    {
        VisualElement socketsContainer = new();
        socketsContainer.Add(new Label("Sockets"));

        socketsContainer.Add(AttachedNode.CustomToggle(nameof(AttachedNode.correctOnlyInOrder), "Correct Only in Order"));

        ListDropdown<InteractorSocketData> socketData = new("Socket Props", AttachedNode.interactorSocketDatas, SocketBox);
        socketData.onAddButtonClicked += RefreshNode;
        socketsContainer.Add(socketData);

        return socketsContainer;
    }

    private VisualElement SocketBox(InteractorSocketData interactableSocketData)
    {
        VisualElement sockets = new()
        {
            style =
            {
                flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Column),
            }
        };

        VisualElement socketContainer = new();

        int index = AttachedNode.interactorSocketDatas.IndexOf(interactableSocketData);
        while (AttachedNode.socketInteractorInterfaces.Count <= index)
        {
            AttachedNode.socketInteractorInterfaces.Add(new PropGUID<IPropSocketInteractor>());
        }

        PropGUID<IPropSocketInteractor> currentValue = AttachedNode.socketInteractorInterfaces[index];
        PropDropdown<IPropSocketInteractor> sceneElementDropdown = new("Interactors: ", currentValue, newValue =>
        {
            AttachedNode.socketInteractorInterfaces[index] = newValue;
        });

        sceneElementDropdown.style.flexGrow = new StyleFloat(1);
        socketContainer.Add(sceneElementDropdown);

        socketContainer.Add(new Divider());

        VisualElement filterBox = new();

        filterBox.Add(FilterBox("Accepted: ", interactableSocketData.tagsFiltered));

        IconButton delete = new IconButton("Close")
        {
            tooltip = "Delete",
            style =
            {
                height = 25f,
                width = 25f
            }
        };

        delete.clicked += () =>
        {
            AttachedNode.interactorSocketDatas.RemoveAt(index);
            AttachedNode.socketInteractorInterfaces.RemoveAt(index);
            RefreshNode();
        };

        socketContainer.Add(delete);

        sockets.Add(socketContainer);
        sockets.Add(new Divider());
        sockets.Add(filterBox);

        return sockets;
    }

    private VisualElement FilterBox(string name, List<InteractorSocketDataInstance> filterList)
    {
        VisualElement wholeFilterBox = new()
        {
            style =
                {
                    flexGrow = new StyleFloat(1)
                }
        };

        ListDropdown<InteractorSocketDataInstance> socketFilterList = new(name, filterList, Action);
        socketFilterList.onAddButtonClicked += RefreshNode;
        wholeFilterBox.Add(socketFilterList);
        return wholeFilterBox;

        VisualElement Action(InteractorSocketDataInstance nextLayer)
        {
            VisualElement filterContainer = new();

            int index = filterList.IndexOf(nextLayer);
            ScriptableObjectDropdown<TagSO> allowedTagDropdown = new("Filter Layer: ", filterList[index].filterTag, evt =>
            {
                filterList[index].filterTag = evt;
            }, () => EditorGUIUtility.PingObject(filterList[index].filterTag));
            filterContainer.Add(allowedTagDropdown);

            Toggle isCorrect = new("Is Correct: ")
            {
                value = filterList[index].isCorrect
            };
            isCorrect.RegisterCallback<ChangeEvent<bool>>(evt => { filterList[index].isCorrect = evt.newValue; });

            //isCorrect.
            filterContainer.Add(isCorrect);

            IconButton delete = new("Close") { tooltip = "Delete", };

            delete.clicked += () =>
            {
                filterList.RemoveAt(index);
                RefreshNode();
            };
            filterContainer.Add(delete);


            return filterContainer;
        }
    }
}
