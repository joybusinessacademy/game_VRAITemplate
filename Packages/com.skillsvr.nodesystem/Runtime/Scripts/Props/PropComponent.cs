using Props.PropInterfaces;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace Props
{
	[DisallowMultipleComponent]
	public class PropComponent : MonoBehaviour
	{
		public string propDescription = "New Prop Description";
		[HideInInspector] public DropPosition dropPosition = DropPosition.Floor;
		
		[Serializable]
		public enum DropPosition
		{
			Floor,
			Wall,
			Ceiling,
			GroundOnly,
			TableOnly,
			Any
		} 

		public List<string> propTags;
		
		[SerializeReference] public IBaseProp propType;
		//[SerializeReference] public IBaseProp propInterfaceType;
		public PropComponent()
		{
			//propInterfaceType = new DefaultProp(this);
			propType = new DefaultProp(this);
		}

		public string PropName
        {
          get => gameObject != null ? gameObject.name : "null";
            set
            {
				if (gameObject != null)
				{
					gameObject.name = value;
				}
			}
		}

        private void Start()
        {
			if ((propType as InteractableProp) != null)
			{
				InteractableProp ip = propType as InteractableProp;			
				ip.MuteInScene();
			}
			if ((propType as SocketProp) != null)
			{
				SocketProp sp = propType as SocketProp;
				sp.MuteInScene();
            }
		}

        public bool TrySetName(string newPropName)
		{
			return PropManager.TrySetPropName(this, newPropName);
		}
		
		public bool CanSetName(string newPropName)
		{
			return PropManager.CanSetName(newPropName);
		}

		private void Reset()
		{
			PropName = gameObject.name;
            
			propType ??= new DefaultProp(this);
			
			propType.AutoConfigProp();
		}

		private void OnDestroy()
		{
			if (!Application.isPlaying)
			{
				PropManager.PropListChanged();
			}
		}
		
		public void OnValidate()
		{
#if UNITY_EDITOR
			if (PrefabStageUtility.GetCurrentPrefabStage() != null)
			{
				return;
			}

			PropManager.AddElementEditor(this);
#endif
		}

		public void SetType(IBaseProp baseProp)
		{
			propType = baseProp;
		}
	}
}
