using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrazyMinnow.SALSA;
using CrazyMinnow.SALSA.OneClicks;
using Props;
using SkillsVRNodes.Editor.NodeViews;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UIElements.Image;


[CustomEditor(typeof(PropComponent))]
public class PropComponentEditor : Editor
{
	private VisualElement thing;
	private PropComponent propComponent;
	public override VisualElement CreateInspectorGUI()
	{
		propComponent = (PropComponent) target;
		// Create a new VisualElement to be the root of our inspector UI
		VisualElement container = new VisualElement();

		// Add a simple label
		container.Add(new Label("Prop Details"));

		TextField nameField = new()
		{
			label = "Name: ",
			value = propComponent.PropName,
            multiline = true
        };
        nameField.RegisterCallback<ChangeEvent<string>>(evt =>
        {
			nameField.value = evt.newValue;
        });
		container.Add(nameField);

        container.Add(propComponent.CustomTextField(nameof(propComponent.propDescription)));

		EnumField enumField = new EnumField("Drop Position", propComponent.dropPosition);
		
		enumField.RegisterCallback<ChangeEvent<Enum>>(evt =>
		{
			propComponent.dropPosition = (PropComponent.DropPosition)evt.newValue;
			EditorUtility.SetDirty(target);
		});
        
		DropdownField dropdownField = new DropdownField
		{
			label = "Prop Type",
			value = propComponent.propType == null ? "null" : propComponent.propType.GetType().FullName
		};

		foreach (Type propType in GetAllTypes())
		{
			dropdownField.choices.Add(propType.FullName);
		}
		
		dropdownField.RegisterCallback<ChangeEvent<string>>( evt =>
		{
			Undo.RecordObject(propComponent, "Changed Prop Type");
			Type type = GetAllTypes().First(x => x.FullName == evt.newValue);
			propComponent.propType = (PropType) Activator.CreateInstance(type, new object[] {propComponent});
			RefreshProdData();
			AutoConfig();
		});
		
		container.Add(dropdownField);
		Button setupButton = new(AutoConfig)
		{
			tooltip = "Auto Config Prop",
			style =
			{
				marginBottom = 0,
				marginTop = 0,
				marginRight = 0,
				paddingBottom = 0,
				paddingRight = 0,
				paddingLeft = 0,
				paddingTop = 0
			}
		};

		setupButton.Add(new Image() { image = Resources.Load<Texture2D>("Icon/Refresh"), style = { width = 17, height = 17}});
		dropdownField.Add(setupButton);
		container.Add(enumField);
		thing = new VisualElement();
		container.Add(thing);
		
		RefreshProdData();

		// Return the finished inspector UI
		return container;
	}

	private void AutoConfig()
	{
		Undo.RecordObject(propComponent, "Auto Config Prop");
		propComponent.propType.AutoConfigProp();
	}

	public void RefreshProdData()
	{
		PropComponent propComponent = (PropComponent) target;
		thing.Clear();

		if (propComponent.propType.GetType().Name.Contains("Character"))
		{
			Button button = new(SetupCharacter) {text = "Setup Character"};

			thing.Add(button);
		}
		thing.Add(propComponent.CustomField(nameof(propComponent.propType)));
	}


	private const string lookAtPointName = "Look at Point";
	private void SetupCharacter()
	{
		GameObject go = (target as PropComponent)?.gameObject;
		if (go == null)
		{
			return;
		}
		
		OneClickCCEditor.SalsaOneClickSetup = OneClickCC4.Setup;
		OneClickCCEditor.EyesOneClickSetup = OneClickCC3Eyes.Setup;

		OneClickCCEditor.SalsaOneClickSetup(go, AssetDatabase.LoadAssetAtPath<AudioClip>(OneClickBase.RESOURCE_CLIP));
		OneClickCCEditor.EyesOneClickSetup(go);

		Eyes eyes = go.GetComponentInChildren<Eyes>();
		Emoter emoter = go.GetComponentInChildren<Emoter>();
		if (eyes == null || emoter == null)
		{
			return;
		}
		
		SkinnedMeshRenderer smr = emoter.emotes.First().expData.controllerVars.First().smr;
		QueueProcessor queueProcessor = emoter.queueProcessor;
		
		Preset preset = Resources.Load<Preset>("Presets/Emoter");
		preset.ApplyTo(emoter);
		
		emoter.queueProcessor = queueProcessor;
		foreach (InspectorControllerHelperData data in emoter.emotes.SelectMany(emoteExpression => emoteExpression.expData.controllerVars))
		{
			data.smr = smr;
		}

		GameObject lookAtPoint = go.transform.Find(lookAtPointName)?.gameObject;
		if (lookAtPoint == null)
		{
			lookAtPoint = new GameObject(lookAtPointName);
		}
		
		eyes.lookTarget = lookAtPoint.transform;
		eyes.headEnabled = true;
		eyes.eyeEnabled = false;
		lookAtPoint.transform.parent = go.transform;
		lookAtPoint.transform.localRotation = Quaternion.identity;
		lookAtPoint.transform.position = RecursiveFindChild(go.transform, "head").position;
		lookAtPoint.transform.localPosition += Vector3.forward;
	}

	public IEnumerable<Type> GetAllTypes()
	{
		// Get all the types in the assembly
		Assembly[] t = AppDomain.CurrentDomain.GetAssemblies();
		IEnumerable<Type> q = t.SelectMany(s =>
		{
			if (s == null)
			{
				return new Type[] {};
			}

			// if (s.ExportedTypes.IsNullOrEmpty())
			// {
			// 	return new Type[] {};
			// }

			return s.GetTypes();
		});
		IEnumerable<Type> r = q.Where(p => typeof(PropType).IsAssignableFrom(p) && !p.IsAbstract && p != typeof(PropType));

		return r;
	}
	
	private Transform RecursiveFindChild(Transform parent, string childName)
	{
		foreach (Transform child in parent)
		{
			if (child.name.Contains(childName))
			{
				return child;
			}

			Transform found = RecursiveFindChild(child, childName);
			if (found != null)
			{
				return found;
			}
		}
		return null;
	}
}
