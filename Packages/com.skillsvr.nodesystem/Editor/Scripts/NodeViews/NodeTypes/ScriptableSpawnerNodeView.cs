using System.Reflection;
using SkillsVR.Mechanic.Core;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using SkillsVRNodes.Managers;
using SkillsVRNodes.Scripts.Nodes;
using NodeView = UnityEditor.Experimental.GraphView.Node;
using Object = UnityEngine.Object;

namespace SkillsVRNodes.Editor.NodeViews
{
	public class ScriptableSpawnerNodeView<TSpawner, TInterface, TData> : SpawnerNodeView<TSpawner, TInterface, TData>
		where TSpawner : AbstractMechanicSpawner<TInterface, TData>
		where TInterface : IMechanicSystem<TData>
		where TData : ScriptableObject, new()
	{
		private const string scriptablePath = "Assets/Contexts/Scriptables";
		
		public ScriptableSpawnerNode<TSpawner, TInterface, TData> AttachedNode => nodeTarget as ScriptableSpawnerNode<TSpawner, TInterface, TData>;

		private Foldout foldout;
		private void UpdateDropdowns()
		{
			if (nodeTarget is not SpawnerNode<TSpawner, TInterface, TData> spawnerNode)
			{
				return;
			}
			
			foldout.SetEnabled(spawnerNode.MechanicData != null);

			if (spawnerNode.MechanicData == null)
			{
				foldout.value = false;
			}
			else
			{
				foldout.Clear();
				DrawInspectorElements(spawnerNode.MechanicData, foldout);
			}
		}

		public void CreateScriptableObject()
		{
			TData asset = ScriptableObjectManager.CreateScriptableObject<TData>(scriptablePath);
			
			if (nodeTarget is not SpawnerNode<TSpawner, TInterface, TData> spawnerNode)
			{
				return;
			}

			spawnerNode.MechanicData = asset;

			AssetDatabase.OpenAsset(asset);
			UpdateDropdowns();
		}
		
		/// <summary>
		/// Creates all the visual elements of the inputted object and adds them to the visual element
		/// </summary>
		/// <param name="objectToDraw">The object to generate it</param>
		/// <param name="visualElement">The visual element to attach it to </param>
		protected static void DrawInspectorElements(Object objectToDraw, VisualElement visualElement)
		{
			IEnumerable<FieldInfo> fields = objectToDraw.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			
			foreach (FieldInfo field in fields)
			{
				CreateField(objectToDraw, field.Name, visualElement);
			}
		}
		
		
		public static void CreateField(Object baseObject, string fieldName, VisualElement visualElement)
		{
			SerializedObject serializedObject = new SerializedObject(baseObject);
			
			SerializedProperty serializedProperty = serializedObject.FindProperty(fieldName);

			if (serializedProperty == null)
			{
				return;
			}

			PropertyField propertyField = new PropertyField();

			propertyField.BindProperty(serializedProperty);

			visualElement.Add(propertyField);
		}
	}
}