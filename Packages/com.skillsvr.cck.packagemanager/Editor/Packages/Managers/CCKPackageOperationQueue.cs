using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.Managers
{
    public enum CCKPackageOperationState
    {
        Unknown,
        QueueToInstall,
        Installing,
        QueueToUninstall,
        Uninstalling,
    }

    public class PackageQueueItem
    {
        public string packageId;
        public CCKPackageOperationState state;
        public bool cancelled;
    }

    public class CCKPackageOperationQueue: IEnumerable<PackageQueueItem>, IEnumerable
    {
        public static CCKPackageOperationQueue MainQueue { get; private set; } = new CCKPackageOperationQueue();

        private Queue<PackageQueueItem> packageOperationQueue = new Queue<PackageQueueItem>();
        public CCKPackageOperationState GetPackageState(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return CCKPackageOperationState.Unknown;
            }
            return packageOperationQueue.FirstOrDefault(x => x.packageId == packageId && !x.cancelled)?.state ?? CCKPackageOperationState.Unknown;
        }

        public void Install(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return;
            }
            if (null != packageOperationQueue.FirstOrDefault(x => x.packageId == packageId && !x.cancelled))
            {
                return;
            }
            packageOperationQueue.Enqueue(new PackageQueueItem() { packageId = packageId, state = CCKPackageOperationState.QueueToInstall });
        }

        public void Uninstall(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return;
            }
            if (null != packageOperationQueue.FirstOrDefault(x => x.packageId == packageId && !x.cancelled))
            {
                return;
            }
            packageOperationQueue.Enqueue(new PackageQueueItem() { packageId = packageId, state = CCKPackageOperationState.QueueToUninstall });
        }

        public void Cancel(string packageId)
        {
            if (string.IsNullOrWhiteSpace(packageId))
            {
                return;
            }
            var item = packageOperationQueue.FirstOrDefault(x => x.packageId == packageId && !x.cancelled);
            if (null == item)
            {
                return;
            }

            if (item.state == CCKPackageOperationState.Installing 
                || item.state == CCKPackageOperationState.Uninstalling)
            {
                return;
            }

            item.cancelled = true;
        }

        public IEnumerator<PackageQueueItem> GetEnumerator()
        {
            return packageOperationQueue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return packageOperationQueue.GetEnumerator();
        }
        /*
private static IEnumerator ProcQueue()
{
   PackageQueueItem current = null;

   while (packageOperationQueue.Count > 0)
   {
       packageOperationQueue.TryPeek(out current);
       if (null == current)
       {
           packageOperationQueue.Dequeue();
       }
   }
}

protected IEnumerator ProcItem(PackageQueueItem item)
{
   if (null == item)
   {
       yield break;
   }
   if (item.cancelled
       || string.IsNullOrWhiteSpace(item.packageId)
       || CCKPackageOperationState.Unknown == item.state
       || CCKPackageOperationState.Installing  == item.state
       || CCKPackageOperationState.Uninstalling == item.state)
   {
       yield break;
   }

   var packageInfo = CCKPackageManagerI.GetInfoFromId(item.packageId);
   if (null == packageInfo)
   {
       yield break;
   }

   switch (item.state)
   {
       case CCKPackageOperationState.QueueToInstall:
           item.state = CCKPackageOperationState.Installing;
           break;
       case CCKPackageOperationState.QueueToUninstall:
           item.state = CCKPackageOperationState.Uninstalling;
           break;
       default: break;
   }
}*/
    }
}