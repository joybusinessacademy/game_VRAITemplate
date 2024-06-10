using NUnit.Framework;
using SkillsVR.CCK.PackageManager.AsyncOperation;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
public class CustomRoutineOperationTests
{
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

    protected class CustomErrorOp : CustomAsyncOperation<string>
    {
        public string ErrorStr { get; protected set; }
        public CustomErrorOp(string error)
        {
            ErrorStr = string.IsNullOrWhiteSpace(error) ? "Unknown Error" : error;
            
        }

        protected override IEnumerator OnProcessRoutine()
        {
            yield return null;
            SetError(ErrorStr);
        }
    }

    [UnityTest]
    public IEnumerator YieldReturnNull_WaitComplete_Success()
    {
        var routine = YieldReturnNull();
        var op = new CustomRoutineOperation<object>(routine);
        yield return op;
        Assert.IsNull(op.Error);
        Assert.IsTrue(op.IsComplete);
        Assert.IsNull(op.Result);
        Assert.AreEqual(OperationState.Success, op.State);
    }

    [UnityTest]
    public IEnumerator YieldReturnBool_WaitComplete_ResultReturnValue()
    {
        bool returnValue = true;
        var routine = YieldReturnObject(returnValue);
        var op = new CustomRoutineOperation<bool>(routine);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }

    [UnityTest]
    public IEnumerator YieldReturnNull_SetCustomError_GetError()
    {
        bool returnValue = true;
        var routine = YieldReturnObject(returnValue);
        var op = new CustomRoutineOperation<bool>(routine);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }

    [UnityTest]
    public IEnumerator YieldReturnInt_WaitComplete_ResultReturnValue()
    {
        int returnValue = 53;
        var routine = YieldReturnObject(returnValue);
        var op = new CustomRoutineOperation<int>(routine);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }

    [UnityTest]
    public IEnumerator YieldReturnString_WaitComplete_ResultReturnValue()
    {
        string returnValue = "ok";
        var routine = YieldReturnObject(returnValue);
        var op = new CustomRoutineOperation<string>(routine);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }
    [UnityTest]
    public IEnumerator YieldReturnVector3_WaitComplete_ResultReturnValue()
    {
        Vector3 returnValue = Vector3.up;
        var routine = YieldReturnObject(returnValue);
        var op = new CustomRoutineOperation<Vector3>(routine);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }

    [UnityTest]
    public IEnumerator YieldReturnObject_WaitComplete_ResultReturnValue()
    {
        object returnValue = new object();
        var routine = YieldReturnObject(returnValue);
        var op = new CustomRoutineOperation<object>(routine);
        yield return op;
        Assert.AreEqual(returnValue, op.Result);
    }
    [UnityTest]
    public IEnumerator YieldReturnWrongType_WaitComplete_GetNull()
    {
        int returnValue = 53;
        var routine = YieldReturnObject(returnValue);
        var op = new CustomRoutineOperation<string>(routine);
        yield return op;
        Assert.IsNull(op.Result);
    }

    [UnityTest]
    public IEnumerator YieldReturnException_WaitComplete_GetError()
    {
        var exc = new Exception("test exc");
        var routine = YieldReturnException(exc);
        var op = new CustomRoutineOperation<object>(routine);
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
        var op = new CustomRoutineOperation<object>(routine);
        yield return op;
        Assert.IsNotNull(op.Error);
        Assert.IsTrue(op.IsComplete);
        Assert.IsNull(op.Result);
        Assert.AreEqual(OperationState.Failure, op.State);
        Assert.IsTrue(op.Error.Contains(exc.Message));
    }

    [UnityTest]
    public IEnumerator YieldReturnCustomInstructionWait_WaitComplete_GetDelay()
    {
        float delayTime = 1.0f;
        var routine = YieldReturnCustomInstruction(delayTime);
        var startTime = Time.realtimeSinceStartup;
        var op = new CustomRoutineOperation<object>(routine);
        yield return op;
        var endTime = Time.realtimeSinceStartup;
        Assert.GreaterOrEqual(endTime - startTime, delayTime);
    }

    [UnityTest]
    public IEnumerator NullCustomRoutine_WaitComplete_GetError()
    {
        IEnumerator routine = null;
        var op = new CustomRoutineOperation<object>(routine);
        yield return op;
        Assert.IsNotNull(op.Error);
        Assert.AreEqual(OperationState.Failure, op.State);
        Assert.IsTrue(op.IsComplete);
    }

    [UnityTest]
    public IEnumerator InternalSetError_WaitComplete_GetError()
    {
        string error = "Test error";
        var op = new CustomErrorOp(error);
        yield return op;
        Assert.IsNotNull(op.Error);
        Assert.AreEqual(OperationState.Failure, op.State);
        Assert.IsTrue(op.IsComplete);
        Assert.IsTrue(op.Error.Contains(error));
    }
}
