using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillsVR.CCK.PackageManager.AsyncOperation
{
    public static class ICustomAsyncOperationExtensions
    {
        public static IEnumerator StartWithoutErrorBreak(this ICustomAsyncOperation operation)
        {
            if (null == operation)
            {
                yield break;
            }
            operation.StartCoroutine();
            while(!operation.IsComplete)
            {
                yield return null;
            }
        }
        public static IEnumerable<string> ToExtraInfoText(this ICustomAsyncOperation operation, string title)
        {
            List<string> list = new List<string>();
            list.Add($"{(null == title ? "Operation" : title)}: {(null == operation ? "null" : "")}");
            if (null != operation)
            {
                list.AddRange(operation.GetExtraInfoStrings().Select(x => "  " + x));
            }
            return list;
        }

        public static void TryLogError(this ICustomAsyncOperation operation)
        {
            if (null == operation)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(operation.Error))
            {
                return;
            }

            string error = operation.Error + "\r\n";
            error += operation.ToString();
            error += "\r\n\r\n";
            error += operation.ErrorStackTrace.ToText("Stack Trace");
            Debug.LogError(error);
        }
    }
}