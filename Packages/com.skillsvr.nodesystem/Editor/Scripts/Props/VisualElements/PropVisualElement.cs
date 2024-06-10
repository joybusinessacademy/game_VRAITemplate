using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Props;
using Samples.Editor.General;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Props
{
	public class PropVisualElement : VisualElement
	{
		private readonly PropComponent propComponent;

		private PropManagerVisualElement propManagerVisualElement;

		public PropVisualElement(string folder, Action onClick)
		{
			Add(new Image() { image = Resources.Load<Texture2D>("Icon/Folder") });
			Add(new Label(folder));
			RegisterCallback<ClickEvent>(evt => onClick());
		}
		
		public PropVisualElement(PropComponent propComponent, PropManagerVisualElement propManagerVisualElement)
		{
			this.propComponent = propComponent;
			this.propManagerVisualElement = propManagerVisualElement;
			string assetPath = AssetDatabase.GetAssetPath(propComponent);
			GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
			
			this.AddManipulator(new DragAssetPoint(this, propComponent.gameObject));
			
			
			Texture2D assetImage = AssetPreview.GetAssetPreview(asset);
			Transparent(assetImage);
			
			Image icon = new();
			if (assetImage == null)
			{
				EditorCoroutineUtility.StartCoroutine(LoadIcon(icon, asset), this);
				assetImage = AssetPreview.GetMiniThumbnail(asset);
			}
			icon.image = assetImage;
			
			Add(icon);
			
			string assetName = TidyAssetName(Path.GetFileNameWithoutExtension(assetPath));
			Add(new Label(assetName));
			tooltip = assetName;
		}

		private string TidyAssetName(string assetName)
		{
			assetName = assetName.Replace("_", " ");
			assetName = assetName.Replace("-", " ");
        
			assetName = ObjectNames.NicifyVariableName(assetName);
			assetName = assetName.Trim();
			return assetName;
		}

		private static void Transparent(Texture2D assetImage)
		{
			if (assetImage != null && assetImage.GetPixel(0, 0) != Color.clear)
			{
				Color[] array = assetImage.GetPixels();
				float f = .322f;
				float t = .001f;
				for (int i = 0; i < array.Length; i++)
				{
					if (Math.Abs(array[i].r - f) < t && Math.Abs(array[i].g - f) < t && Math.Abs(array[i].b - f) < t)
					{
						array[i] = Color.clear;
					}
				}

				assetImage.SetPixels(array);
				assetImage.Apply();
			}
		}


		IEnumerator LoadIcon(Image icon, GameObject asset)
		{
			const float period = 0.1f;
			float timer = 20;
			while (AssetPreview.GetAssetPreview(asset) == null)
			{
				if (timer < 0)
				{
					Debug.LogWarning("Could not load icon for " + propComponent.name, asset);
					yield break;
				}
				timer -= period;
				yield return new EditorWaitForSeconds(period);
			}
	
			icon.image =  AssetPreview.GetAssetPreview(asset);
		}
	}
}