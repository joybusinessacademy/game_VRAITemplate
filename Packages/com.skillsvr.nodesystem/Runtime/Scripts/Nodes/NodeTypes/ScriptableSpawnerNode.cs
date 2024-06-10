using GraphProcessor;
using SkillsVR.Mechanic.Core;
using System;
using UnityEngine;

namespace SkillsVRNodes.Scripts.Nodes
{
	public class ScriptableSpawnerNode<TSpawner, TInterface, TData> : SpawnerNode<TSpawner, TInterface, TData>, ISaveHandler
		where TSpawner : AbstractMechanicSpawner<TInterface, TData>
		where TInterface : IMechanicSystem<TData>
		where TData : ScriptableObject, new()
	{
		public override void OnNodeCreated()
		{
			if (mechanicDataJSON.IsNullOrEmpty())
			{
				mechanicDataJSON = JsonUtility.ToJson(ScriptableObject.CreateInstance<TData>());
			}

			base.OnNodeCreated();
		}

		/// <summary>
		/// The runtime mechanic data instance that not serialized.
		/// This is generated from mechanicDataJSON when loading, and save as json when saving.
		/// </summary>
		[NonSerialized]
		public TData mechanicDataCache;
		
		public override TData MechanicData
		{
			get => mechanicDataCache == null ? Deserialize() : mechanicDataCache;
			set => Serialize(value);
		}

		[SerializeField] private string mechanicDataJSON;

		public void OnBeforeSave()
		{
			if (mechanicDataCache != null)
			{
				mechanicDataJSON = JsonUtility.ToJson(mechanicDataCache);
			}
		}

		void Serialize(TData data)
		{
			mechanicDataCache = data;
			mechanicDataJSON = JsonUtility.ToJson(data);
		}
		
		TData Deserialize()
		{
			mechanicDataCache = ScriptableObject.CreateInstance<TData>();
			
			if (!string.IsNullOrEmpty(mechanicDataJSON))
			{
				JsonUtility.FromJsonOverwrite(mechanicDataJSON, mechanicDataCache);
			}

			return mechanicDataCache;
		}
	}
}