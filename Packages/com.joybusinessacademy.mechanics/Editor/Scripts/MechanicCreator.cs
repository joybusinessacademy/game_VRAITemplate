using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MechanicCreator : EditorWindow
{
    public string spawnerPath = "";
    public string newMechanicPath = "";

    public string mechanicName = "";

    [MenuItem("Window/Asset Management/SkillsVR/Mechanic/Create Mechanic")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(MechanicCreator));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("Mechanic Creator");
    }

    private void Awake()
    {
        spawnerPath = Application.dataPath + "/game_VRMechanicsPackage/Runtime/MechanicSpawner/Spawners/";
        newMechanicPath = Application.dataPath + "/game_VRMechanicsPackage/Runtime/Mechanics/";
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Mechanic Details", EditorStyles.boldLabel);
        mechanicName = EditorGUILayout.TextField("Mechanic Name", mechanicName);

        GUILayout.Space(25);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Spawner Path", EditorStyles.boldLabel);
        EditorGUILayout.TextField(spawnerPath);
        if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(60), GUILayout.Height(14)))
        {
            spawnerPath = EditorUtility.SaveFolderPanel("Spawner Path", spawnerPath, Application.dataPath);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Mechanic Path", EditorStyles.boldLabel);
        EditorGUILayout.TextField(newMechanicPath);
        if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(60), GUILayout.Height(14)))
        {
            newMechanicPath = EditorUtility.SaveFolderPanel("Mechanic Path", newMechanicPath, Application.dataPath);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(25);

        if (GUILayout.Button("GENERATE MECHANIC", GUILayout.ExpandWidth(true), GUILayout.Height(25)))
        {
            string mechanicFolderPath = newMechanicPath + mechanicName;
            string mechanicFolderScriptsPath = newMechanicPath + mechanicName + "/Scripts";
            Directory.CreateDirectory(mechanicFolderPath);
            Directory.CreateDirectory(mechanicFolderScriptsPath);

            GenerateMechanicClass(mechanicFolderScriptsPath);
            GenerateMechanicDataClass(mechanicFolderScriptsPath);
            GenerateMechanicSpawnerClass(spawnerPath);
        }
    }

    private void GenerateMechanicClass(string scriptPath)
    {
        string copyPath = scriptPath + "/" + mechanicName + ".cs";
        Debug.Log("Creating Classfile: " + copyPath);

        if (File.Exists(copyPath) == false)
        { // do not overwrite
            using (StreamWriter outfile =
                new StreamWriter(copyPath))
            {
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using System.Collections;");
                outfile.WriteLine("");
                outfile.WriteLine("public class " + mechanicName + " : AbstractMechanicSystem<" + mechanicName + "Data" + "> {");
                outfile.WriteLine(" ");
                outfile.WriteLine("}");
            }
        }
        AssetDatabase.Refresh();
    }

    private void GenerateMechanicDataClass(string scriptPath)
    {
        string copyPath = scriptPath + "/" + mechanicName + "Data" + ".cs";
        Debug.Log("Creating Classfile: " + copyPath);

        if (File.Exists(copyPath) == false)
        { // do not overwrite
            using (StreamWriter outfile =
                new StreamWriter(copyPath))
            {
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using System.Collections;");
                outfile.WriteLine("using System;");
                outfile.WriteLine(" ");
                outfile.WriteLine("[Serializable]");
                outfile.WriteLine("public class " + mechanicName + "Data" + " : MechanicData<" + mechanicName + "Data" + "> {");
                outfile.WriteLine("");
                outfile.WriteLine("}");
            }
        }
        AssetDatabase.Refresh();
    }

    private void GenerateMechanicSpawnerClass(string scriptPath)
    {
        string copyPath = scriptPath + "/" + "MechanicSpawner" + mechanicName  + ".cs";
        Debug.Log("Creating Classfile: " + copyPath);

        if (File.Exists(copyPath) == false)
        { // do not overwrite
            using (StreamWriter outfile =
                new StreamWriter(copyPath))
            {
                outfile.WriteLine("using UnityEngine;");
                outfile.WriteLine("using System.Collections;");
                outfile.WriteLine(" ");
                outfile.WriteLine("public class " + "MechanicSpawner" + mechanicName + " : AbstractMechanicSpawner<" + mechanicName + "Data" + "> {");
                outfile.WriteLine("");
                outfile.WriteLine("}");
            }
        }
        AssetDatabase.Refresh();
    }
}
