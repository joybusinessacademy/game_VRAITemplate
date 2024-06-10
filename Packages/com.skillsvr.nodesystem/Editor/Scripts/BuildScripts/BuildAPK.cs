using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using System.Linq;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Management;
using Debug = UnityEngine.Debug;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Android;
#if PICO_XR
using Unity.XR.PXR;
#endif

namespace BuildScripts.Build
{
    public static class BuildAPK
    {
        private static string ShouldBuildAfterReloadKey => "BUILD_AFTER_RELOAD" + Application.dataPath;
        private static string IsPicoBuildKey => "PICO_BUILD" + Application.dataPath;
        private static string macAdbPath = "/Applications/Unity CCK/Unity/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb";

        private static string localMacADB = Path.Combine(Application.dataPath, "Plugins/AndroidTools/adb");
        private static string editorADBPath = AndroidExternalToolsSettings.sdkRootPath + "\\platform-tools\\adb";

        //[MenuItem("SkillsVR CCK/Build APK")]
        public static void BuildGameNoReturn()
        {
            BuildGame();
        }

        public static BuildReport BuildGame(string filename = "/BuiltGame.apk")
        {
            // Get filename.
            var path = BuildUtilities.GetBuildFolderDir();

            SessionState.SetString("APKBuildLocation", path);

            List<string> levels = (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToList();

            string[] devices = ExecuteCommand(editorADBPath, "devices", false).Split("\n");
            // Build player.

            var outputFilePath = path + filename;
            BuildUtilities.SaveLastBuildPath(outputFilePath);

            BuildReport buildReport = BuildPipeline.BuildPlayer(levels.ToArray(), outputFilePath,
                BuildTarget.Android, devices.ToList().FindAll(str => str.Contains("device")).Count > 1 ? BuildOptions.AutoRunPlayer : BuildOptions.None);

            return buildReport;
        }

        public static void BuildToHeadset(bool picoBuild)
        {
            SessionState.SetBool(IsPicoBuildKey, picoBuild);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            // In first time switch to Android, the project may not loading properly,
            // which cause black window in vr with incorrect apk bundle id.
            // Require script reload will refresh project data and scripts update to date.
            SessionState.SetBool(ShouldBuildAfterReloadKey, true);
            EditorUtility.RequestScriptReload();
            // Reload script will cause all editor time code reference lost (i.e. static callbacks and values).
            // Followed build steps will be triggered by [InitializeOnLoadMethod] with SessionState ShouldBuildAfterReloadKey.
            // See CheckShouldStartBuildAfterReload() for next.
        }

        [InitializeOnLoadMethod]
        private static void CheckShouldStartBuildAfterReload()
        {
            bool shouldBuild = SessionState.GetBool(ShouldBuildAfterReloadKey, false);
            if (!shouldBuild)
            {
                return;
            }
            SessionState.EraseBool(ShouldBuildAfterReloadKey);
            // New scene action in build addressable will be fail if called from InitializeOnLoadMethod.
            // Delay start build to make everything works.
            EditorCoroutineUtility.StartCoroutineOwnerless(DelayStartBuild());
        }

        private static IEnumerator DelayStartBuild()
        {
            // wait one frame in case of some actions cannot start from init on load callbacks
            // i.e. create new scene when build addressables.
            yield return null;
            bool pico = SessionState.GetBool(IsPicoBuildKey, false);
            SetupPlatformBeforeBuild(pico);
            PreBuildApk(pico);
        }

        private static IEnumerator CheckPXR(bool isPico)
        {
            if (isPico)
            {
                yield return AddPXRMOnPicoBuild();
            }
            else
            {
                RemovePXRMOnOculusBuild();
            }

            BuildApk(isPico);
        }

        private static void PreBuildApk(bool picoBuild)
        {

            if (Application.isPlaying)
            {
                Debug.Log("Build Failed - Application was running");
                return;
            }
            EditorCoroutineUtility.StartCoroutineOwnerless(CheckPXR(picoBuild));
           
        }

        private static void BuildApk(bool picoBuild)
        {
            string stringTargetFilename = picoBuild ? "/BuiltGame_PXR.apk" : "/BuiltGame_OC.apk";
            BuildReport report = BuildGame(stringTargetFilename);

            if (report == null)
            {
                return;
            }

            if (report.summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed: " + report.summary);
            }
            else if (report.summary.result == BuildResult.Cancelled)
            {
                MoveVideoClipsBuild.DeleteVideosFromStreamingAssets();
                Debug.LogError("Build process was aborted.");
            }
            else
            {
                Debug.Log("Build succeeded!");

                if (MoveVideoClipsBuild.handlingVideosViaPersistantData)
                {
                    MoveVideoClipsBuild.GenerateShareableItems();
                    MoveVideoClipsBuild.MoveVideosToDevice(MoveVideoClipsBuild.GetVideosFromNodes());
                }
                BuildUtilities.OpenFolder(BuildUtilities.GetLastBuildPath());
            }

            // string apkPath = report.files[^1].path;
            // InstallAndRunAndroid(apkPath);
        }

        //No Longer used - Using Default Unity Build System
        [Obsolete]
        public static void InstallAndRunAndroid(string filePath)
        {
            string[] devices = ExecuteCommand(editorADBPath, "devices", false).Split("\n");
            if (devices.Length == 0)
            {
                Debug.LogError("Missing Devices for Install");
                return;
            }

            ExecuteCommand(editorADBPath, "uninstall " + PlayerSettings.applicationIdentifier, false);
            ExecuteCommand(editorADBPath, "install -r " + filePath, false);
            ExecuteCommand(editorADBPath, $"shell am start -n {PlayerSettings.applicationIdentifier}/com.unity3d.player.UnityPlayerActivity", true);
        }

        private static string ExecuteCommand(string command, string arguments, bool runAsync)
        {
			ProcessStartInfo processInfo = new ProcessStartInfo()
            {
                FileName = "cmd",
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments = $"/c \"{command} {arguments}\""
			};

#if UNITY_EDITOR_OSX
			processInfo.FileName = "/bin/bash";
			processInfo.Arguments = $"-c \"{localMacADB} {arguments}\"";			
#endif

            Process process = Process.Start(processInfo);
            if (runAsync)
            {
                if (process != null)
                {
                    process.OutputDataReceived += (_, args) => Debug.Log(args.Data);
                    process.Exited += (_, _) => Debug.Log("external process exited"); ;
                }

                return null;
            }

            Stopwatch stopWatch = new();
            string output = "";
            while (process != null && !process.StandardOutput.EndOfStream)
            {
                output += process.StandardOutput.ReadLine() + "\n";
                if (stopWatch.ElapsedMilliseconds > 60000)
                {
                    output += "Timeout error\n";
                    process.Close();
                    return output;
                }

            }

            if (process != null)
            {
                process.WaitForExit();
                process.Close();
            }

            return output;
        }
        public static void SetupPlatformBeforeBuild(bool picoBuild)
        {


            string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            // remove
            defineSymbols = defineSymbols.Replace(";PICO_XR", string.Empty);
            if (picoBuild)
                defineSymbols += ";PICO_XR";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbols);

            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UniversalRenderPipelineAsset urp = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget buildTargetSettings);
            XRGeneralSettings androidXRSettings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);

