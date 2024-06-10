using NUnit.Framework;
using SkillsVR.CCK.PackageManager;
using SkillsVR.CCK.PackageManager.AsyncOperation;
using SkillsVR.CCK.PackageManager.AsyncOperation.PackageOperations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.TestTools;

public class QueuedClientRequestTests
{
    [UnityTest]
    public IEnumerator List_ExecSuccessful_ResultNotNull()
    {
        var op = new QueuedClientRequest<ListRequest>(() => { return Client.List(true); });
        yield return op;
        Assert.IsNotNull(op.Result);
    }

    [UnityTest]
    public IEnumerator List_ExecSuccessful_ResultIsClientRequest()
    {
        var op = new QueuedClientRequest<ListRequest>(() => { return Client.List(true); });
        yield return op;
        Assert.IsTrue(op.Result is Request);
    }

    [UnityTest]
    public IEnumerator NormalMultipleRequests_AddPackagex10_AnyFailWithConflictError()
    {
        string pkgAddIdentifier = "file:/Some/Fake/Package/That/Not/Exists/";

        List<AddRequest> list = new List<AddRequest>();
        for (int i = 0; i < 20; i++)
        {
            var op = Client.Add(pkgAddIdentifier);
            list.Add(op);
        }
        yield return new WaitUntil(() => list.All(x => x.IsCompleted));
        foreach(var item in list)
        {
            Debug.Log(item.Error.message);
        }
        Assert.IsTrue(list.All(x => null != x.Error));
        Assert.IsTrue(list.Any(x => x.Error.message.Contains(QueuedClientRequest.REQUEST_CONFLICT_MESSAGE)));
    }

    [UnityTest]
    public IEnumerator QueuedMultipleRequests_StartAddPackagex10_NoConflictError()
    {
        string pkgAddIdentifier = "file:/Some/Fake/Package/That/Not/Exists/";

        List<QueuedClientRequest> list = new List<QueuedClientRequest>();
        for(int i = 0; i < 10; i++)
        {
            var op = new QueuedClientRequest<AddRequest>(() => { return Client.Add(pkgAddIdentifier); });
            list.Add(op);
            op.StartCoroutine();
        }

        yield return new WaitUntil(() => list.All(x=> x.IsComplete));
        foreach(var op in list)
        {
            Debug.Log(op.Error);
        }
        Assert.IsTrue(list.All(x => null != x.Result));
        Assert.IsTrue(list.All(x => null != x.Result.Error));
        Assert.IsFalse(list.Any(x => null != x.Result.Error && x.Result.Error.message.Contains(QueuedClientRequest.REQUEST_CONFLICT_MESSAGE)));
    }
}
