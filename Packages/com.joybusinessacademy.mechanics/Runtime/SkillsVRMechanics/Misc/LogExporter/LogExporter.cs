using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.XR;
using System.Collections;
using System.Text.RegularExpressions;

public class LogExporter : MonoBehaviour
{
	[Serializable]
	public struct Logs
	{
		public string condition;
		public string stackTrace;
		public LogType type;

		public string dateTime;

		public Logs(string condition, string stackTrace, LogType type, string dateTime)
		{
			this.condition = condition;
			this.stackTrace = stackTrace;
			this.type = type;
			this.dateTime = dateTime;
		}
	}

	public List<Logs> logInfoList = new List<Logs>();

	private void OnEnable()
	{
		Application.logMessageReceived += LogCallback;
	}

	private void OnApplicationPause(bool pause)
	{
		if(pause)
			WriteDataToFile();
	}

	private void LogCallback(string condition, string stackTrace, LogType type)
	{
		Logs logInfo = new Logs(condition, stackTrace, type, DateTime.Now.ToString("g"));
		logInfoList.Add(logInfo);
	}

	private void OnDisable()
	{
		Application.logMessageReceived -= LogCallback;
	}

	private void CheckIfOldLogsNeedDeleting()
	{
		if(Directory.Exists(Application.persistentDataPath))
		{
			string [] logFiles = Directory.GetFiles(Application.persistentDataPath);

			foreach(string file in logFiles)
			{
				if (file.Contains("_log"))
				{
					FileInfo logFileInfo = new FileInfo(file);
					DateTime creationDate = logFileInfo.CreationTime;
					TimeSpan difference = DateTime.Now - creationDate;

					if(difference.TotalDays > 30)
						File.Delete(file);
				}

			}
		}
	}

	public void WriteDataToFile()
	{
		CheckIfOldLogsNeedDeleting();

		string fileTimeNaming = (DateTime.Now.ToString("G") + "_log").Replace(".","").Replace("/", "").Replace(":", "").Replace("-", "");
		string logFilePath = Application.persistentDataPath + "/" + fileTimeNaming + ".txt";

		StreamWriter writer = new StreamWriter(logFilePath, true);

		writer.WriteLine(GetVRHeadsetType() + "\n");
		writer.WriteLine("Package ID: " + Application.identifier + "\n\n\n");

		foreach (var item in logInfoList)
		{
			writer.WriteLine("Type: " + item.type.ToString() + " \nCondition: " + item.condition + " \nStack Trace: " + item.stackTrace + " \nTime Logged: " + item.dateTime + "\n");
		}

		writer.Close();
	}

	private string GetVRHeadsetType()
	{
		string loadedDeviceName = XRSettings.loadedDeviceName;

		if (!string.IsNullOrEmpty(loadedDeviceName))
		{
			if (loadedDeviceName.Contains("Oculus"))
			{
				return "Oculus VR Headset";
			}
			else if (loadedDeviceName.Contains("Pico"))
			{
				return "Pico VR Headset";
			}
		}

		return loadedDeviceName;
	}

}