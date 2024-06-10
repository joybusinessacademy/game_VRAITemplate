using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace SkillsVRNodes
{
	[Obsolete("All Scene elements and hotspots are now Props. Use Prop instead of SceneElement.")]
	public abstract class SceneElement : MonoBehaviour
	{
		public virtual void Reset()
		{
			ResetName();
			SceneElementManager.Instance.allSceneElements.Add(this);
		}

		private void ResetName()
		{
			string firstname = transform.parent == null ? "Base" : transform.parent.name;
			string defaultName = firstname + "/" + gameObject.name;

			List<SceneElement> allSceneElements = SceneElementManager.Instance.allSceneElements;
			if (allSceneElements.Exists(element => element.elementName == defaultName && element != this))
			{
				int number = 1;
				while (allSceneElements.Exists(element => element.elementName == defaultName + number && element != this))
				{
					number++;
				}

				elementName = defaultName + number;
			}
			else
			{
				elementName = defaultName;
			}
		}

		[ReadOnly] public string elementName = "";
	}
}