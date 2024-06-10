using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class AbstractMechanicSystem<T> : MechanicSystem
{
	public T mechanicData;

	internal bool isIntializaed = false;

	public bool storeTelemetryData = true;

	public UnityEvent onDataReadyEvent = new UnityEvent();

	public override void Start()
	{
		base.Start();
		if (!storeTelemetryData)
			return;
	}

	public override void Init(object data, GameObject spawner)
	{
		base.Init(data, spawner);

		if (data !=  null)
		{
			InitializeMechanic(data);
			isIntializaed = true;
		}
		else
		{
			Debug.LogError("DATA MISSING FOR MECHANIC - Unable to get data from spawned mechanic " + this.gameObject.name + " type: " + this.GetType().Name);
		}
	}

	private void InitializeMechanic(object rawData)
	{
		if (rawData != null)
			mechanicData = (T)rawData;

		OnMechanicDataReady();
		onDataReadyEvent?.Invoke();
	}

	protected virtual void OnMechanicDataReady()
	{
	}
}
