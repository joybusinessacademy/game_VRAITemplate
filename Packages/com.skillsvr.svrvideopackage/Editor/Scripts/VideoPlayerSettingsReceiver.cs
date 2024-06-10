using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class VideoPlayerSettingsReceiver : IPreprocessBuildWithReport
{
    public int callbackOrder => 100;

    public void OnPreprocessBuild(BuildReport report)
    {
        ApplyVideoPersistanceSettingFromBuildSettings();
    }

    public static void ApplyVideoPersistanceSettingFromBuildSettings()
    {
        bool usingPersistanceVideoData = GetVideoViaPersistanceSettingValue();
        VideoPlayerItem.VideoLocation location = usingPersistanceVideoData 
            ? VideoPlayerItem.VideoLocation.RelativeToPersistentDataFolder
            : VideoPlayerItem.VideoLocation.RelativeToStreamingAssetsFolder;
        SetLocationValueToAllVideoData(location);
    }

    // Get the bool value from handlingVideosViaPersistantData in class BuildScripts.MoveVideoClipsBuild.
    public static bool GetVideoViaPersistanceSettingValue()
    {
        // namespace BuildScripts
        // class MoveVideoClipsBuild
        // {
        //   public static bool handlingVideosViaPersistantData = true;
        // }
        //
        string videoBuildClassName = "BuildScripts.MoveVideoClipsBuild";
        string presistanceDataValueName = "handlingVideosViaPersistantData";

        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => null != a.GetType(videoBuildClassName, false));

        if (null == assembly)
        {
            throw new Exception("Cannot get video build setting class: " + videoBuildClassName);
        }
        var type = assembly.GetType(videoBuildClassName, false);
        var field = type.GetField(presistanceDataValueName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        bool persisitance = (bool)field.GetValue(null);

        return persisitance;
    }

    protected static void SetLocationValueToAllVideoData(VideoPlayerItem.VideoLocation videoLocation)
    {
        var allAssets = GetAllVideoPlayyerItemAssets();
        foreach(var asset in allAssets)
        {
            asset.videoLocationType = videoLocation;
            EditorUtility.CopySerialized(asset, asset);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Set video location {videoLocation} to {allAssets.Count()} video player items.");
    }

    protected static IEnumerable<VideoPlayerItem> GetAllVideoPlayyerItemAssets()
    {
        var guids = AssetDatabase.FindAssets($"t: {nameof(VideoPlayerItem)}");
        return guids.Select(id => AssetDatabase.LoadAssetAtPath<VideoPlayerItem>(AssetDatabase.GUIDToAssetPath(id)));
    }
}