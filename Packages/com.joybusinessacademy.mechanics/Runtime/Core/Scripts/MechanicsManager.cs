using UnityEngine;
using System;
using System.Collections;
using SkillsVR.UnityExtenstion;

[DefaultExecutionOrder(-1)]
public class MechanicsManager : MonoBehaviour
{
	public VisualPaletteScriptable visualPaletteScriptable;

	internal bool pulledCatalogFromServer = false;

	private TelemetryController telemetryController;

	private MechanicsManager() { }
	private static MechanicsManager instance = null;

	public static MechanicsManager Instance {
		get { return instance; }
	}

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
			return;
		}

		instance = this;

		if (telemetryController == null)
			this.transform.GetOrAddComponent<TelemetryController>();

		Application.targetFrameRate = 90;
		QualitySettings.vSyncCount = 0;

	}

	private IEnumerator CheckInternetConnection(Action<bool> action)
	{
		yield return new WaitForSeconds(1);

		WWW www = new WWW("http://google.com");
		yield return www;
		if (www.error != null)
		{
			action(false);
		}
		else
		{
			action(true);
		}
	}
}
