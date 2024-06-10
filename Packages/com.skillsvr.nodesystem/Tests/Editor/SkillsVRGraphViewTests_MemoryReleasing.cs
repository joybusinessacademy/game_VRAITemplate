using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using NUnit.Framework;
using SkillsVRNodes.Editor.Graph;
using SkillsVRNodes.Managers.Utility;
using UnityEngine;
using UnityEngine.TestTools;



public class CountableSkillsVRGraphView : SkillsVRGraphView
{
    public static int InstanceCount { get; private set; }

    public delegate void OnInstnaceCountChanged(int newValue);
    public static OnInstnaceCountChanged OnInstanceCountChangedEvent;


    public CountableSkillsVRGraphView(BaseGraphWindow window) : base(window)
    {
        ++InstanceCount;
        Debug.Log("++InstanceCount " + InstanceCount);
        OnInstanceCountChangedEvent?.Invoke(InstanceCount);
    }

    ~CountableSkillsVRGraphView()
    {
        --InstanceCount;
        Debug.Log("--InstanceCount " + InstanceCount);
        OnInstanceCountChangedEvent?.Invoke(InstanceCount);
    }
}

public class CountableSkillsVRGraphWindow : SkillsVRGraphWindow
{
    protected override BaseGraphView CreateNewGraphViewInstance()
    {
        return new CountableSkillsVRGraphView(this); ;
    }
}


public class SkillsVRGraphViewTests
{

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator GraphViewDeadlockReferenced_RepeatOpenGraphWindow_InstanceCountLessThan3()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;

        ExecGC();
        int instanceCountBefore = CountableSkillsVRGraphView.InstanceCount;
        Debug.Log("Base View Instance Start Count: " + instanceCountBefore);
        int testCount = 10;
        for (int i = 0; i < testCount; i++)
        {
            yield return null;
            var window = CountableSkillsVRGraphWindow.GetWindow<CountableSkillsVRGraphWindow>();
            window.LoadGraph(GraphFinder.CurrentGraph);
            yield return null;
            yield return null;

            float time = Time.realtimeSinceStartup;

            while (Time.realtimeSinceStartup - time < 0.5)
            {
                yield return null;
            }

            window.Close();
            yield return null;
            yield return null;

            if (i % 5 == 0)
            {
                ExecGC();
            }
        }



        float t = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - t < 2)
        {
            yield return null;
        }
        ExecGC();


        int instanceCountAfter = CountableSkillsVRGraphView.InstanceCount;
        int instanceCountDiff = instanceCountAfter - instanceCountBefore;
        Debug.Log("Base View Instance Count: " + instanceCountDiff);
        Assert.IsTrue(3 >= instanceCountDiff);
        Debug.Log("Done");
    }


    public static void ExecGC()
    {
        GC.Collect(); // Manually trigger the Garbage Collector
                      // Wait for the GC to complete
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
