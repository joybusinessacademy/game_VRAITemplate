using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;
using Scripts.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews
{
	[NodeCustomEditor(typeof(LearningRecordNode))]
	public class LearningRecordNodeView : BaseNodeView
	{
		public class LearningRecord 
		{
			public string id;
			public string text;
			public string readableId = string.Empty;
			public string parentId;
			public int index;
		}

		public static Dictionary<string, LearningRecord> learningRecordMap = new Dictionary<string, LearningRecord>();
		public LearningRecordNode AttachedNode => nodeTarget as LearningRecordNode;
		//private Label infoLabel = new Label("Learning record is not configured. Advanced => Login to SkillsVR Account");
		private const string ECCloudClassName = "SkillsVR.EnterpriseCloudSDK.Editor.EnterpriseCloudSDKEditorWindow, com.skillsvr.EnterpriseCloudSDK.editor";

		public override VisualElement GetNodeVisualElement()
		{
			VisualElement dropdownContainer = new VisualElement();

			DropdownField dropdown = new() { label = "Record" };
			dropdownContainer.Add(dropdown);

			ReloadConfig(dropdown);
			
			return dropdownContainer;
		}

		public override VisualElement GetInspectorVisualElement()
		{
			var visualElement = new VisualElement();
			
			visualElement.Add(GetNodeVisualElement());
			visualElement.Add(new SkillsVR.VisualElements.Divider());
			
			Label infoLabel = new Label("Learning record is not configured. Advanced => Login to SkillsVR Account");
			infoLabel.style.whiteSpace = WhiteSpace.Normal;
			infoLabel.style.marginLeft = 5;
			infoLabel.style.marginRight = 5;
			visualElement.Add(infoLabel);	

			Button setupEnterpriseButton = new() { text = "Login to SkillsVR Account" };
			setupEnterpriseButton.clicked += () => {
				Type windowType = Type.GetType(ECCloudClassName);
				EditorWindow.GetWindow(windowType, true, null, true);
			};			
			visualElement.Add(setupEnterpriseButton);

			Button reloadConfigButton = new() { text = "Reload Config" };
			reloadConfigButton.clicked += RefreshNode;			
			visualElement.Add(reloadConfigButton);

			UpdateLabel(infoLabel);


			return visualElement;
		}
		


		private void ReloadConfig(DropdownField dropdown)
		{
			string path = Path.Combine(Application.dataPath, "Resources", "ECRecordConfig.asset");

			if (File.Exists(path))			
			{
				dropdown.choices.Clear();

				string[] lines = File.ReadAllLines(path);
				
				int lastIndex = 0;
				string lastId = string.Empty;
				string lastParentId = string.Empty;
				bool lastWasName = false;
				lines.ForEach(line => {
					if (line.Contains("- id:"))
					{
						var split = line.Split("id:");
						if (!learningRecordMap.ContainsKey(split[1]))
							learningRecordMap.Add(split[1], null);
						lastId = split[1];
					}

					if (line.Contains("index:"))
					{
						lastIndex = int.Parse(line.Split("index:")[1].Replace(" ", string.Empty));
					}

					if (line.Contains("parentId:"))
					{
						lastParentId = line.Split("parentId:")[1];
					}
					
					if (line.Contains("type: "))
					{
						lastWasName = false; 
					}
					
					if (lastWasName)
					{
						learningRecordMap[lastId].text += line;
						learningRecordMap[lastId].text = ClearWhiteSpacesFromText(learningRecordMap[lastId].text);
					}

					if (line.Contains("name:"))
					{
						var split = line.Split("name:");
						lastWasName = true;
						learningRecordMap[lastId] = new LearningRecord(){ id = lastId, text = split[1], parentId = lastParentId, index = lastIndex};

                        // build readble id
                        string fullId = BuildReadableId(learningRecordMap[lastId]);
                        learningRecordMap[lastId].readableId = fullId;
                        learningRecordMap[lastId].text = ClearWhiteSpacesFromText(learningRecordMap[lastId].text);
					}

				});

				lines.ForEach(line => {
					if (line.Contains("- id:"))
					{
						var split = line.Split("id:");
						lastId = split[1];
					}
					if (line.Contains("type: 1"))
					{
						learningRecordMap.Remove(lastId);
					}
				});

				learningRecordMap.ForEach(k => {
					dropdown.choices.Add(k.Value.readableId);
				});
				
				dropdown.value = string.IsNullOrEmpty(AttachedNode.learningRecordId) ? "not assigned" : AttachedNode.learningRecordId;
				dropdown.RegisterCallback<ChangeEvent<string>>(evt =>
				{
					AttachedNode.learningRecordId = evt.newValue;
					RefreshNode();
				});
			}
		}

		private void UpdateLabel( Label infoLabel)
		{
			string path = Path.Combine(Application.dataPath, "Resources", "ECRecordConfig.asset");

			if (File.Exists(path))
			{
				UpdateDisplayText(infoLabel);
			}
			else
			{
				infoLabel.text = "Configuration cant be found. Please Login to your SkillsVR Account.";
			}
		}

		private void UpdateDisplayText(Label infoLabel)
		{
			var record = learningRecordMap.ToList().Find(k => k.Value.readableId.Equals(AttachedNode.learningRecordId));
			AttachedNode.learningRecordPortalId = learningRecordMap.ToList().Find(k => k.Value.readableId == AttachedNode.learningRecordId).Key;

			if (!string.IsNullOrEmpty(AttachedNode.learningRecordId) && record.Value == null)
				infoLabel.text = "Configuration is wrong, the saved learning record id is not found.";
			else if (!string.IsNullOrEmpty(AttachedNode.learningRecordId))
				infoLabel.text = record.Value.text;
            else if (string.IsNullOrEmpty(AttachedNode.learningRecordId) && learningRecordMap.Count > 0)
                infoLabel.text = "Please select which learning record you want to connect.";

        }

		public static string ClearWhiteSpacesFromText (string text)
		{
			return System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Replace("\t",string.Empty);
		}

        
        public static string BuildReadableId(LearningRecord record)
        {
            // safe check
            if (record == null)
                return "";

            string id = (record.index + 1).ToString();

            if (!string.IsNullOrEmpty(record.parentId) && learningRecordMap.TryGetValue(record.parentId, out LearningRecord parent))
            {
	            string parentId = BuildReadableId(parent);
                return parentId + "." + id;
            }

            return id;
        }
    }
	
}