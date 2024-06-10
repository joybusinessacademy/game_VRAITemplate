using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.UIElements;

namespace Samples.Editor.General
{
	public class DragNewAssetPoint : PointerManipulator
	{
		Object droppedObject = null;

		GetNewObject onStartedDrag;
		System.Action<Object> onDrop;
		public DragNewAssetPoint(VisualElement root, GetNewObject onStartedDrag, System.Action<Object> _onDrop = null)
        {
            target = root;
            this.onStartedDrag = onStartedDrag;
			if(_onDrop != null)
				this.onDrop = _onDrop;
        }

         protected override void RegisterCallbacksOnTarget()
		{
			target.RegisterCallback<PointerDownEvent>(OnPointerDown);
			target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);

			if (!EditorWindow.HasOpenInstances<TimelineEditorWindow>())
			{
				return;
			}
			if (EditorWindow.GetWindow<TimelineEditorWindow>() == null)
			{
				return;
			}
			
			VisualElement element = EditorWindow.GetWindow<TimelineEditorWindow>()?.rootVisualElement;

			if (element?.parent[0] == null)
			{
				return;
			}
			element = element.parent[0];
			element?.RegisterCallback<DragPerformEvent>(OnDragPerform);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
			target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);

            VisualElement element = EditorWindow.GetWindow<TimelineEditorWindow>()?.rootVisualElement;

            if (element?.parent[0] == null)
            {
                return;
            }
            element = element.parent[0];
            element?.UnregisterCallback<DragPerformEvent>(OnDragPerform);
        }
		
		public delegate Object GetNewObject();
		
		void OnPointerDown(PointerDownEvent _)
		{
			droppedObject = onStartedDrag.Invoke();
			
			if (droppedObject == null)
			{
				return;
			}
			
			DragAndDrop.PrepareStartDrag();
			DragAndDrop.objectReferences = new[] { droppedObject };
			DragAndDrop.StartDrag(droppedObject.name);
		}

		void OnDragUpdate(DragUpdatedEvent _)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
		}
		
		void OnDragPerform(DragPerformEvent dropEvent)
		{
			if (DragAndDrop.objectReferences[0] == droppedObject)
			{
				
				AssetDatabase.AddObjectToAsset(droppedObject, TimelineEditor.masterAsset);

                if (onDrop != null)
                    onDrop.Invoke(DragAndDrop.objectReferences[0]);
            }
        }
	}
}