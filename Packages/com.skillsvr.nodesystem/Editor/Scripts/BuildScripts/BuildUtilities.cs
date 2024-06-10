using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BuildScripts.Build
{
	public class BuildUtilities
	{
		public static string GetBuildFolderDir()
		{
            string path = Application.dataPath.Replace("Assets", "Builds");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static void OpenFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            try
            {
                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    path = path.Replace("/", "\\");
                    Process.Start("explorer.exe", "/select," + path);
                }
                else if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    Process.Start("open", path);
                }
                else if (Application.platform == RuntimePlatform.LinuxEditor)
                {
                    Process.Start("xdg-open", path);
                }
            }
            catch { }
        }


        private const string OUTPUT_PATH_KEY = "BUILD_OUTPUT_PATH_u23RGQWBQ";
        public static void SaveLastBuildPath(string path)
        {
            path = string.IsNullOrWhiteSpace(path) ? GetBuildFolderDir() : path;
            SessionState.SetString(OUTPUT_PATH_KEY, path);
        }
        public static string GetLastBuildPath()
        {
            return SessionState.GetString(OUTPUT_PATH_KEY, GetBuildFolderDir());
        }
    }
}