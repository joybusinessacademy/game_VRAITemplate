using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace SkillsVR.EventExtenstion
{
	[System.Serializable]
	public class UnityEventString : UnityEvent<String> { }

	[System.Serializable]
	public class UnityEventKeyValueString : UnityEvent<string, string> { }

	[System.Serializable]
	public class UnityEventInt : UnityEvent<int> { }
	[System.Serializable]
	public class UnityEventFloat : UnityEvent<float> { }
	[System.Serializable]
	public class UnityEventBool : UnityEvent<bool> { }
	[System.Serializable]
	public class UnityEventVector2 : UnityEvent<Vector2> { }
	[System.Serializable]
	public class UnityEventVector3 : UnityEvent<Vector3> { }
	[System.Serializable]
	public class UnityEventQuaternion : UnityEvent<Quaternion> { }
	[System.Serializable]
	public class UnityEventGameObject : UnityEvent<GameObject> { }
	[System.Serializable]
	public class UnityEventTransform : UnityEvent<Transform> { }
	[System.Serializable]
	public class UnityEventTexture : UnityEvent<Texture> { }
	[System.Serializable]
	public class UnityEventSprite : UnityEvent<Sprite> { }

}
