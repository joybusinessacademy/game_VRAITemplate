using UnityEngine;
using System;
using System.Diagnostics;

namespace SkillsVRNodes.Diagnostics
{
    public class CCKLogEntry
	{
		public DateTime timeUTC;
		public LogType logType;
		public string[] tags;
		public object message;
		public StackTrace stackTrace;
		public string stackTraceString;
		public object context;
    }
}

