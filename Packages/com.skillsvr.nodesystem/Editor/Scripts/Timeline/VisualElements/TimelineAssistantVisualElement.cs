using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CCKManagers;
using CrazyMinnow.SALSA;
using CrazyMinnow.SALSA.Timeline;
using DG.DemiEditor;
using Props;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Props;
using SkillsVRNodes.Scripts.Hierarchy;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEditor.UIElements;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Samples.Editor.General
{
    public class TimelineAssistantVisualElement : VisualElement
    {
        List<PointerManipulator> allManipulators = new();
        private Button showSelectedTimelineObject;
        private ScrollView scrollView;

        private static PropReferences activlyEditingCharacter;
        private Button characterButton;
        private List<Button> tabButtons = new List<Button>();
        private Dictionary<string, VisualElement> activeDragables = new Dictionary<string, VisualElement>();
        private TrackAutoBinder autoBinder;
        private string searchKey = "";
        public TimelineAssistantVisualElement()
        {
            
            styleSheets.Add(Resources.Load<StyleSheet>("DragAndDrop"));

            //showSelectedTimelineObject = new Button
            //{
            //    text = "Show: Elements"
            //};
            //Selection.selectionChanged += OnSelectionChanged;
            //showSelectedTimelineObject.clicked += ShowSelectedTimelineObject;
            //Add(showSelectedTimelineObject);

            var tabsContainer = new VisualElement
            {
                name = "tabs"
            };


            characterButton = new(() =>
            {
                scrollView.Clear();
                CharacterSelection();
            })
            { text = "Characters: None Currently Selected" };
            Add(characterButton);

            Button allButton = (Button)CreateTab("All", ShowAll);
            tabButtons.Add(allButton);
            tabsContainer.Add(allButton);

            Button animStateButton = (Button)CreateTab("Animation States", AddAnimationStateAssets);
            tabButtons.Add(animStateButton);
            tabsContainer.Add(animStateButton);

            Button lookatButton = (Button)CreateTab("Look At Points", AddLookAtPoints);
            tabButtons.Add(lookatButton);
            tabsContainer.Add(lookatButton);

            Button emoteButton = (Button)CreateTab("Emoter", AddEmotions);
            tabButtons.Add(emoteButton);
            tabsContainer.Add(emoteButton);

            Button animButton = (Button)CreateTab("Animations", AddAnimations);
            tabButtons.Add(animButton);
            tabsContainer.Add(animButton);

            Button audioButton = (Button)CreateTab("Audio", AddAudio);
            tabButtons.Add(audioButton);
            tabsContainer.Add(audioButton);

            Add(tabsContainer);
            AddSearchBar();
            activlyEditingCharacter = null;
            SetTabButtonState();

            scrollView = new();
            Add(scrollView);
            scrollView.Clear();
            
            CharacterSelection();
            // OnSelectionChanged();
            Add(CreateExitButton());
        }

        private VisualElement CreateExitButton()
        {
            Button exit = new Button();
            exit.name = "exit-button";
            exit.text = "Save and Exit";
            
            exit.clicked += () => {
                PropsHierarchyWindow.highlightedName = null;
                TimelineAssistant.currentAsset.Save();
                LayoutManager.ResetLayoutA();
            };

            return exit;
        }

        private void CharacterSelection()
        {
            foreach (var prop in PropManager.GetAllProps<CharacterProp>())
            {

                scrollView.Add(CreateNewClickableAsset(prop.PropComponent.gameObject.name, "Character", () =>
                {

                    activlyEditingCharacter = prop;
                    characterButton.text = "Characters: " + prop.PropComponent.gameObject.name + " Currently Selected";

                    activeDragables.Clear();
                    SetTabButtonState();

                    scrollView.Clear();
                    ShowAll();

                    autoBinder = new TrackAutoBinder(activlyEditingCharacter.PropComponent.gameObject);

                }));

            }
        }

        private void AddAudio()
        {



            foreach (var audio in AssetDatabaseFileCache.GetAllObjects<AudioClip>())
            {
                activeDragables.Add(audio.name, CreateNewDraggableAsset(audio.name, "Audio Clip", () =>
                {
                    SalsaControl audioPlayable = ScriptableObject.CreateInstance<SalsaControl>();
                    audioPlayable.name = audio.name;
                    return audioPlayable;
                }, (asset) =>
                {
                    autoBinder.StartLateBind<Salsa>(asset);
                }));           
            }
            ApplySearch(searchKey);
        }

        private void AddAnimations()
        {
          

            foreach (var anim in AssetDatabaseFileCache.GetAllObjects<AnimationClip>())
            {
                string[] labels = AssetDatabase.GetLabels(anim);
                if (labels.Contains<string>("CharacterAnimation"))
                {

                    activeDragables.Add(anim.name, CreateNewDraggableAsset(anim.name, "Animation Clip", () =>
                    {
                        AnimationPlayableAsset asset = ScriptableObject.CreateInstance<AnimationPlayableAsset>();
                        asset.name = anim.name;
                        asset.clip = anim;
                        asset.removeStartOffset = false;
                        return asset;
                    }, (asset) =>
                    {
                        autoBinder.StartLateBind<Animator>(asset);                  
                    }));
                }
            }
            ApplySearch(searchKey);

        }

        private void AddAnimationStateAssets()
        {
            IEnumerable<RuntimeAnimatorController> allAssets = GetAnimationControllers();
            foreach (RuntimeAnimatorController asset in allAssets)
            {
                activeDragables.Add(asset.name, CreateNewDraggableAsset(asset.name, "Animation State", () =>
                {
                    AnimationStateAsset animationStateControl = ScriptableObject.CreateInstance<AnimationStateAsset>();
                    animationStateControl.template = new AnimationStatePlayable() { animatorController = asset };
                    animationStateControl.name = asset.name;
                    return animationStateControl;
                }));
            }
            ApplySearch(searchKey);
        }

        private void AddLookAtPoints()
        {
            //Empty Scene (First Scene Loaded or Destroyed Scene)
            if (PropManager.Instance == null)
                return;


            foreach (PropReferences propTransform in PropManager.GetAllProps<IPropLookAt>())
            {
                activeDragables.Add(propTransform.GUID.GetPropName(), CreateNewDraggableAsset(propTransform.GUID.GetPropName(), "Look At Point", () =>
                {
                    LookAtAsset asset = ScriptableObject.CreateInstance<LookAtAsset>();
                    asset.name = propTransform.GUID.GetPropName();
                    asset.template = new LookAtPlayable() { lookAtPoint = new PropGUID<IPropLookAt>(propTransform.GUID) };
                    return asset;
                }, (asset) =>
                {
                    autoBinder.StartLateBind<Eyes>(asset);
                }));
            }
            ApplySearch(searchKey);
        }

        private void AddEmotions()
        {
            // Gets all Emtions
            Emoter[] allEmoters = Object.FindObjectsOfType<Emoter>();
            List<string> emotions = new();
            foreach (Emoter emoter in allEmoters)
            {
                foreach (EmoteExpression test in emoter.emotes)
                {
                    emotions.AddIfNotFound(test.expData.name);
                }
            }

            foreach (string emotion in emotions)
            {
                activeDragables.Add(emotion, CreateNewDraggableAsset(emotion, "Emotion", () =>
                {
                    EmoterControl emotionControl = ScriptableObject.CreateInstance<EmoterControl>();
                    emotionControl.template = new EmoterControlBehavior() { emoteName = emotion };
                    emotionControl.name = emotion;
                    return emotionControl;
                }, (asset) =>
                {
                    autoBinder.StartLateBind<Emoter>(asset);
                }));
            }
            ApplySearch(searchKey);
        }

        private VisualElement CreateTab(string label, Action action)
        {
            Button button = new(() =>
            {
                activeDragables.Clear();
                scrollView.Clear();
                action.Invoke();
            })
            { text = label };

            button.Add(new Label(label));
            button.SetEnabled(activlyEditingCharacter != null);
            return button;
        }

        private void ApplySearch(string newValue)
        {
            List<VisualElement> itemsToAdd = new List<VisualElement>();

            foreach (var item in activeDragables)
            {
                if (item.Key.IndexOf(newValue, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    itemsToAdd.Add(item.Value);
                }
            }

            foreach (var item in activeDragables)
            {
                if (!itemsToAdd.Contains(item.Value))
                {
                    if (scrollView.Contains(item.Value))
                        scrollView.Remove(item.Value);
                }
            }

            foreach (var item in itemsToAdd)
            {
                scrollView.Add(item);
            }

            searchKey = newValue;
        }

        private void ShowAll()
        {
            AddAnimationStateAssets();
            AddLookAtPoints();
            AddEmotions();
            AddAnimations();
            AddAudio();
        }

        private VisualElement CreateNewDraggableAsset(string assetName, string assetType, DragNewAssetPoint.GetNewObject createAsset, Action<Object> ondrop = null)
        {
            var draggableAsset = new VisualElement();
            draggableAsset.Add(new Label(assetName));
            draggableAsset.Add(new Label(assetType) { name = "asset-type" });
            allManipulators.Add(new DragNewAssetPoint(draggableAsset, createAsset, ondrop));
            draggableAsset.name = "timeline-asset";
            return draggableAsset;
        }

        private VisualElement CreateNewClickableAsset(string assetName, string assetType, Action onClick)
        {
            var clickableAsset = new Button(onClick) { text = assetName };
            clickableAsset.Add(new Label(assetType) { name = "asset-type" });
            clickableAsset.name = "timeline-asset";
            return clickableAsset;
        }
        //private void ShowSelectedTimelineObject()
        //{
        //    Object selectedObject = Selection.objects.First();
        //    if (selectedObject == null || TimelineAssistant.currentDirector == null)
        //    {
        //        return;
        //    }

        //    TrackAsset asset = selectedObject as TrackAsset;
        //    Object editedObject = TimelineAssistant.currentDirector.GetGenericBinding(asset);

        //    if (editedObject == null)
        //    {
        //        return;
        //    }

        //    Selection.activeObject = editedObject;
        //    EditorGUIUtility.PingObject(editedObject);
        //    EditorWindow.GetWindow<SceneView>().FrameSelected();
        //}

        private void SetTabButtonState()
        {
            foreach (var item in tabButtons)
            {
                item.SetEnabled(activlyEditingCharacter != null);
            }
        }

        private void AddSearchBar()
        {
            ToolbarSearchField searchField = new();
            searchField.RegisterValueChangedCallback(evt => ApplySearch(evt.newValue));
            Add(searchField);
            activeDragables.Clear();
        }

        //private void OnSelectionChanged()
        //{
        //    if (Selection.objects.Length == 0)
        //    {
        //        showSelectedTimelineObject.text = "No track Selected";
        //        showSelectedTimelineObject.SetEnabled(false);
        //        return;
        //    }


        //    if (Selection.objects[0] is PlayableDirector trackAsset)
        //    {
        //        TimelineAssistant.currentDirector = trackAsset;
        //    }

        //    if (Selection.objects[0] is TrackAsset && TimelineAssistant.currentDirector != null)
        //    {
        //        if (TimelineAssistant.currentDirector.GetGenericBinding(Selection.objects[0] as TrackAsset) != null)
        //        {
        //            Object genericBinding = TimelineAssistant.currentDirector.GetGenericBinding(Selection.objects[0] as TrackAsset);
        //            showSelectedTimelineObject.text = "Show " + genericBinding.GetType().Name + ": " + genericBinding.name;
        //            showSelectedTimelineObject.SetEnabled(true);
        //        }
        //        else
        //        {
        //            showSelectedTimelineObject.text = "No Object Bound";
        //            showSelectedTimelineObject.SetEnabled(false);
        //        }
        //    }
        //    else
        //    {
        //        showSelectedTimelineObject.text = TimelineAssistant.currentDirector != null ? "No track Selected" : "No Director Selected";
        //        showSelectedTimelineObject.SetEnabled(false);
        //    }
        //}

        public static IEnumerable<RuntimeAnimatorController> GetAnimationControllers()
        {
            string[] guids = AssetDatabase.FindAssets($"t:RuntimeAnimatorController l:Timeline");
            foreach (string t in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(t);
                RuntimeAnimatorController asset = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetPath);
                if (asset != null)
                {
                    yield return asset;
                }
            }
        }

        ~TimelineAssistantVisualElement()
        {
            foreach (PointerManipulator manipulator in allManipulators)
            {
                manipulator.target.RemoveManipulator(manipulator);
            }
            allManipulators.Clear();
        }
    }
}