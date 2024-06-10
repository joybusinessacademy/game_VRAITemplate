using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public static class SetUpProjectData
{
    internal const string FIRST_TIME = "FIRST_TIME_PROJECTDATA";

    static SetUpProjectData()
    {
        EditorApplication.update += RunOnce;
        EditorApplication.quitting += Quit;
    }

    private static void Quit() => EditorPrefs.DeleteKey(FIRST_TIME);
    private static void RunOnce()
    {
        var firstTime = EditorPrefs.GetBool(FIRST_TIME, true);
        if (firstTime)
        {
            EditorPrefs.SetBool(FIRST_TIME, false);

            ProjectController projectController = new ProjectController();
            projectController.CheckAllProjectData();
        }
        EditorApplication.update -= RunOnce;

    }
}
