using SkillsVRNodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace CCKManagers
{
	
	[InitializeOnLoad]
	public static class LayoutManager
	{
		public static string FirstOpenString => Application.dataPath+"SkillsNode-HasBeenOpened";
		private const string CCK_LOGGEDIN = "CCK_USERLOGGEDIN";

		static LayoutManager()
		{
			// Resets the layout on project open
			if (!SessionState.GetBool("FirstInitDone", false))
			{
				EditorCoroutineUtility.StartCoroutineOwnerless(ResetLayoutCoroutine());
 
				
				SessionState.SetBool("FirstInitDone", true);
			}
		}

		public static IEnumerator ResetLayoutCoroutine()
		{
			while (EditorApplication.isCompiling)
				yield return null;

			//Set From Login Manager
			while (SessionState.GetBool(CCK_LOGGEDIN, false) == false)
				yield return null;

			//Wait for Editor Loginwindow to be removed fully
			yield return new EditorWaitForSeconds(1);

			ResetLayoutA();

			yield return new EditorWaitForSeconds(1);

			ClearEditorConsole();
		} 
		
		private static void ClearEditorConsole()
		{
			var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
			var type = assembly.GetType("UnityEditor.LogEntries");
			var method = type.GetMethod("Clear");
			method.Invoke(new object(), null);
		}

        [MenuItem("SkillsVR CCK/Reset Layout/Layout A", false, 99999)]
		public static void ResetLayoutA()
		{
            CCKDebug.Log("Used Menu Item: CCK_A");
			TryLoadCCKLayout("Editor/Resources/Layout/CCK_A.wlt");
			ChangeGizmoSize();

			ClearEditorConsole();
		}

		[MenuItem("SkillsVR CCK/Reset Layout/Layout B", false, 99999)]
		public static void ResetLayoutB()
		{
            CCKDebug.Log("Used Menu Item: CCK_B");
            TryLoadCCKLayout("Editor/Resources/Layout/CCK_B.wlt");
            ChangeGizmoSize();

			ClearEditorConsole();
		}

		public static void ResetTimelineLayout()
		{
            TryLoadCCKLayout("Editor/Resources/Layout/CCK_TL.wlt");
        }


		/// <summary>
		/// Try find and apply layout .wlt file within package.
		/// </summary>
		/// <param name="relativeLayoutFilePath">The layout file path relative to package folder folder. 
		/// i.e. use A/B/MyLayout.wlt if the file is at MyPackageFolder/A/B/MyLayout.wlt.
		/// </param>
		/// <returns></returns>
		private static bool TryLoadCCKLayout(string relativeLayoutFilePath)
		{
			if (string.IsNullOrWhiteSpace(relativeLayoutFilePath))
			{
				return false;
			}

            // LayoutUtility.LoadLayoutFromAsset request real full file path.
			// And if package is in PackageCache, the folder path is dynamic (with commit hash),
			// so always get dynamic package dir here.
            string pkgDir = GetMyPackagePath();
			if (string.IsNullOrWhiteSpace(pkgDir))
			{
				pkgDir = Application.dataPath.Replace("Assets", "/");
			}
			string fullLayoutFilePath = Path.Combine(pkgDir, relativeLayoutFilePath).Replace("\\", "/");
			bool loadSuccess = LayoutUtility.LoadLayoutFromAsset(fullLayoutFilePath);

			if (loadSuccess)
			{
				return true;
			}

			loadSuccess = FindAssetsAndLoadFirstCCKLayout(relativeLayoutFilePath);

			if (!loadSuccess)
			{
				Debug.LogError("Cannot find cck layout at " + relativeLayoutFilePath);
			}
			return loadSuccess;
        }

		private static bool FindAssetsAndLoadFirstCCKLayout(string layoutFile)
		{
            string fileWithoutExtension = Path.GetFileNameWithoutExtension(layoutFile);
            var guids = AssetDatabase.FindAssets(fileWithoutExtension, new string[] { "Assets", "Packages" });
            foreach (var guid in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.EndsWith(".wlt")
                    && LayoutUtility.LoadLayoutFromAsset(p.Replace("/", "\\")))
                {
                    return true;
                }
            }
			return false;
        }
		private static void ChangeGizmoSize()
		{
			Type annotationUtilityType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AnnotationUtility");

			if (annotationUtilityType != null)
			{
				PropertyInfo iconSizeField = annotationUtilityType.GetProperty("iconSize", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

				if (iconSizeField != null)
					iconSizeField.SetValue(null, 0);

			}
		}

		private static string GetMyFilePath([CallerFilePath] string path = null)
		{
			return path;
		}

		private static string GetMyPackagePath()
		{
			// Get full path for this file,
			// like C:/Projects/CCK/LibraryCache/NodePackage@234/Editor/Scripts/CCKManagers/LayoutManager.cs;
			string filePath = GetMyFilePath().Replace("\\", "/");
			string projPath = Application.dataPath.Replace("Assets", "").Replace("\\", "/");

			string[] dirs = new string[] {
				Path.Combine(projPath, "Packages").Replace("\\", "/"),
				Path.Combine(projPath, "Library/PackageCache").Replace("\\", "/")
			};

            // Pick the package source dir like C:/Projects/CCK/LibraryCache
            var root = dirs.FirstOrDefault(x => filePath.StartsWith(x));
			if (string.IsNullOrWhiteSpace(root))
			{
				return null;
			}

            // Get the package folder name like NodePackage@234
            string pkgDirName = filePath.Substring(root.Length).TrimStart('/');
			var firstSectionIndex = pkgDirName.IndexOf('/');
			if (0 > firstSectionIndex)
			{
				return null;
			}
			string pkgFolderName = pkgDirName.Substring(0, firstSectionIndex);

            // Return full package path like C:/Projects/CCK/LibraryCache/NodePackage@234.
            string fullPkgPath = Path.Combine(root, pkgFolderName).Replace("\\", "/");
            return fullPkgPath;
		}
	}
}