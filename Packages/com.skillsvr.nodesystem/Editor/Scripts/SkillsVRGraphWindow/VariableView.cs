using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using GraphProcessor;
using SkillsVRNodes.Managers;
using SkillsVRNodes.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace SkillsVRNodes.Editor.Graph
{

    
    public class VariableView : PinnedElementView
    {
	    BaseGraphView           graphView;
    
        public VariableView() => title = "Variable view";
    
        protected override void Initialize(BaseGraphView graphView)
        {
	        this.graphView = graphView;
	        // Add exposed parameter button
	        header.Add(new Button(() =>
	        {
		        OnAddClicked();
		        ResetWindow();
	        })
	        {
		        text = "+"
	        });

	        InitializeElements();
        }

        public void ResetWindow()
        {
	        content.Clear();
	        ScriptableObjectManager.RefreshList<FloatSO>();
	        ScriptableObjectManager.RefreshList<IntSO>();
	        InitializeElements();
        }
        private void InitializeElements()
        {
	        AddVariables();
            

        }

        protected void AddVariables()
        {
	        foreach (FloatSO scriptableObject in ScriptableObjectManager.GetAllInstances<FloatSO>())
	        {
		        CreateFloatVariable(scriptableObject);
	        }
	        foreach (IntSO scriptableObject in ScriptableObjectManager.GetAllInstances<IntSO>())
	        {
		        CreateFloatVariable(scriptableObject);
	        }
        }

        protected void CreateFloatVariable(ScriptableObject scriptableObject)
        {
	        VisualElement floatVariable = new()
	        {
		        name = scriptableObject.name + "Variable",
		        style =
		        {
			        flexDirection = new StyleEnum<FlexDirection> { value = FlexDirection.Row }
		        }
	        };
	        floatVariable.AddToClassList("VariableView");
	        Label heading = new()
	        {
		        text = scriptableObject.name,
		        style = { width = 75}
	        };
	        floatVariable.Add(heading);

	        Button showInProject = new Button();
	        showInProject.clicked += () => EditorGUIUtility.PingObject(scriptableObject);
	        showInProject.text = "showInProject";
	        floatVariable.Add(showInProject);
	        
	        
	        content.Add(floatVariable);
        }
        
        public static void OnAddClicked()
        {
	        GenericMenu parameterType = new GenericMenu();

	        parameterType.AddItem(new GUIContent("Float"), false, () =>
	        {
		        ScriptableObjectManager.CreateScriptableObject<FloatSO>();
	        });
	        
	        parameterType.AddItem(new GUIContent("Int"), false, () =>
	        {
		        ScriptableObjectManager.CreateScriptableObject<IntSO>();
	        });

	        parameterType.ShowAsContext();
        }
        
        protected virtual IEnumerable< Type > GetExposedParameterTypes()
        {
	        return TypeCache.GetTypesDerivedFrom<VariableSO>().Where(type => !type.IsGenericType);
        }
    
        protected string GetNiceNameFromType(Type type)
        {
	        string name = type.Name;

	        // Remove parameter in the name of the type if it exists
	        name = name.Replace("Parameter", "");

	        return ObjectNames.NicifyVariableName(name);
        }
    }

}