using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;
using UnityEditor;

namespace SkillsVRNodes.Managers.Utility
{
	public class IndexedData<TData>
	{
		public IndexedData(string path, TData indexedObject)
		{
			this.path = path;
			indexTime = GetAccessTimeOfFile();
			Data = indexedObject;
		}
		
		public void UpdateData(TData indexedObject)
		{
			indexTime = GetAccessTimeOfFile();
			Data = indexedObject;
		}

		public DateTime indexTime { get; private set; }
		public string path { get; }

		public TData Data;

		public bool isValidData => indexTime == GetAccessTimeOfFile();
		
		private DateTime GetAccessTimeOfFile() => new FileInfo(path).LastAccessTimeUtc;
	}
}