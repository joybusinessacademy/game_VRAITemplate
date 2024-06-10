using SkillsVR.CCK.PackageManager;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

/// <summary>
/// See https://app.clickup.com/9003107213/v/dc/8ca10wd-31942/8ca10wd-25596 for details
/// </summary>
public class AzureNpmInstaller : EditorWindow
{
    protected static string CacheProjectDir =>
        Path.Combine(
            Application.dataPath.Replace("Assets", "")
            , "Temp/SkillsVR CCK/Cache/AzureNpmPackages")
        .Replace("\\", "/");

    protected static string EditorNPMPath =>
        Path.Combine(
            Path.GetDirectoryName(EditorApplication.applicationPath)
            , "Data/Tools/nodejs/npm.cmd")
        .Replace("\\", "/");

    [MenuItem("Window/Azure Npm Installer")]
    public static void ShowExample()
    {
        AzureNpmInstaller wnd = GetWindow<AzureNpmInstaller>();
        wnd.titleContent = new GUIContent("Azure Npm Installer");

        AzureNpmInstaller.InstallAsync("your pakcage name", "package version");
    }

    protected TextField pkgNameInput;
    protected TextField versionInput;
    protected Button InstallButton;

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        Label title = new Label();
        title.text = "NPM Install:";
        root.Add(title);
        pkgNameInput = new TextField("Package Name");
        pkgNameInput.value = "com.skillsvr.environment.cckbank";
        root.Add(pkgNameInput);

        versionInput = new TextField("version");
        versionInput.value = "1.0.1";
        root.Add(versionInput);

        InstallButton = new Button();
        InstallButton.text = "Install";
        InstallButton.clicked += OnInstallButtonClick;
        root.Add(InstallButton);
    }

    protected void OnInstallButtonClick()
    {
        Install(pkgNameInput.value, versionInput.value);
    }

    public void Install(string pkg, string version)
    {
        InstallAsync(pkg, version).StartCoroutine();
    }

    public static IEnumerator InstallAsync(string pkg, string version)
    {
        PrepareCacheDir();
        string cmd = $"install \"{pkg}@{version}\" --prefix \"{CacheProjectDir}\"";
        yield return null;

        string fullCmd = $"\"{EditorNPMPath}\" {cmd}"; // /C flag tells cmd to execute the command and then terminate

        string bat = MakeInstallBat(fullCmd);
        int code = RunWinCommand($"\"{bat}\"");
        if (0 != code)
        {
            yield break;
        }
        
        var path = Path.Combine(CacheProjectDir, "node_modules", pkg).Replace("\\", "/") + "/";
        Debug.Log(path);


        var pakOp = Client.Pack(path, CacheProjectDir);
        while(!pakOp.IsCompleted)
        {
            yield return null;
        }

        var pakPath = pakOp.Result.tarballPath;

        string fileP = "file:/" + pakPath.Substring("D:/".Length);
        var req = Client.Add(fileP);
        Debug.Log("Start add pkg " + fileP);
        while (!req.IsCompleted)
        {
            yield return null;
        }
        if (null != req.Error)
        {
            Debug.Log(req.Error.message);
        }
        else
        {
            Debug.Log("done");
        }
    }

    protected static string MakeInstallBat(string cmd)
    {
        string dir = CacheProjectDir;
        string path = Path.Combine(dir, "Install.bat").Replace("\\", "/");

        File.WriteAllText(path, $"@echo off\r\n{cmd}\r\n");
        return path;
    }

    protected static void PrepareCacheDir()
    {
        string dir = CacheProjectDir;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        PrepareFile(".npmrc", 
            "registry=https://pkgs.dev.azure.com/PrideAndJoy/CCK/_packaging/cck/npm/registry/\r\n" +
            "always-auth=true");

        PrepareFile("package.json",
            "{\r\n" +
            "    \"name\": \"com.skillsvr.cck.azurepackagedownloadcache\",\r\n" +
            "    \"version\": \"1.0.0\",\r\n" +
            "    \"displayName\": \"SVR: CCK Azure Package Download Cache\",\r\n" +
            "    \"description\": \"A cache package folder to download cck package from azure.\"\r\n" +
            "}\r\n");
    }

    protected static void PrepareFile(string fileName, string text)
    {
        string dir = CacheProjectDir;
        string path = Path.Combine(dir, fileName);

        if (File.Exists(path))
        {
            return;
        }
        File.WriteAllText(path, text);
    }

    
    protected static int RunWinCommand(string cmd)
    {
        // Create process start info
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "cmd.exe";
        psi.Arguments = $"/C {cmd}"; // /C flag tells cmd to execute the command and then terminate
        Debug.Log(psi.Arguments);
        psi.UseShellExecute = false; // Required to redirect standard output/error
        psi.CreateNoWindow = true; // Set to true to hide the command prompt window
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        // Create and start the process
        Process process = new Process();
        process.StartInfo = psi;
        process.EnableRaisingEvents = true;
        process.OutputDataReceived += (sender, e) => {
            if (string.IsNullOrWhiteSpace(e.Data))
            {
                return;
            }
            Debug.Log(e.Data);
        };// Log output
        process.ErrorDataReceived += (sender, e) => {
            if (string.IsNullOrWhiteSpace(e.Data))
            {
                return;
            }
            Debug.LogError(e.Data);
        }; // Log errors
        process.Start();

        // Begin asynchronously reading the output/error streams
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // Wait for the process to exit
        process.WaitForExit();

        int code = process.ExitCode;
        // Log exit code
        Debug.Log($"Exit code: {process.ExitCode}");
        process.Close();
        return code;
    }
}