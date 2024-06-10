using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Managers
{

    public enum PackageRegistrationState
    {
        NoChange,
        Added,
        Removed,
        ChangedFrom,
        ChangedTo,
        OnAdding,
    }

    public class LocalPackageManager
    {
        public static LocalPackageManager Current { get; protected set; } = new LocalPackageManager();


        public event Action<string, PackageRegistrationState> OnPackageRegistrationChanged;

        protected Dictionary<string, PackageInfo> managedLocalPackageInfos = new Dictionary<string, PackageInfo>();

        public bool IsLoading { get; protected set; }
        public LocalPackageManager()
        {
            Events.registeredPackages += Events_registeredPackages;
            Events.registeringPackages += Events_registeringPackages; ;
            ReloadLocalPackages();
        }


        ~LocalPackageManager()
        {
            Events.registeredPackages -= Events_registeredPackages;
            Events.registeringPackages -= Events_registeringPackages; ;
        }

        public void ReloadLocalPackages()
        {
            ReloadLocalPackagesRoutine().StartCoroutine();
        }

        private void Events_registeringPackages(PackageRegistrationEventArgs args)
        {
            if (null != args.added)
            {
                foreach (var info in args.added)
                {
                    TryInvokePackageRegistrationEvent(info.name, PackageRegistrationState.OnAdding);
                }
            }
        }
        private void Events_registeredPackages(PackageRegistrationEventArgs args)
        {
            if (null != args.removed)
            {
                foreach (var info in args.removed)
                {
                    if (managedLocalPackageInfos.ContainsKey(info.name))
                        managedLocalPackageInfos.Remove(info.name);

                    TryInvokePackageRegistrationEvent(info.name, PackageRegistrationState.Removed);
                }
            }

            if (null != args.changedFrom)
            {
                foreach (var info in args.changedFrom)
                {
                    if (managedLocalPackageInfos.ContainsKey(info.name))
                        managedLocalPackageInfos.Remove(info.name);

                    TryInvokePackageRegistrationEvent(info.name, PackageRegistrationState.ChangedFrom);
                }
            }

            if (null != args.added)
            {
                foreach (var info in args.added)
                {
                    if (!managedLocalPackageInfos.ContainsKey(info.name))
                        managedLocalPackageInfos.Add(info.name, info);

                    TryInvokePackageRegistrationEvent(info.name, PackageRegistrationState.Added);
                }
            }

            if (null != args.changedTo)
            {
                foreach (var info in args.changedTo)
                {
                    if (!managedLocalPackageInfos.ContainsKey(info.name))
                        managedLocalPackageInfos.Add(info.name, info);

                    TryInvokePackageRegistrationEvent(info.name, PackageRegistrationState.ChangedTo);
                }
            }
        }

        public void ReloadAsEmbedPackage(PackageInfo package)
        {
            if (!package.IsEmbed())
            {
                return;
            }
            managedLocalPackageInfos.Remove(package.name);
            managedLocalPackageInfos.Add(package.name, package);
            TryInvokePackageRegistrationEvent(package.name, PackageRegistrationState.ChangedFrom);
            TryInvokePackageRegistrationEvent(package.name, PackageRegistrationState.ChangedTo);
        }

        protected void TryInvokePackageRegistrationEvent(string name, PackageRegistrationState state)
        {
            try
            {
                OnPackageRegistrationChanged?.Invoke(name, state);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public bool IsInstalled(string packageName)
        {
            return null != Get(packageName);
        }

        public bool IsEmbed(string packageName)
        {
            var pkg = Get(packageName);
            return pkg.IsEmbed();
        }

        public string GetVersion(string packageName)
        {
            var info = Get(packageName);
            if (null == info)
            {
                return string.Empty;
            }
            return info.version;
        }

        public PackageInfo Get(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                return null;
            }

            PackageInfo info = null;
            managedLocalPackageInfos.TryGetValue(packageName, out info);
            return info;
        }

        public IEnumerator WaitReady(Action onComplete = null)
        {
            if (managedLocalPackageInfos.Count > 0)
            {
                onComplete?.Invoke();
                yield break;
            }
            yield return Reload(onComplete);
        }

        public IEnumerator Reload(Action onComplete = null)
        {
            if (!IsLoading)
            {
                yield return ReloadLocalPackagesRoutine();
            }
            else
            {
                while (IsLoading)
                {
                    yield return null;
                }

            }
            onComplete?.Invoke();
        }

        protected IEnumerator ReloadLocalPackagesRoutine()
        {
            IsLoading = true;
            var listOp = new QueuedClientRequest<ListRequest>(() => Client.List(true));
            yield return listOp.StartWithoutErrorBreak();

            managedLocalPackageInfos.Clear();

            listOp.TryLogError();
            var listRequest = listOp.Result;
            if (null != listRequest && null != listRequest.Result)
            {
                foreach (var info in listOp.Result.Result)
                {
                    managedLocalPackageInfos.Add(info.name, info);
                    TryInvokePackageRegistrationEvent(info.name, PackageRegistrationState.Added);
                }
            }
            IsLoading = false;
        }
    }
}