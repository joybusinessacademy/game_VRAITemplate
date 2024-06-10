using SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations;
using SkillsVR.CCK.PackageManager.Data;
using SkillsVR.CCK.PackageManager.Managers;
using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation.CCKPackageOperations
{
    // Uninstall package by
    // - Remove symbolic link for embed package
    // - Or delete embed package folder
    // - Remove package by Client
    public class UninstallCCKPackageAsyncOperation : CustomAsyncOperation
    {
        public CCKPackageInfo Package { get; protected set; }
        public string PackageIdOrName { get; protected set; }
        public bool ShowConfirmDialog { get; protected set; }
        public UninstallCCKPackageAsyncOperation(string packageIdOrName, bool showConfirmDialog = true)
        {
            PackageIdOrName = packageIdOrName;
            ShowConfirmDialog = showConfirmDialog;
        }

        public UninstallCCKPackageAsyncOperation(CCKPackageInfo package, bool showConfirmDialog = true)
        {
            Package = package;
            ShowConfirmDialog = showConfirmDialog;
        }

        protected override IEnumerator OnProcessRoutine()
        {
            if (null == Package)
            {
                Package = CCKPackageManagment.Main.GetPackageInfo(PackageIdOrName);
            }
            
            if (null == Package)
            {
                SetError($"No package found by id {PackageIdOrName}");
                yield break;
            }

            yield return LocalPackageManager.Current.WaitReady();

            if (!Package.IsInstalled())
            {
                yield break;
            }

            if (ShowConfirmDialog)
            {
                string title = "Remove Package - " + Package.displayName + " v" + Package.GetInstalledVersion();
                string msg = "You will load all your changes (if any) if you delete a package in development.\r\n" +
                    "Are you sure?";
                if (!EditorUtility.DisplayDialog(title, msg, "Yes, Remove Package", "No"))
                {
                    yield break;
                }
            }
            

            if (Package.IsEmbed())
            {
                TryRemoveEmbedPackageFiles();
            }

            var op = new QueuedClientRequest<RemoveRequest>(() => Client.Remove(Package.name));
            yield return op;
            TrySetErrorFrom(op);
            Debug.Log("Uninstall " + Package.name);
        }

        protected void TryRemoveEmbedPackageFiles()
        {
            try
            {
                string dirToDelete = Package.GetUnityPackageInfo().resolvedPath;
                if (!Directory.Exists(dirToDelete))
                {
                    return;
                }
                bool removeContents = !IsSymbolicLink(dirToDelete);
                Directory.Delete(dirToDelete, removeContents);
            }
            catch (Exception e)
            {
                SetError(e.Message + "\r\n" + e.StackTrace + "\r\n");
            }
        }

        protected bool IsSymbolicLink(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            return (attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
        }
    }
}