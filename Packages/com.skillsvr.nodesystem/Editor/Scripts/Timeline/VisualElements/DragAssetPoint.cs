using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Samples.Editor.General
{
	public class DragAssetPoint : PointerManipulator
	{
		
		string assetPath = string.Empty;
		Object droppedObject = null;

		
		public DragAssetPoint(VisualElement root, Object dragObject)
		{
			target = root;
			droppedObject = dragObject;
		}

		protected override void RegisterCallbacksOnTarget()
		{
			// Register a callback when the user presses the pointer down.
			target.RegisterCallback<PointerDownEvent>(OnPointerDown);
			// Register callbacks for various stages in the drag process.
			target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
			target.RegisterCallback<DragPerformEvent>(OnDragPerform);
		}

		protected override void UnregisterCallbacksFromTarget()
		{
			// Unregister all callbacks that you registered in RegisterCallbacksOnTarget().
			target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
			target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
			target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
		}
		
		
		
		
				// This method runs when a user presses a pointer down on the drop area.
		void OnPointerDown(PointerDownEvent _)
		{
			// Only do something if the window currently has a reference to an asset object.
			if (droppedObject == null)
			{
				return;
			}
            
			// Clear existing data in DragAndDrop class.
			DragAndDrop.PrepareStartDrag();

			// Store reference to object and path to object in DragAndDrop static fields.
			DragAndDrop.objectReferences = new[] { droppedObject };
			DragAndDrop.paths = assetPath != string.Empty ? new[] { assetPath } : new string[] { };

			// Start a drag.
			DragAndDrop.StartDrag(string.Empty);
		}

		// This method runs every frame while a drag is in progress.
		void OnDragUpdate(DragUpdatedEvent _)
		{
			DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
		}

		// This method runs when a user drops a dragged object onto the target.
		void OnDragPerform(DragPerformEvent _)
		{
			// Set droppedObject and draggedName fields to refer to dragged object.
			droppedObject = DragAndDrop.objectReferences[0];
			string draggedName;
			if (assetPath != string.Empty)
			{
				string[] splitPath = assetPath.Split('/');
				draggedName = splitPath[^1];
			}
			else
			{
				draggedName = droppedObject.name;
			}
		}
	}
}