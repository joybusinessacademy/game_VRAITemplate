using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.AsyncOperation.Registry;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using SkillsVR.CCK.PackageManager.Registry;
using SkillsVR.CCK.PackageManager.Settings;
using SkillsVR.CCK.PackageManager.UI.Events;
using SkillsVR.CCK.PackageManager.UI.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI
{

    public class CCKPackageManagerWindow : EditorWindow
    {
        [MenuItem("SkillsVR CCK/Package Manager")]
        [MenuItem("Window/SkillsVR CCK/Package Manager")]
        public static void ShowExample()
        {
            CCKPackageManagerWindow wnd = GetWindow<CCKPackageManagerWindow>();
            wnd.titleContent = new GUIContent("CCK Package Manager");
        }
        public void CreateGUI()
        {
            var view = new CCKPackageManagerWindowView();
            this.rootVisualElement.Add(view);
        }

        void OnEnable()
        {
            EditorUtility.ClearProgressBar();
        }

        private void OnDisable()
        {
        }
    }
}