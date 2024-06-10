using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class MechanicSystem : MonoBehaviour
{
	internal object dataPulled;
	internal GameObject spawner;

	internal Component spawnerComponent;
	private Action<string, object[]> spawnerMessageCallback;

	internal bool isLinked = false;
	internal MechanicSystem linkedPartner = null;

	public delegate void OnMechanicStarted();
	public OnMechanicStarted onMechanicStarted;

	public delegate void OnMechanicFinished();
	public OnMechanicFinished onMechanicFinished;

	public virtual void Init(object data ,GameObject mechanicSpawner)
	{
		dataPulled = data;
		spawner = mechanicSpawner;
	}

	public virtual void Start()
	{
	}

	public virtual void OnDestroy()
	{
	}

	public virtual void StartMechanic()
	{
		onMechanicStarted?.Invoke();
	}

	public virtual void FinishedMechanic()
	{
		onMechanicFinished?.Invoke();

		if(isLinked && linkedPartner != null)
			linkedPartner.StartMechanic();
	}

	public virtual void OnException(Exception e)
	{
		spawnerMessageCallback?.Invoke(nameof(OnException), new object[] { e });
	}


	#region Messages
	public object InvokeDyanmicMethod(string message, params object[] args)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			return null;
		}
		switch (message)
		{
			case nameof(SetSpawnerCallback): SetSpawnerCallback((Action<string, object[]>)args[0]); return null;
			case nameof(SetSpawnerComponent): SetSpawnerComponent((Component)args[0]); return null;
			case nameof(Init): Init((object)args[0], (GameObject)args[1]); return null;
			default:
				return OnReceiveMechanicSystemMessage(message, args);
		}
	}
	
	protected void SendMessageToSpawner(string message, params object[] customParams)
	{
		spawnerMessageCallback.Invoke(message, customParams);
	}

	protected virtual object OnReceiveMechanicSystemMessage(string message, object[] args)
	{
		return null;
	}

	protected void ThrowMessageNotMatchException(string message, object[] args)
	{
		throw new Exception(
					string.Join(" ",
					this.GetType().Name, "not contains method", message, "(",
					null == args ? "" : string.Join(", ", args.Select(x => x.GetType().Name)),
					")"
					));
	}

	private void SetSpawnerCallback(Action<string, object[]> callback)
	{
		spawnerMessageCallback = callback;
	}

	private void SetSpawnerComponent(Component spawner)
	{
		spawnerComponent = spawner;
	}
	#endregion Messages
}
