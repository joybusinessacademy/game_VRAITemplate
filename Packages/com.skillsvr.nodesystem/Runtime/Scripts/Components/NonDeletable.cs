using System;
using SkillsVRNodes;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SkillsVRNodes
{
	//[ExecuteInEditMode]
	public class NonDeletable : MonoBehaviour
	{
// 		[HideInInspector] public bool willDelete;
//
// 		private void Start()
// 		{
// #if UNITY_EDITOR
// 			willDelete = true;
// 			EditorSceneManager.sceneClosing += OnEditorSceneManagerOnSceneClosing;
// 			
// 			RefreshCallbacks();
//
// #endif
// 		}
//
// #if UNITY_EDITOR
// 		[InitializeOnLoadMethod]
// 		private static void RefreshCallbacks()
// 		{
// 			var nonDeletables = FindObjectsOfType<NonDeletable>();
// 			foreach (var nonDeletable in nonDeletables)
// 			{
// 				nonDeletable.Start();
//
// 				EditorApplication.wantsToQuit += () => nonDeletable.willDelete = false;
// 			}
// 		}
// #endif
//
//
// 		private void OnEditorSceneManagerOnSceneClosing(Scene scene, bool b)
// 		{
// #if UNITY_EDITOR
//
// 			if (scene == transform.gameObject.scene)
// 			{
// 				willDelete = false;
// 			}
// #endif
// 		}
//
// 		private void OnDestroy()
// 		{
// #if UNITY_EDITOR
// 			
// 			return;
// 			EditorSceneManager.sceneClosing -= OnEditorSceneManagerOnSceneClosing;
// 			
// 			if (Application.isPlaying)
// 			{
// 				return;
// 			}
//
//
// 			if (!willDelete)
// 			{
// 				return;
// 			}
// 			
// 			if (EditorUtility.DisplayDialog("Delete Object", "This object is not supposed to be deleted. Are you sure you want to delete it?", "Delete", "Keep"))
// 			{
// 				return;
// 			}
//
// 			EditorApplication.update += UndoDelete;
// #endif
// 		}
//
// 		private void UndoDelete()
// 		{
// #if UNITY_EDITOR
// 			Undo.PerformUndo();
// 			EditorApplication.update -= UndoDelete;
// #endif
// 		}
	}
}