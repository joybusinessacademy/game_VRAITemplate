using GraphProcessor;
using SkillsVR.Mechanic.MechanicSystems.AudioPlayback;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using Props.PropInterfaces;
using SkillsVR.VisualElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElements;
using Scripts.VisualElements;

namespace SkillsVRNodes.Editor.NodeViews
{
    [NodeCustomEditor(typeof(AudioPlaybackNode))]
    public class AudioPlaybackNodeView : SpawnerNodeView<SpawnerAudioPlayback, IAudioPlaybackSystem, SkillsVR.Mechanic.MechanicSystems.AudioPlayback.AudioPlaybackData>
    {
        public override VisualElement GetNodeVisualElement()
        {
            string audioFileSet = "";

            string assosciatedName = AttachedNode<AudioPlaybackNode>().AssociatedRecorderNodesaveName;

            if (!assosciatedName.IsNullOrWhitespace())
                audioFileSet = assosciatedName;
            else
                audioFileSet = AttachedNode.MechanicData.audioClip ? AttachedNode.MechanicData.audioClip.name : null;


			return new TextLabel("Audio file", audioFileSet);
        }
        
        public override VisualElement GetInspectorVisualElement()
        {
            var visualElement = new VisualElement();
            visualElement.Add(new PropDropdown<IPropAudioSource>("Playback Position: ",
                AttachedNode<AudioPlaybackNode>().AssociatedPlaybackAudioProp,
                evt => AttachedNode<AudioPlaybackNode>().AssociatedPlaybackAudioProp = evt));

            visualElement.Add(new Divider());

            visualElement.Add(CreateAudioDropdown());

            return visualElement;
        }
        
        private const string AUDIO_RECORDER_STRING = "Audio Recorder/";
        
        private VisualElement CreateAudioDropdown()
        {
            AudioSelector audioSelector = null;
            audioSelector = new AudioSelector(evt => DropdownCallback(evt, audioSelector), AttachedNode.MechanicData.audioClip);

            if (!AttachedNode<AudioPlaybackNode>().AssociatedRecorderNodesaveName.IsNullOrWhitespace())
            {
                audioSelector.audioDropdown.dropdown.value = AUDIO_RECORDER_STRING + AttachedNode<AudioPlaybackNode>().AssociatedRecorderNodesaveName;
            }
            //find all audio recorder nodes

            var allSceneGraphs = ScriptableObjectManager.GetAllInstances<SceneGraph>();
            
            List<BaseNode> allRecorders = new();
            foreach (SceneGraph graph in allSceneGraphs)
            {                
                allRecorders.AddRange( graph.nodes.FindAll(t => t.GetType() == typeof(AudioRecorderNode)));
            }

            if (GetRecorderNodes().Count > 0)
            {
                audioSelector.audioDropdown.dropdown.choices.Insert(1, "");
                foreach (BaseNode recorder in GetRecorderNodes())
                {
                    audioSelector.audioDropdown.dropdown.choices.Insert(2,AUDIO_RECORDER_STRING + ((AudioRecorderNode)recorder).saveName);
                }
            }
            
            return audioSelector;
        }

        private void DropdownCallback(AudioClip evt, AudioSelector audioSelector)
        {
            string newValue = audioSelector.audioDropdown.dropdown.value;
            if (newValue.Contains(AUDIO_RECORDER_STRING))
            {
                string[] newValSplit = newValue.Split('/');
                
                List<BaseNode> allRecorders = GetRecorderNodes();

                AudioRecorderNode arn = (AudioRecorderNode)allRecorders.Find(t => ((AudioRecorderNode)t).saveName.Equals(newValSplit[^1]));
                AttachedNode<AudioPlaybackNode>().AssociatedRecorderNodesaveName = arn.saveName;
        
                AttachedNode<AudioPlaybackNode>().AssociatedCustomClip = null;
            }
            else
            {
                AttachedNode<AudioPlaybackNode>().AssociatedRecorderNodesaveName = null;
                AttachedNode<AudioPlaybackNode>().AssociatedCustomClip = newValue;

            }
            
            AttachedNode.MechanicData.audioClip = evt;
        }

        private static List<BaseNode> GetRecorderNodes()
        {
            List<SceneGraph> allSceneGraphs = ScriptableObjectManager.GetAllInstances<SceneGraph>();

            List<BaseNode> allRecorders = new();
            foreach (SceneGraph graph in allSceneGraphs)
            {
                allRecorders.AddRange(graph.nodes.FindAll(t => t.GetType() == typeof(AudioRecorderNode)));
            }

            return allRecorders;
        }
    }
}