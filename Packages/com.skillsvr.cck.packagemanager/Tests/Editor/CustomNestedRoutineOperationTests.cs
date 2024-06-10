using NUnit.Framework;
using SkillsVR.CCK.PackageManager.AsyncOperation;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class CustomNestedRoutineOperationTests
{
    protected IEnumerator NestedRoutineRoot(IEnumerator sub)
    {
        yield return null;
        yield return sub;
    }

    protected IEnumerator YieldReturnNull()
    {
        yield return null;
    }

    protected IEnumerator YieldReturnException(Exception exception)
    {
        if (null == exception)
        {
            exception = new Exception("Test exception");
        }
        yield return exception;
    }

    protected IEnumerator YieldThrowException(Exception exception)
    {
        if (null == exception)
        {
            exception = new Exception("Test exception");
        }
        yield return null;
        throw exception;
    }

    protected IEnumerator YieldReturnObject(object returnValue)
    {
        yield return returnValue;
    }

    protected IEnumerator YieldReturnCustomInstruction(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
    }

    [UnityTest]
    public IEnumerator YieldReturnNull_WaitComplete_Success()
    {
        var routine = YieldReturnNull();
        var root = NestedRoutineRoot(routine);
        var op = new CustomRoutineOperation<object>(root);
        yield return op;
        Assert.IsNull(op.Error);
        Assert.IsTrue(op.IsComplete);
        Assert.IsNull(op.Result);
        Assert.AreEqual(OperationState.Success, op.State);
    }

    [UnityTest]
    public IEnumerator NestedYieldReturnBool_WaitComplete_ResultReturnValue()
    {
        bool returnValue = true;
        var routine = YieldReturnObject(returnValue);
        var root = NestedRoutineRoot(routine);
        var op = new CustomRoutineOperation<bool>(root);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }

    [UnityTest]
    public IEnumerator NestedYieldReturnInt_WaitComplete_ResultReturnValue()
    {
        int returnValue = 53;
        var routine = YieldReturnObject(returnValue);
        var root = NestedRoutineRoot(routine);
        var op = new CustomRoutineOperation<int>(root);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }

    [UnityTest]
    public IEnumerator NestedYieldReturnVector3_WaitComplete_ResultReturnValue()
    {
        Vector3 returnValue = Vector3.up;
        var routine = YieldReturnObject(returnValue);
        var root = NestedRoutineRoot(routine);
        var op = new CustomRoutineOperation<Vector3>(root);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }

    [UnityTest]
    public IEnumerator NestedYieldReturnObject_WaitComplete_ResultReturnValue()
    {
        object returnValue = new object();
        var routine = YieldReturnObject(returnValue);
        var root = NestedRoutineRoot(routine);
        var op = new CustomRoutineOperation<object>(root);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }

    [UnityTest]
    public IEnumerator NestedYieldReturnWrongType_WaitComplete_ResultNull()
    {
        bool returnValue = true;
        var routine = YieldReturnObject(returnValue);
        var root = NestedRoutineRoot(routine);
        var op = new CustomRoutineOperation<string>(root);
        yield return op;
        Assert.IsNull(op.Result);
    }

    [UnityTest]
    public IEnumerator YieldReturnException_WaitComplete_GetError()
    {
        var exc = new Exception("test exc");
        var routine = YieldReturnException(exc);
        var root = NestedRoutineRoot(routine);
        var op = new CustomRoutineOperation<object>(root);
        yield return op;
        Assert.IsNotNull(op.Error);
        Assert.IsTrue(op.IsComplete);
        Assert.IsNull(op.Result);
        Assert.AreEqual(OperationState.Failure, op.State);
        Assert.IsTrue(op.Error.Contains(exc.Message));
    }

    [UnityTest]
    public IEnumerator YieldThrowException_WaitComplete_GetError()
    {
        var exc = new Exception("test exc");
        var routine = YieldThrowException(exc);
        var root = NestedRoutineRoot(routine);
        var op = new CustomRoutineOperation<object>(root);
        yield return op;
        Assert.IsNotNull(op.Error);
        Assert.IsTrue(op.IsComplete);
        Assert.IsNull(op.Result);
        Assert.AreEqual(OperationState.Failure, op.State);
        Assert.IsTrue(op.Error.Contains(exc.Message));
    }

    [UnityTest]
    public IEnumerator NestedYieldReturnCustomInstructionWait_WaitComplete_GetDelay()
    {
        float delayTime = 1.0f;
        var routine = YieldReturnCustomInstruction(delayTime);
        var root = NestedRoutineRoot(routine);
        var startTime = Time.realtimeSinceStartup;
        var op = new CustomRoutineOperation<object>(root);
        yield return op;
        var endTime = Time.realtimeSinceStartup;
        Assert.GreaterOrEqual(endTime - startTime, delayTime);
    }
}