            //string loaderOculusTypeName = "Unity.XR.Oculus.OculusLoader, Unity.XR.Oculus, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
            //string loaderPicoTypeName = "Unity.XR.PXR.PXR_Loader, Unity.XR.PICO, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

            string loaderOculusTypeNameShort = "Unity.XR.Oculus.OculusLoader";
            string loaderPicoTypeNameShort = "Unity.XR.PICO.PXR_Loader";


            //UnityEngine.XR.XRSettings.LoadDeviceByName("Oculus");


            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
            XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            XRPackageMetadataStore.AssignLoader(settings.Manager, loaderOculusTypeNameShort, BuildTargetGroup.Android);

            if (picoBuild)
            {
                XRPackageMetadataStore.AssignLoader(settings.Manager, loaderPicoTypeNameShort, BuildTargetGroup.Android);
                XRPackageMetadataStore.RemoveLoader(settings.Manager, loaderOculusTypeNameShort, BuildTargetGroup.Android);
            }
            else
            {
                XRPackageMetadataStore.AssignLoader(settings.Manager, loaderOculusTypeNameShort, BuildTargetGroup.Android);
                XRPackageMetadataStore.RemoveLoader(settings.Manager, loaderPicoTypeNameShort, BuildTargetGroup.Android);
            }

            //var classExist = Type.GetType(loaderPicoTypeName);
            //if (classExist != null && urp != null)
            //{
            //	if (picoBuild)
            //	{
            //		Debug.Log("############################# PICO BUILD #############################");
            //		XRPackageMetadataStore.RemoveLoader(androidXRSettings.Manager, loaderOculusTypeNameShort, BuildTargetGroup.Android);
            //		XRPackageMetadataStore.AssignLoader(androidXRSettings.AssignedSettings, loaderPicoTypeNameShort, BuildTargetGroup.Android);
            //	}
            //	else
            //	{
            //		Debug.Log("############################# OCULUS BUILD #############################");
            //		XRPackageMetadataStore.RemoveLoader(androidXRSettings.Manager, loaderPicoTypeNameShort, BuildTargetGroup.Android);
            //		XRPackageMetadataStore.AssignLoader(androidXRSettings.AssignedSettings, loaderOculusTypeNameShort, BuildTargetGroup.Android);
            //	}

