using System.Collections.Generic;
using GraphProcessor;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEngine.UIElements;
using DialogExporter;
using Scripts.VisualElements;
using UnityEngine;
using SkillsVRNodes.Scripts;
using SkillsVR.VisualElements;
using VisualElements;
using System.Linq;
using Props.PropInterfaces;
using SkillsVRNodes.Audio;

namespace SkillsVRNodes.Editor.NodeViews
{
	
	[NodeCustomEditor(typeof(DialogueNode))]
	public class DialogueNodeView : BaseNodeView
	{
		private DialogueNode AttachedNode => AttachedNode<DialogueNode>();
		private const string scriptablePath = "Assets/Contexts/Dialogues";

		private AudioRecorderVisualElement audioRecorderVisualElement;
		private Label audioClipLabel;

		
		private bool isMyVoiceInGenerating;
		public override VisualElement GetNodeVisualElement()
		{ 
			VisualElement visualElement = new VisualElement();

			visualElement.Add(new TextLabel("Character", AttachedNode.dialoguePosition.GetProp() != null ? AttachedNode.dialoguePosition.GetPropName() : null));

			if (AttachedNode.dialogueAsset)
			{
				visualElement.Add(new Label(AttachedNode.dialogueAsset.Dialog));
			}
			else
			{
				visualElement.Add(new Label("No Dialogue Selected") {name = "empty-text"});
			}

			return visualElement;
		}

		public override VisualElement GetInspectorVisualElement()
		{
			VisualElement visualElement = new VisualElement();

			ScriptableObjectDropdown<LocalizedDialog> dialogueDropdown = new("Dialogue: ", AttachedNode.dialogueAsset, so =>
			{
				AttachedNode.dialogueAsset = so;
				AttachedNode.audioClipBeingUsed = AttachedNode.dialogueAsset != null ? AttachedNode.dialogueAsset.GetAudioClip : null;
				
				ClipBeingUsedUpdate();
				RefreshNode();
			}, () => EditorGUIUtility.PingObject(AttachedNode.dialogueAsset));
			dialogueDropdown.nameAfterScene = true;
			
			visualElement.Add(dialogueDropdown);
			visualElement.Add(AudioClipVisual()); 
			var createDialogueButton = new Button()
			{
				text = "Create New Dialogue",
				name = "NewDialogueButton"
			};

			createDialogueButton.clicked += () =>
			{
				AttachedNode.dialogueAsset = dialogueDropdown.CreateScriptableObject();
				RefreshNode();
			};
			visualElement.Add(createDialogueButton);
			TextField subtitleText = new(-1, true, false, '*')
			{
				style =
				{
					minHeight = 75,
				},
			};

			subtitleText.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				if (AttachedNode.dialogueAsset != null)
				{
					Undo.RecordObject(AttachedNode.dialogueAsset, "Changed text");
					AttachedNode.dialogueAsset.Dialog = evt.newValue;
				}
			});
			visualElement.Add(subtitleText);
			visualElement.Add(new PropDropdown<IPropAudioSource>("Character: ", AttachedNode.dialoguePosition,
				elementName => AttachedNode.dialoguePosition = elementName, false));
			VisualElement recordAudioContainer = new();
			visualElement.Add(recordAudioContainer);
			
			
			bool hasDialogue = AttachedNode.dialogueAsset;
			
			createDialogueButton.style.visibility = !hasDialogue ? Visibility.Visible : Visibility.Hidden;
			
			subtitleText.SetEnabled(hasDialogue);
			subtitleText.value = hasDialogue ? AttachedNode.dialogueAsset.Dialog : "null";

			if (AttachedNode.dialogueAsset != null)
			{
				var audioRecorderVisualElement = new AudioRecorderVisualElement(AttachedNode.dialogueAsset);
				audioRecorderVisualElement.recordingFinished = null;
				audioRecorderVisualElement.recordingFinished += OnRecordingFinished;
				
				recordAudioContainer.Add(audioRecorderVisualElement);
			}
			
			DialogueNodeExportSettingsView settings = new();
			visualElement.Add(settings);

			var generateAudio = new Button
			{
				text = "Generate Audio"
			};
			generateAudio.clicked += () => { OnGenerateVoiceClicked(settings); };

