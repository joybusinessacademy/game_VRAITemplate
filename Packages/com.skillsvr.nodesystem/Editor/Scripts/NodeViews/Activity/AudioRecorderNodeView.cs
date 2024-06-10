using GraphProcessor;
using Scripts.VisualElements;
using SkillsVR.Mechanic.MechanicSystems.AudioRecorder;
using SkillsVR.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using Props.PropInterfaces;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using SkillsVR.Mechanic.MechanicSystems.AudioRecorder.Impl;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts;
using Toggle = UnityEngine.UIElements.Toggle;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(AudioRecorderNode))]
	public class AudioRecorderNodeView : SpawnerNodeView<SpawnerAudioRecorder, IAudioRecorderSystem, SkillsVR.Mechanic.MechanicSystems.AudioRecorder.AudioRecorderData>
	{
		public AudioRecorderNode AttachedNode => nodeTarget as AudioRecorderNode;

		private VisualElement recorderTypesVE = new VisualElement();
		private VisualElement keywordSoundBiteVE = new VisualElement();
		private VisualElement controllerInputVE = new VisualElement();
		private DropdownField audioDropdown;

		public Dictionary<string, int> audioRecorderPresets = new Dictionary<string, int>()
		{
			{ "No Playback" ,0 },
			{ "With Playback",1 },
			{ "Playback and Information" , 2 },
		};

		public override VisualElement GetNodeVisualElement()
		{
			VisualElement visualElement = new();
			visualElement.Add(new TextLabel("Save audio as", AttachedNode.saveName));
			return visualElement;
		}

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new VisualElement();
			visualElement.Add(CreateTransformDropdown<IPropPanel>(AttachedNode<AudioRecorderNode>()));


			//Save Name for Recording
			visualElement.Add(ShowSaveName());

			//Input Type (Trigger vs Button)
			visualElement.Add(ShowPresetKeysDropdown());

			//Add Trigger Selection
			visualElement.Add(AddControllerSelectionDropdown());

			//Add Recorder Types
			//recorderTypesVE = ;
			visualElement.Add(ShowRecorderTypes());

			//Sound Bites
			visualElement.Add(ShowSoundBiteVisuals());

			//Show Visual Based on Data
			UpdatePresetVisuals();
			UpdateBasedOnInputType();

			//Audio Node Update
			UpdateAudioAssetDropdownFromNode();
			
			AudioRecorderNode spawnerNode = AttachedNode;

			//presetKeyField = new TextField("Preset Key Field: ");
			//presetKeyField.value = AttachedNode.nodePresetKey;
			//presetKeyField.RegisterValueChangedCallback(e =>
			//{
			//	AttachedNode.nodePresetKey = e.newValue;
			//});
			//visualElements.Add(presetKeyField);

			visualElement.Add(spawnerNode.MechanicData.CustomTextField(nameof(spawnerNode.MechanicData.startRecordingInformationText)));
			visualElement.Add(spawnerNode.MechanicData.CustomTextField(nameof(spawnerNode.MechanicData.stopRecordingInformationText)));
			
			bool fixedDuration = spawnerNode.enableMaxRecordDuration;
			Toggle enableMaxDurationToggle = spawnerNode.CustomToggle(nameof(spawnerNode.enableMaxRecordDuration));
			FloatField durationField = spawnerNode.MechanicData.CustomFloatField(nameof(spawnerNode.MechanicData.clipDuration));
			durationField.SetDisplay(fixedDuration);
			enableMaxDurationToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
			{
				bool fixedDuration = evt.newValue;
				durationField.SetDisplay(fixedDuration);
				durationField.MarkDirtyRepaint();
			});
			enableMaxDurationToggle.value = fixedDuration;

			visualElement.Add(enableMaxDurationToggle);
			visualElement.Add(durationField);

			//var autoStopToggle = spawnerNode.CustomToggle(nameof(spawnerNode.keepMechanicAliveAfterRecord));
			//visualElements.Add(autoStopToggle);
			
			return visualElement;
		}
		
		private void UpdatePresetVisuals()
		{
			switch (AttachedNode.mechanicData.audioRecorderType)
			{
				case AudioRecorderType.DEFAULT:
				case AudioRecorderType.RECORDWITHPLAYBACK:
					AttachedNode.keepMechanicAliveAfterRecord = true;
					keywordSoundBiteVE.style.display = DisplayStyle.None;
					break;
				case AudioRecorderType.RECORDWITHPLAYBACKANDINFO:
					AttachedNode.keepMechanicAliveAfterRecord = true;
					keywordSoundBiteVE.style.display = DisplayStyle.Flex;
					break;
				default:
					break;
			}
		}

		private void UpdateBasedOnInputType()
		{
			//Trigger
			if(AttachedNode.recorderInputStyle == 0)
			{
				controllerInputVE.style.display = DisplayStyle.Flex;
				recorderTypesVE.style.display = DisplayStyle.None;
				keywordSoundBiteVE.style.display = DisplayStyle.None;
				
				AttachedNode.keepMechanicAliveAfterRecord = false;
			}//Button
			else
			{
				recorderTypesVE.style.display = DisplayStyle.Flex;
				controllerInputVE.style.display = DisplayStyle.None;
				UpdatePresetVisuals();
			}
		}

		private VisualElement ShowSoundBiteVisuals()
		{
			keywordSoundBiteVE.Clear();

			keywordSoundBiteVE.Add(AttachedNode.mechanicData.keyword.LocField("Keyword"));
			keywordSoundBiteVE.Add(AttachedNode.mechanicData.informationForKeyword.LocField("Keyword Information"));
			BuildAudioDropdown();

			UpdatePresetVisuals();
			return keywordSoundBiteVE;
		}

		private VisualElement ShowRecorderTypes()
		{
			List<string> keyArray = audioRecorderPresets.Keys.ToList();

			DropdownField dropdown = new DropdownField("Select Recorder Preset: ", keyArray, AttachedNode.currentSelectedRecorderType);
			int valueToFind = AttachedNode.currentSelectedRecorderType;

			foreach (var item in audioRecorderPresets)
			{
				if (item.Value == valueToFind)
				{
					dropdown.value = item.Key;
					break;
				}
			}

			dropdown.RegisterValueChangedCallback((evt) =>
			{
				AttachedNode.currentSelectedRecorderType = audioRecorderPresets[evt.newValue];
				AttachedNode.mechanicData.audioRecorderType = (AudioRecorderType)AttachedNode.currentSelectedRecorderType;

				UpdatePresetVisuals();
			});

			return dropdown;
		}

		private VisualElement AddControllerSelectionDropdown()
		{
			List<string> keyArray = AttachedNode.inputIdsRemap.Keys.ToList();

			DropdownField dropdown = new DropdownField("Select Input Type: ", keyArray, 0);
			string valueToFind = AttachedNode.inputIds;

			foreach (KeyValuePair<string, string> item in AttachedNode.inputIdsRemap)
			{
				if (item.Value == valueToFind)
				{
					dropdown.value = item.Key;
					break;
				}
			}

			dropdown.RegisterValueChangedCallback((evt) =>
			{
				AttachedNode.inputIds = AttachedNode.inputIdsRemap[evt.newValue];
			});

			return dropdown;
		}

		private VisualElement ShowSaveName()
		{
			TextField saveField = new TextField("Set Save Name Of Clip: ")
			{
				value = AttachedNode.saveName
			};

			saveField.RegisterValueChangedCallback((evt) =>
			{
				AttachedNode.saveName = evt.newValue;
			});

			return saveField;
		}

		private DropdownField ShowPresetKeysDropdown()
		{
			List<string> keyArray = AttachedNode.nodePresetKeyRemap.Keys.ToList();

			DropdownField dropdown = new DropdownField("Select Recorder UI Type: ", keyArray, 0);
			int valueToFind = AttachedNode.recorderInputStyle;

			foreach (var item in AttachedNode.nodePresetKeyRemap)
			{
				if (item.Value == valueToFind)
				{
					dropdown.value = item.Key;
					break;
				}
			}

			dropdown.RegisterValueChangedCallback((evt) =>
			{
				AttachedNode.recorderInputStyle = AttachedNode.nodePresetKeyRemap[evt.newValue];
				AttachedNode.mechanicData.audioInputType = (AudioInputType)AttachedNode.recorderInputStyle;
				UpdateBasedOnInputType();
			});

			return dropdown;
		}


		#region Audio Dropdown
		private void BuildAudioDropdown()
		{
			audioDropdown = CreateAudioDropDown();
			RefreshAudioAssetDropdownChoices();
			keywordSoundBiteVE.Add(audioDropdown);
		}

		private DropdownField CreateAudioDropDown()
		{
			var dropdown = new DropdownField
			{
				label = "Add Audio File: ",
				tooltip = "Audio file to play"
			};
			dropdown.RegisterCallback<ChangeEvent<string>>(OnAudioAssetChanged);
			return dropdown;
		}

		private void RefreshAudioAssetDropdownChoices()
		{
			audioDropdown.choices.Clear();
			audioDropdown.choices.Insert(0, "Null");

			//find all custom imported
			if (ScriptableObjectManager.GetAllInstances<AudioSO>().Count != 0)
			{
				foreach (AudioSO audio in ScriptableObjectManager.GetAllInstances<AudioSO>())
				{
					if (audio.clip == null)
					{
						continue;
					}
					audioDropdown.choices.Add("Audio Clips/" + audio.clip.name);
				}
			}
			audioDropdown.choices.Add("External/Add Audio Clip");
		}

		private void OnAudioAssetChanged(ChangeEvent<string> evt)
		{
			AudioSO asset = null;
			if (evt.newValue.Equals("External/Add Audio Clip"))
			{
				asset = OnAddCustomAudio(evt.previousValue);
			}
			else
			{
				string[] newValSplit = evt.newValue.Split('/');
				asset = ScriptableObjectManager.GetAllInstances<AudioSO>().Find(t => t.clip.name.Equals(newValSplit[newValSplit.Length - 1]));
			}

			if (asset != null)
			{
				AttachedNode<AudioRecorderNode>().AssociatedCustomClip = asset.clip.name;
				AttachedNode<AudioRecorderNode>().mechanicData.keywordSoundBite = asset.clip;
			}

			if(evt.newValue.Equals("Null"))
			{
				AttachedNode<AudioRecorderNode>().AssociatedCustomClip = "";
				AttachedNode<AudioRecorderNode>().mechanicData.keywordSoundBite = null;
			}
		}
		
		private void UpdateAudioAssetDropdownFromNode()
		{
			if (!string.IsNullOrEmpty(AttachedNode<AudioRecorderNode>().AssociatedCustomClip))
			{
				int index = audioDropdown.choices.FindIndex(x => x.Contains(AttachedNode<AudioRecorderNode>().AssociatedCustomClip));

				audioDropdown.SetValueWithoutNotify(index != -1 ? audioDropdown.choices[index] : "Null");
			}
			else
			{
				audioDropdown.SetValueWithoutNotify("Null");
			}
		}


		private AudioSO OnAddCustomAudio(string previousValue)
		{
			AudioClip clip = AskUserForWav.TryAddAudioFile("Error in Finding that File",
				 "Could not detect a .wav or .ogg filetype", out string filename);

			if (clip == null)
			{
				return null;
			}

			AudioSO returnAsset = null;

			if (!ScriptableObjectManager.CheckIfAssetNameExists<AudioSO>(filename))
			{
				returnAsset = ScriptableObjectManager.CreateScriptableObject<AudioSO>("Assets/Contexts/Scriptables", filename);
				returnAsset.clip = clip;
				ScriptableObjectManager.ForceSerialization(returnAsset);
			}
			else
			{
				foreach (AudioSO asset in ScriptableObjectManager.GetAllInstances<AudioSO>())
				{
					if (asset.name.Equals(filename))
					{
						asset.clip = clip;
						ScriptableObjectManager.ForceSerialization(asset);
						returnAsset = asset;
						break;
					}
				}
			}

			if (returnAsset != null)
			{
				audioDropdown.value = "Audio Clips/" + returnAsset.clip.name;
			}
			else
			{
				audioDropdown.value = previousValue;
			}

			return returnAsset;
		}
		#endregion Audio Dropdown

	}
}
