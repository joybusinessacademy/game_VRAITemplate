using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;
using System.IO;
using System.Linq;
using System;
using System.Collections;
using Unity.EditorCoroutines.Editor;

static class SVRShaderUtil
{
    private static string REBUILD_SHADERS_KEY = "REBUILD_SHADERS_5348sn";
    [InitializeOnLoadMethod]
    private static void RebuildShadersOnRequest()
    {
        if (!SessionState.GetBool(REBUILD_SHADERS_KEY, false))
        {
            return;
        }

        RebuildShaders();
        SessionState.SetBool(REBUILD_SHADERS_KEY, false);
    }

    [MenuItem("Assets/SkillsVR CCK/Rebuild SVR Shaders")]
    private static void RebuildShaders()
    {
        DeleteLibraryShaderCache();
        RebuildShaderCache();
    }
    private static void DeleteLibraryShaderCache()
    {
        var projPath = Directory.GetCurrentDirectory();
        string cachePath = "Library\\ShaderCache";
        string dir = Path.Combine(projPath, cachePath);
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }

    static void RebuildShaderCache()
    {
        var shaderGuids = AssetDatabase.FindAssets("t:shader SVR_", new string[] { "Assets", "Packages" });
        var ps = shaderGuids.Select(x => AssetDatabase.GUIDToAssetPath(x));
        foreach (var path in ps)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
        }
    }
}
static class ImportPluginPackages
{

    private static string GetCallerFilePath([CallerFilePath] string path = "")
    {
        return path;
    }
    
    private static string GetMyPackageFolderPath()
    {
        var filePath = GetCallerFilePath();
        var projDir = Directory.GetCurrentDirectory();
        filePath = filePath.Substring(projDir.Length).TrimStart('/', '\\').Replace("\\", "/");

        string[] packageSourceDirRoots = new string[] {
            "Packages/",
            "Library/PackageCache/"
        };
        var prefix = packageSourceDirRoots.FirstOrDefault(x => filePath.StartsWith(x));
        if (string.IsNullOrWhiteSpace(prefix))
        {
            throw new System.Exception("Get package folder name fail: The entry path is not belong to a package.\r\nEntry Path: " + filePath);
        }
        filePath = filePath.Substring(prefix.Length);

        int index = filePath.IndexOf('/');
        if (index > 0)
        {
            string folderName = filePath.Substring(0, index);
            return $"{prefix}{folderName}";
        }

        return filePath;
    }

    [Serializable]
    private class PkgInfo
    {
        public string name;
        public string version;
    }
    private static string GetMyPackageVersion()
    {
        var projDir = Directory.GetCurrentDirectory();
        var dir = GetMyPackageFolderPath();
        var pkgJsonPath = Path.Combine(projDir, dir, "package.json");
        var json = File.ReadAllText(pkgJsonPath);
        var pkgInfo = JsonUtility.FromJson<PkgInfo>(json);
        return pkgInfo.version;
    }
}
