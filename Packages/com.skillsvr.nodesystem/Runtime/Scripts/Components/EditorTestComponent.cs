using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes
{
	/// <summary>
	/// When attached to an object, this component will be destroyed when the editor is not in play mode.
	/// </summary>
	[ExecuteInEditMode]
	public class EditorTestComponent : MonoBehaviour
	{
		public VisualElement attachedElement;
		
		public static void AddComponentToGameObject(GameObject gameObject, VisualElement attachedElement)
		{
			if (!Application.isEditor || Application.isPlaying)
			{
				return;
			}
			
			EditorTestComponent testComponent = gameObject.AddComponent<EditorTestComponent>();
			testComponent.attachedElement = attachedElement;
		}
		
		
		private void Update()
		{
			if (attachedElement == null)
			{
				DestroyImmediate(gameObject);
			} 
		}
	}
}