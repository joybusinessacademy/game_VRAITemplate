using System;

namespace SkillsVRNodes.Diagnostics
{
    [Serializable]
	public class SerializedLogLine
	{
		public const string TIME_FORMAT_STRING = "yyyy/MM/dd HH:mm:ss";

        public string utc;
		public string lv;
		public string tags;
        public string msg;

		public void SetUTC(DateTime utc)
		{
			this.utc = utc.ToString(TIME_FORMAT_STRING);
		}
    }
}

