using System;
using System.Collections.Generic;

namespace SkillsVRNodes.Diagnostics
{
    [Serializable]
	public class SerlizedLogFileData
	{
		public string timeFormat = SerializedLogLine.TIME_FORMAT_STRING;
		public List<SerializedLogLine> logs = new List<SerializedLogLine>();
	}
}

