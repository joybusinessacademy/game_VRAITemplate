using SkillsVRNodes.Editor.Graph;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SkillsVRNodes.Managers
{
    public static class ContextSceneManager
	{
        public static string CONTEXT_SCENE_FOLDER_PATH => "Assets/Contexts/" + AssetDatabaseFileCache.GetCurrentMainGraphName() + "/Scenes/";


        public static IEnumerable<string> GetAllContextScenePath()
        {
            IEnumerable<string> scenes = AssetDatabase.FindAssets("t:scene", new[] { CONTEXT_SCENE_FOLDER_PATH }).Select(AssetDatabase.GUIDToAssetPath);
            return scenes;
        }
    }
}