            this.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanelEvent);
            this.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanelEvent);

			visualElement.Add(generateAudio);
			visualElement.Add(new Divider());

			//Audio
			var audioTitle = new VisualElement();

			visualElement.Add(audioTitle);

			visualElement.Add(SetAudioDropDown());

			var addCustomAudio = new Button
			{
				text = "Add Custom Audio",
			};
			addCustomAudio.clicked += OnAddCustomAudio;
			visualElement.Add(addCustomAudio);

			return visualElement;
		}

		protected void OnDetachFromPanelEvent(DetachFromPanelEvent evt)
		{
            if (isMyVoiceInGenerating)
            {
                VoiceExporter.finishedGenerated = null;
                VoiceExporter.Release();
            }
        }


        private void OnGenerateVoiceClicked(DialogueNodeExportSettingsView settings)
        {
            if (null != VoiceExporter.finishedGenerated)
            {
                Debug.LogError("Voice generation is running. Please wait for finishe and try again later.");
                return;
            }
			isMyVoiceInGenerating = true;
            AttachedNode.dialogueAsset.textToSpeechAPI = settings.ReturnActiveApi();
            VoiceExporter.Export(AttachedNode.dialogueAsset, AttachedNode.dialogueAsset.language);
            AttachedNode.dialogueAsset.TextIsDirty = false;

            VoiceExporter.finishedGenerated = OnFinishedGeneratedClip;

            RefreshNode();
        }

        private VisualElement AudioClipVisual()
		{
			var audioClipLabel = new Label();
			audioClipLabel.style.paddingTop = 5f;
			audioClipLabel.style.paddingLeft = 5f;
			
			if (AttachedNode.audioClipBeingUsed != null)
				audioClipLabel.text = "Clip Being Used: " + (AttachedNode.audioClipBeingUsed == null ? "None" : AttachedNode.audioClipBeingUsed.name);

			return audioClipLabel;
		}

        private void OnFinishedGeneratedClip(AudioClip generatedClip)
		{
			isMyVoiceInGenerating = false;
            VoiceExporter.finishedGenerated = null;
			ClipBeingUsedUpdate(generatedClip);
		}

        private void ClipBeingUsedUpdate(AudioClip audioClip = null)
		{
			if (null == AttachedNode)
			{
				return;
			}
			if (audioClip != null)
			{
				AttachedNode.audioClipBeingUsed = audioClip;
			}
			
			RefreshNode();
		}
		
		private void OnRecordingFinished(AudioClip audioClip)
		{
			ClipBeingUsedUpdate(audioClip);
			RefreshNode();
		}

		private const string audioDropdownDefault = "Auto";
		
		private VisualElement SetAudioDropDown()
		{
			var audioDropdown = new DropdownField();
			audioDropdown.choices.Add(audioDropdownDefault);

			if (AttachedNode.dialogueAsset && AttachedNode.dialogueAsset.IsUsingCustom)
			{
				audioDropdown.value = AttachedNode.dialogueAsset.GetAudioClip.name;
			}
			else
			{
				audioDropdown.value = audioDropdownDefault;
			}

			if (ScriptableObjectManager.GetAllInstances<AudioSO>().Count != 0)
			{
				foreach (AudioSO audio in ScriptableObjectManager.GetAllInstances<AudioSO>().Where(audio => audio != null && audio.clip != null))
				{
					audioDropdown.choices.Add(audio.clip.name);
				}
			}

			audioDropdown.RegisterCallback<ChangeEvent<string>>(evt =>
			{
				if (!AttachedNode.dialogueAsset)
				{
					return;
				}
				
				if (evt.newValue == audioDropdownDefault)
				{
					AttachedNode.dialogueAsset.UseGeneratedClip();
				}
				else if (!AttachedNode.dialogueAsset.GeneratedClipNull 
				    && evt.newValue == AttachedNode.dialogueAsset.GeneratedClipName)
				{
					AttachedNode.dialogueAsset.UseGeneratedClip();
				}
				else
				{
					AudioClip clip = ScriptableObjectManager.GetAllInstances<AudioSO>().Find(t => t?.clip?.name == evt?.newValue)?.clip;
					AttachedNode.dialogueAsset.SetCustomAudio(clip);
					ClipBeingUsedUpdate(clip);
				}

				RefreshNode();
			});

			audioDropdown.label = "Clip: ";
			
			return audioDropdown;
		}

		private void OnAddCustomAudio()
        {
			AudioClip clip = AskUserForWav.TryAddAudioFile("Error in Finding that File", 
				"Could not detect a .wav or .ogg filetype", out string filename);
			if (clip == null)
			{
				return;
			}

			if (!ScriptableObjectManager.CheckIfAssetNameExists<AudioSO>(filename))
            {
	            AudioSO asset = ScriptableObjectManager.CreateScriptableObject<AudioSO>("Assets/Contexts/Scriptables", filename);
	            asset.clip = clip;
				ScriptableObjectManager.ForceSerialization(asset);
			}
            else
            {
	            foreach (AudioSO asset in ScriptableObjectManager.GetAllInstances<AudioSO>())
	            {
		            if (asset.name.Equals(filename))
		            {
						asset.clip = clip;
						ScriptableObjectManager.ForceSerialization(asset);
					}
	            }				
            }

			AttachedNode.dialogueAsset.SetCustomAudio(clip);

			ClipBeingUsedUpdate(clip);
			RefreshNode();
		}
	}
}