            //}

            EditorUtility.SetDirty(urp);
            EditorUtility.SetDirty(androidXRSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private const string picoManagerName = "[PXR_Manager]";
        private static IEnumerator AddPXRMOnPicoBuild()
        {
            yield return new WaitUntil(() => !EditorApplication.isCompiling);

            string scenePath = SceneUtility.GetScenePathByBuildIndex(0);
            if (!string.IsNullOrEmpty(scenePath))
            {
#if PICO_XR
                UnityEngine.SceneManagement.Scene alreadyOpened = EditorSceneManager.GetActiveScene();
                UnityEngine.SceneManagement.Scene opened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                EditorSceneManager.SetActiveScene(opened);
                GameObject manager = IsPXRPresent(opened);
                if (manager == null)
                {
                    var settings = PXR_ProjectSetting.GetProjectConfig();
                    settings.eyeTracking = true;
                    settings.faceTracking = true;
                    settings.lipsyncTracking = true;

                    manager = Resources.Load(picoManagerName)as GameObject;
                    GameObject.Instantiate(manager);
                    //var pxr = new GameObject(picoManagerName).AddComponent<PXR_Manager>();


                    //pxr.faceTracking = true;
                    //pxr.eyeTracking = true;
                    //pxr.trackingMode = FaceTrackingMode.Hybrid;

                    EditorSceneManager.MarkSceneDirty(opened);
                    EditorSceneManager.SaveScene(opened);
                }

                
                EditorSceneManager.CloseScene(opened, true);
                EditorSceneManager.SetActiveScene(alreadyOpened);
#endif
            }
        }

        private static GameObject IsPXRPresent(UnityEngine.SceneManagement.Scene scene)
        {
            GameObject[] allGameObjects = scene.GetRootGameObjects();

            foreach (GameObject gameObject in allGameObjects)
            {
                if (gameObject.name.Equals(picoManagerName+"(Clone)"))
                {
                    return gameObject;
                }
            }
            return null;
        }


        private static void RemovePXRMOnOculusBuild()
        {
            
            string scenePath = SceneUtility.GetScenePathByBuildIndex(0);

            if (!string.IsNullOrEmpty(scenePath))
            {

                UnityEngine.SceneManagement.Scene alreadyOpened = EditorSceneManager.GetActiveScene();
                UnityEngine.SceneManagement.Scene opened = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                EditorSceneManager.SetActiveScene(opened);

                GameObject manager = IsPXRPresent(opened);
                if (null != manager)
                {
                    GameObject.DestroyImmediate(manager.gameObject);
                    EditorSceneManager.MarkSceneDirty(opened);
                    EditorSceneManager.SaveScene(opened);
                }

                EditorSceneManager.CloseScene(opened, true);
                EditorSceneManager.SetActiveScene(alreadyOpened);
               
            }
        }
    }
}