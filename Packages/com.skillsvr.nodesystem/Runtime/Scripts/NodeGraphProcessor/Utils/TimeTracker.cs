using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillsVR.Utlis
{
	public class TimeTracker
	{
		public class TimeStamp
		{
			public int Id { get; private set; }
			public string Name { get; private set; }
			public DateTime Time { get; private set; }

			public TimeStamp(int id, string name, DateTime time)
			{
				Id = id;
				Name = string.IsNullOrWhiteSpace(name) ? "" : name;
				Time = null == time ? DateTime.Now : time;
			}

			public override string ToString()
			{
				return string.Format("{0} {1} {2}", Id, Time.ToString("HH:mm:ss.fff"), Name);
			}

			public static TimeSpan operator -(TimeStamp a, TimeStamp b)
			{
				return a.Time - b.Time;
			}
		}
		public string name;

		protected List<TimeStamp> ManagedStamps { get; private set; } = new List<TimeStamp>();

		public TimeSpan Duration => GetTotal();

		public int Count => ManagedStamps.Count;

		public TimeTracker(string customName = null)
		{
			name = string.IsNullOrWhiteSpace(customName) ? "" : customName;
			Reset();
		}

		public void Reset()
		{
			ManagedStamps.Clear();
			Stamp("Start");
		}

		public TimeStamp Stamp(string name = null)
		{
			int id = ManagedStamps.Count;
			var stamp = new TimeStamp(id, name, DateTime.Now);
			ManagedStamps.Add(stamp);
			return stamp;
		}

		TimeSpan GetTotal()
		{
			var first = ManagedStamps.FirstOrDefault();
			var last = ManagedStamps.LastOrDefault();

			if (null == first || null == last)
			{
				return new TimeSpan();
			}

			return last - first;
		}

		public TimeStamp GetStamp(int id)
		{
			if (id < 0 || id > ManagedStamps.Count - 1)
			{
				return null;
			}
			return ManagedStamps[id];
		}

		public TimeSpan GetStampTime(int id)
		{
			var first = ManagedStamps.FirstOrDefault();
			var curr = GetStamp(id);
			if (null == first || null == curr)
			{
				return new TimeSpan();
			}
			return curr - first;
		}

		public TimeSpan GetStampDiffTime(int id)
		{
			var prev = GetStamp(id - 1);
			var curr = GetStamp(id);
			if (null == prev || null == curr)
			{
				return new TimeSpan();
			}
			return curr - prev;
		}

		public override string ToString()
		{
			string info = "";
			info += string.Format("TimeTracker {0} ({1}) {2}ms in {3} steps", name, GetHashCode(), GetTotal().TotalMSString(), ManagedStamps.Count);

			info += "\r\n";

			foreach (var stamp in ManagedStamps)
			{
				info += string.Format("{0} diff: {1}\r\n", stamp, GetStampDiffTime(stamp.Id).TotalMSString());
			}
			return info;
		}
	}

	static class TimeSpanExt
	{
		public static string TotalMSString(this TimeSpan timeSpan)
		{
			return null == timeSpan ? "0" : timeSpan.TotalMilliseconds.ToString();
		}
	}
}

