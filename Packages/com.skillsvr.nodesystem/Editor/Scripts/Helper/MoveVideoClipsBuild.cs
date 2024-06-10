using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using SkillsVRNodes.Managers.Utility;
using SkillsVRNodes.Scripts.Nodes;
using System;
using System.Diagnostics;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Android;

namespace BuildScripts
{
	public class MoveVideoClipsBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		private const string videoClipFolder = "Videos";
		private const string streamingAssetsFolder = "StreamingAssets";
		private static string macAdbPath = "/Applications/Unity CCK/Unity/PlaybackEngines/AndroidPlayer/SDK/platform-tools/";
		public int callbackOrder => 1;

		[SerializeField]
		private static Dictionary<string, string> videoNamePrePath = new Dictionary<string, string>();
		public static bool handlingVideosViaPersistantData = true;

		public static bool generateShareableAPK = true;

		private static string editorADBPath = AndroidExternalToolsSettings.sdkRootPath + "\\platform-tools\\adb";

		private static void PopulateVideoPathDictonary()
		{
			string[] guids = AssetDatabase.FindAssets("t:VideoClip");

			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				string removedAssetPath = assetPath.Substring(7);// assetPath.Replace("/Assets", "");
				string fullAssetPath = Path.Combine(Application.dataPath, removedAssetPath);
				string clipName = Path.GetFileName(assetPath);

				if (!videoNamePrePath.ContainsKey(clipName))
					videoNamePrePath.Add(clipName, fullAssetPath);
			}
		}

		public static void MoveToStreamingAssets(List<string> videoClipsFromNodes)
		{
			PopulateVideoPathDictonary();

			string streamingAssetsPath = Path.Combine(Application.dataPath, streamingAssetsFolder);

			foreach (var item in videoNamePrePath)
			{
				string videoClipPathFromD = item.Value;

				string fileName = item.Key;
				string destination = Path.Combine(streamingAssetsPath, fileName);

				// Check if the file is a video clip
				string extension = Path.GetExtension(videoClipPathFromD);
				string[] allowedExtensions = { ".mp4", ".mov", ".avi", ".m4v" }; // Add or remove supported video file extensions as needed

				if (!Directory.Exists(streamingAssetsPath))
					Directory.CreateDirectory(streamingAssetsPath);

				string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

				bool containsSubstring = videoClipsFromNodes.Any(str => str.Contains(filenameWithoutExtension));

				if (allowedExtensions.Contains(extension.ToLower()) && containsSubstring)
				{
					// Move video clip to streaming assets
					if (File.Exists(videoClipPathFromD))
						File.Copy(videoClipPathFromD, destination);
				}
			}

			AssetDatabase.Refresh();
		}

		public static void MoveVideosToDevice(List<string> videoClipsFromNodes)
		{

			if (!IsDeviceConnected())
			{
				UnityEngine.Debug.Log("No Android device found. Please connect your device and ensure USB debugging is enabled.");
				return;
			}

			PopulateVideoPathDictonary();

			string destinationDirectory = "/sdcard/Android/data/" + Application.identifier + "/files";

			if (videoClipsFromNodes.Count > 0)
				CreateDirectoryIfNot();

			foreach (var item in videoNamePrePath)
			{
				string videoClipPathFromD = item.Value;
				string fileName = item.Key;

				// Check if the file is a video clip
				string extension = Path.GetExtension(videoClipPathFromD);
				string[] allowedExtensions = { ".mp4", ".mov", ".avi", ".m4v" }; // Add or remove supported video file extensions as needed

				string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

				bool containsSubstring = videoClipsFromNodes.Any(str => str.Contains(filenameWithoutExtension));

				if (allowedExtensions.Contains(extension.ToLower()) && containsSubstring)
				{
					// Move video clip to device
					MoveFileToDevice(videoClipPathFromD, fileName, destinationDirectory);
				}
			}
		}

		private static Process process;

		static void MoveFileToDevice(string sourceFilePath, string fileName, string destinationDirectory)
		{
			if (FileExistsOnDevice(destinationDirectory, fileName))
			{
				UnityEngine.Debug.Log("File already exists on the Android device.");
				return;
			}


			// Use ADB to push the file to the Android device
			//if (process != null && !process.HasExited)
			//{
			//	// Wait for the previous process to exit
			//	process.WaitForExit();
			//}

			process = new Process();

			if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
				process.StartInfo.FileName = "bash";
			else
			{
				process.StartInfo.FileName = editorADBPath;
			}

			process.StartInfo.Arguments = $"push \"{sourceFilePath}\" \"{destinationDirectory}\"";

			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;

			process.Start();
			process.WaitForExit();

			if (process.ExitCode == 0)
			{
				UnityEngine.Debug.Log("File transferred successfully.");
			}
			else
			{
				UnityEngine.Debug.Log("Error transferring file.");
				UnityEngine.Debug.Log(process.StandardError.ReadToEnd());
			}

			process.Close();
		}

		static void CreateDirectoryIfNot()
		{
			string packageName = Application.identifier;
			string destinationDirectory = $"/sdcard/Android/data/{packageName}/files";
			string adbCommand = $"shell mkdir -p \"{destinationDirectory}\"";

			Process process = new Process();

			if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
				process.StartInfo.FileName = "bash";
			else
			{
				process.StartInfo.FileName = editorADBPath;
			}

			process.StartInfo.Arguments = adbCommand;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.CreateNoWindow = true;

			process.Start();
			process.WaitForExit();
		}

		// Function to check if a file exists on the Android device
		static bool FileExistsOnDevice(string directory, string fileName)
		{
			Process process = new Process();

			if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
				process.StartInfo.FileName = "bash";
			else
			{
				process.StartInfo.FileName = editorADBPath;
			}

			process.StartInfo.Arguments = $"shell ls \"{directory}\"";

			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;

			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			string errorOutput = process.StandardError.ReadToEnd(); // Capture error output
			process.WaitForExit();

			// Split the output into lines
			string[] lines = output.Split('\n');

			// Check if the desired file name is present in the output
			foreach (string line in lines)
			{
				if (line.Trim() == fileName)
				{
					return true;
				}
			}

			// Print error output for debugging purposes
			if (errorOutput != string.Empty)
				UnityEngine.Debug.Log("Error Output: " + errorOutput);

			return false;
		}


		// Function to check if an Android device is connected
		static bool IsDeviceConnected()
		{
			Process process = new Process();

			if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX)
			{
				process.StartInfo.FileName = "bash";
				process.StartInfo.Arguments = $"-c \"{macAdbPath} devices\"";
			}
			else
			{
				process.StartInfo.FileName = editorADBPath;
				process.StartInfo.Arguments = "devices";
			}

			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.CreateNoWindow = true;

			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			string[] lines = output.Split('\n');

			// Iterate through the lines to find device entries
			foreach (string line in lines)
			{
				// Skip empty lines
				if (string.IsNullOrWhiteSpace(line))
					continue;

				// Check if a device entry exists (it won't be the header)
				if (!line.Contains("List of devices attached"))
				{
					// At least one device is connected
					return true;
				}
			}

			// No device entries found, no devices connected
			return false;
		}

		public static void DeleteVideosFromStreamingAssets()
		{
			string videoClipPath = Path.Combine(Application.dataPath, videoClipFolder);
			string streamingAssetsPath = Path.Combine(Application.dataPath, streamingAssetsFolder);

			// Move video clips from streaming assets
			string[] videoClipFiles = Directory.GetFiles(streamingAssetsPath);
			foreach (string file in videoClipFiles)
			{
				// Check if the file is a video clip
				string extension = Path.GetExtension(file);
				string[] allowedExtensions = { ".mp4", ".mov", ".avi", ".m4v" }; // Add or remove supported video file extensions as needed

				if (allowedExtensions.Contains(extension.ToLower()))
				{
					// Move video clip to streaming assets
					if (File.Exists(file))
						File.Delete(file);

				}
			}

			AssetDatabase.Refresh();
		}

		public void OnPreprocessBuild(BuildReport report)
		{
			bool projectContainsVideos = false;

			List<string> videosFromNodes = new List<string>();

#if PANORAMA_VIDEO
			videosFromNodes = GetVideosFromNodes();
			if (videosFromNodes.Count > 0)
				projectContainsVideos = true;

#endif

			if (!projectContainsVideos)
				return;

			if (!handlingVideosViaPersistantData)
			{
				//This can be used for small videos or with large videos and mode set to Split Binary OBB
				MoveToStreamingAssets(videosFromNodes);
			}
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			DeleteVideosFromStreamingAssets();

			bool projectContainsVideos = false;

			List<string> videosFromNodes = new List<string>();

#if PANORAMA_VIDEO
			videosFromNodes = GetVideosFromNodes();
			if (videosFromNodes.Count > 0)
				projectContainsVideos = true;
#endif

			if (!projectContainsVideos)
				return;

			if (handlingVideosViaPersistantData)
			{
				EditorCoroutineUtility.StartCoroutineOwnerless(WaitAfterBuild(videosFromNodes));

			}
		}

		public IEnumerator WaitAfterBuild(List<string> videosFromNodes)
		{
			float startTime = (float)EditorApplication.timeSinceStartup;

			while ((float)EditorApplication.timeSinceStartup - startTime < 2)
			{
				// Wait until the desired time has passed
				yield return null;
			}

			MoveVideosToDevice(videosFromNodes);

			startTime = (float)EditorApplication.timeSinceStartup;

			while ((float)EditorApplication.timeSinceStartup - startTime < 2)
			{
				// Wait until the desired time has passed
				yield return null;
			}

			GenerateShareableItems();
		}

		[PostProcessBuild]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
		}

		[MenuItem("Custom/MoveVideosTest")]
		public static void MoveVideoClips()
		{
			CreateDirectoryIfNot();

			IsDeviceConnected();

			return;

			bool projectContainsVideos = false;

			List<string> videosFromNodes = new List<string>();

#if PANORAMA_VIDEO
			videosFromNodes = GetVideosFromNodes();
			if (videosFromNodes.Count > 0)
				projectContainsVideos = true;
#endif

			if (!projectContainsVideos)
				return;

			if (handlingVideosViaPersistantData)
			{
				MoveVideosToDevice(videosFromNodes);
			}
			else
			{
				//This can be used for small videos or with large videos and mode set to Split Binary OBB
				MoveToStreamingAssets(videosFromNodes);
			}
		}

		public static List<string> GetVideosFromNodes()
		{
			List<string> videosFromNodes = new List<string>();
#if PANORAMA_VIDEO

			var item = GraphFinder.CurrentActiveProject;
            if (null != item)
            {
                foreach (var subGraph in item.sceneGraphs)
                {
                    var newSceneNodes = subGraph.graphGraph.Nodes.Where(node => node is PanoramaVideoNode).Select(x => (PanoramaVideoNode)x);
                    if (newSceneNodes.Count() > 0)
                        newSceneNodes.ForEach(x => { if (!string.IsNullOrWhiteSpace(x.videoClipLocation)) videosFromNodes.Add(Path.GetFileNameWithoutExtension(x.videoClipLocation)); });

                    var panelSceneNodes = subGraph.graphGraph.Nodes.Where(node => node is PanelVideoNode).Select(x => (PanelVideoNode)x);
                    if (panelSceneNodes.Count() > 0)
                        panelSceneNodes.ForEach(x => { if (x.mechanicData.videoClipLocation != null) videosFromNodes.Add(Path.GetFileNameWithoutExtension(x.mechanicData.videoClipLocation)); });
                }
            }
#endif
			return videosFromNodes;
		}

		[MenuItem("Custom/GenerateShareableItems")]
		public static void GenerateShareableItems()
		{
			generateShareableAPK = SessionState.GetBool("GenerateShareableToggleValue", false);

			if (!generateShareableAPK)
				return;

			string buildAPKLocation = SessionState.GetString("APKBuildLocation", "");

			if (buildAPKLocation == string.Empty)
				return;

			string[] apkFiles = Directory.GetFiles(buildAPKLocation, "*.apk");

			if (apkFiles.Length == 0)
				return;

			string dataPath = Application.dataPath; // Get the path to the "Assets" folder
			string projectFolderPath = System.IO.Path.GetDirectoryName(dataPath); // Get the parent folder (project folder)

			string apkSharingSystemFolder = "CCK_ShareableAPK";
			string directoryFolderName = "ShareableAPK";
			string directoryLocation = projectFolderPath + "/" + apkSharingSystemFolder + "/" + directoryFolderName;

			if (!Directory.Exists(directoryLocation))
				Directory.CreateDirectory(directoryLocation);
			else
			{
				//Clean Directory Folder
				Directory.Delete(directoryLocation, true);
				Directory.CreateDirectory(directoryLocation);
			}

			List<string> videosFromNodes = new List<string>();
			videosFromNodes = GetVideosFromNodes();

			if (videosFromNodes.Count == 0)
				return;

			CopyVideosToNewLocation(directoryLocation, videosFromNodes);

			string sourceApkPath = apkFiles[0];
			string destinationApkPath = Path.Combine(directoryLocation, Path.GetFileName(sourceApkPath));
			File.Copy(sourceApkPath, destinationApkPath, true);
		}

		private static void CopyVideosToNewLocation(string newLocation, List<string> clipsFromNodes)
		{
			PopulateVideoPathDictonary();

			foreach (var item in videoNamePrePath)
			{
				string videoClipPathFromD = item.Value;

				string fileName = item.Key;
				string destination = Path.Combine(newLocation, fileName);

				// Check if the file is a video clip
				string extension = Path.GetExtension(videoClipPathFromD);
				string[] allowedExtensions = { ".mp4", ".mov", ".avi", ".m4v" }; // Add or remove supported video file extensions as needed

				if (!Directory.Exists(newLocation))
					Directory.CreateDirectory(newLocation);

				string filenameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

				bool containsSubstring = clipsFromNodes.Any(str => str.Contains(filenameWithoutExtension));

				if (allowedExtensions.Contains(extension.ToLower()) && containsSubstring)
				{
					// Move video clip to streaming assets
					if (File.Exists(videoClipPathFromD))
						File.Copy(videoClipPathFromD, destination);
				}
			}
		}
	}
}
