using UnityEngine.UI;

namespace SkillsVR.VideoPackage
{
	public static class FloatToTimeStringEx
	{
		public static string ToTimeString(this float time, bool includeDecimal = true)
		{
			int hours = (int)time / 3600;
			int mins = ((int)time / 60) % 60;
			int secs = ((int)time) % 60;
			int decimalAsInt = (int)((time - (int)time) * 100);

			string txt = string.Format("{0:00}:{1:00}", mins, secs);
			if (hours > 0)
			{
				txt = hours.ToString() + ":" + txt;
			}
			if (includeDecimal)
			{
				txt += string.Format(".{0:00}", decimalAsInt);
			}
			return txt;
		}
	}
}