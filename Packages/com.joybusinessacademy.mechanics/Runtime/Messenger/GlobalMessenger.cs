using System;
using UnityEngine;
/// <summary>
/// A singleton EventReceiver that can be used for global broadcasts.
/// </summary>
///
namespace SkillsVR.Messeneger
{
	public class GlobalMessenger : EventReceiver
	{
		private static GlobalMessenger instance;

		public static GlobalMessenger Instance {
			get {
				if (instance == null)
					instance = FindObjectOfType<GlobalMessenger>();

				return instance;
			}
		}

		private void Awake()
		{
			if (instance != null && instance != this)
			{
				DestroyImmediate(this);
				return;
			}

			instance = this;
		}

		private void OnDestroy()
		{
			if (instance == this)
				instance = null;
		}

	}
}