using SkillsVRNodes.Editor.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.Scripts
{
    public class AskUserForWav : EditorWindow
    {
        private string description;
        private bool initializedPosition = false;

        private bool shouldClose = false;
        private Vector2 maxScreenPos;

        public bool successfulUpload;
        public string successfulUploadFilepath;
        public string fileName;


        void OnGUI()
        {
            // Check if Esc/Return have been pressed
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    // Escape pressed
                    case KeyCode.Escape:
                        shouldClose = true;
                        e.Use();
                        break;

                    // Enter pressed
                    case KeyCode.Return:
                        shouldClose = true;
                        e.Use();
                        break;
                    case KeyCode.KeypadEnter:
                        shouldClose = true;
                        e.Use();
                        break;
                }
            }

            if (shouldClose)
            {  // Close this dialog
                Close();
            }

            // Draw our control
            Rect rect = EditorGUILayout.BeginVertical();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField(description);
            EditorGUILayout.Space(8);
            EditorGUILayout.EndVertical();

            // Force change size of the window
            if (rect.width != 0 && minSize != rect.size)
            {
                minSize = maxSize = rect.size;
            }

            // Set dialog position next to mouse position
            if (!initializedPosition && e.type == EventType.Layout)
            {
                initializedPosition = true;

                // Move window to a new position. Make sure we're inside visible window
                Vector2 mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                mousePos.x += 32;
                if (mousePos.x + position.width > maxScreenPos.x) mousePos.x -= position.width + 64; // Display on left side of mouse
                if (mousePos.y + position.height > maxScreenPos.y) mousePos.y = maxScreenPos.y - position.height;

                position = new Rect(mousePos.x, mousePos.y, position.width, position.height);

                // Focus current window
                Focus();
            }
        }

        /// <summary>
        /// Returns AudioClip user selected, or null if player cancelled or chose the wrong filetype.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static AudioClip TryAddAudioFile(string title, string description, out string _filename)
        {

            // Make sure our popup is always inside parent window, and never offscreen
            // So get caller's window size
            Vector2 maxPos = GUIUtility.GUIToScreenPoint(new Vector2(Screen.width, Screen.height));

            AudioClip ret = null;

            AskUserForWav window = CreateInstance<AskUserForWav>();

            window.TriggerFileOpen();

            if (window.successfulUpload)
            {
                Debug.Log("JJ " + window.successfulUploadFilepath);
                ret = AssetDatabase.LoadAssetAtPath<AudioClip>(window.successfulUploadFilepath);
            }
            else
            {
                window.maxScreenPos = maxPos;
                window.titleContent = new GUIContent(title);
                window.description = description;

                window.ShowModal();
            }
            _filename = window.fileName;
            return ret;
        }



        public void TriggerFileOpen()
        {
            string filepath = EditorUtility.OpenFilePanelWithFilters("Select .wav or .ogg file", "", new[] { "Audio", "wav,ogg" });

            string[] filepathSplit = filepath.Split('/');
            fileName = filepathSplit[filepathSplit.Length - 1];

            string[] fileNameSplit = fileName.Split('.');
            string suffix = fileNameSplit[fileNameSplit.Length - 1];

            if (suffix.Equals("wav", StringComparison.Ordinal) || suffix.Equals("ogg", StringComparison.Ordinal))
            {
                successfulUploadFilepath = "Assets/Contexts/" + AssetDatabaseFileCache.GetCurrentMainGraphName() + "/Dialogues/CustomAudio";

                if (!Directory.Exists(successfulUploadFilepath))
                {
                    Directory.CreateDirectory(successfulUploadFilepath);
                }

                successfulUploadFilepath += "/" + fileName;

                FileUtil.ReplaceFile(filepath, successfulUploadFilepath);
                AssetDatabase.Refresh();
                successfulUpload = true;
            }
            else
            {
                successfulUpload = false;
            }
        }
    }
}
