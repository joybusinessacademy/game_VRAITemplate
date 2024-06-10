using NUnit.Framework;
using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.AsyncOperation.Networking;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class DownloadFileFromUrlAsyncOperationTests
{
    [UnityTest]
    public IEnumerator DownloadGoogleText_WaitComplete_ResultNotNull()
    {
        string url = "www.google.com";

        var op = new DownloadFromUrlAsyncOperation(url, 30);
        yield return op;

        Assert.IsTrue(op.IsComplete, "Is complete");
        Assert.IsNull(op.Error, "Error");
        Assert.AreEqual(OperationState.Success, op.State, "State");
        Assert.IsNotNull(op.Result, "Result");
    }

    [UnityTest]
    public IEnumerator DownloadGoogleImg_WaitComplete_ResultNotNull()
    {
        string url = "https://www.google.com/logos/doodles/2024/celebrating-the-flat-white-6753651837110463-s.png";

        var op = new DownloadTextureFromUrlAsyncOperation(url, 30);
        yield return op;

        Assert.IsTrue(op.IsComplete, "Is complete");
        Assert.IsNull(op.Error, "Error");
        Assert.AreEqual(OperationState.Success, op.State, "State");
        Assert.IsNotNull(op.Result, "Result");

        Debug.Log($"Download Image Done: size {op.Result.width}, {op.Result.height}");
    }
}
