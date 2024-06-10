using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkillsVR.Mechanic.Events.Gen;
using UnityEditor;
using UnityEngine;

namespace SkillsVR.Mechanic.Events
{
	public class MechanicEventsGenerator
	{
		[MenuItem("Window/Asset Management/SkillsVR/Mechanic/CodeGenerator/MechanicEventsGenerator/Generator File")]
		static void SelectGeneratorFile()
		{
			var guid = AssetDatabase.FindAssets(nameof(MechanicEventsGenerator)).FirstOrDefault();
			string path = AssetDatabase.GUIDToAssetPath(guid);
			Debug.LogError(path);
			var item = AssetDatabase.LoadMainAssetAtPath(path);
			Selection.activeObject = item;
			AssetDatabase.Refresh();
		}


		[MenuItem("Window/Asset Management/SkillsVR/Mechanic/CodeGenerator/MechanicEventsGenerator/Generate")]
		public static void Gen()
		{
			string filePath = MechanicEventsFileMarker.GetFilePath();
			var lines = File.ReadAllLines(filePath);
			List<string> outputLines = new List<string>();
			bool copy = true;
			bool genEventClassMark = false;
			bool genListMark = false;
			bool genListNameMark = false;
			foreach(string line in lines)
			{
				if (null == line && copy)
				{
					outputLines.Add("");
					continue;
				}
				// Gen classes
				if (line.TrimStart(" ").TrimStart("	").StartsWith(MechanicEventsFileMarker.genAreaStartMark))
				{
					outputLines.Add(line);
					copy = false;
					genEventClassMark = true;
				}
				else if (line.TrimStart(" ").TrimStart("	").StartsWith(MechanicEventsFileMarker.genAreaEndMark))
				{
					copy = true;
				}
				// Gen list properties
				else if (line.TrimStart(" ").TrimStart("	").StartsWith(MechanicEventsFileMarker.genListStartMark))
				{
					outputLines.Add(line);
					copy = false;
					genListMark = true;
				}
				else if (line.TrimStart(" ").TrimStart("	").StartsWith(MechanicEventsFileMarker.genListEndMark))
				{
					copy = true;
				}

				// Gen list name
				if (line.TrimStart(" ").TrimStart("	").StartsWith(MechanicEventsFileMarker.genListNamesStartMark))
				{
					outputLines.Add(line);
					copy = false;
					genListNameMark = true;
				}
				else if (line.TrimStart(" ").TrimStart("	").StartsWith(MechanicEventsFileMarker.genListNamesEndMark))
				{
					copy = true;
				}

				if (copy)
				{
					outputLines.Add(line);
				}
				if (genEventClassMark)
				{
					genEventClassMark = false;
					foreach(var dataTypeName in MechanicEventsFileMarker.typeList)
					{
						string dataName = string.Concat(dataTypeName[0].ToString().ToUpper(), dataTypeName.Substring(1));
						string typeLine = MechanicEventsFileMarker.eventTemplate
							.Replace(MechanicEventsFileMarker.dataTypeTemplate, dataTypeName)
							.Replace(MechanicEventsFileMarker.dataNameTemplate, dataName);
						string typeListLine = MechanicEventsFileMarker.eventListTemplate
							.Replace(MechanicEventsFileMarker.dataTypeTemplate, dataTypeName)
							.Replace(MechanicEventsFileMarker.dataNameTemplate, dataName);
						outputLines.Add(typeLine);
					}
					outputLines.Add("");
					outputLines.Add("");
					foreach (var dataTypeName in MechanicEventsFileMarker.typeList)
					{
						string dataName = string.Concat(dataTypeName[0].ToString().ToUpper(), dataTypeName.Substring(1));
						string typeListLine = MechanicEventsFileMarker.eventListTemplate
							.Replace(MechanicEventsFileMarker.dataTypeTemplate, dataTypeName)
							.Replace(MechanicEventsFileMarker.dataNameTemplate, dataName);
						outputLines.Add(typeListLine);
					}
				}

				if (genListMark)
				{
					genListMark = false;
					foreach (var dataTypeName in MechanicEventsFileMarker.typeList)
					{
						string dataName = string.Concat(dataTypeName[0].ToString().ToUpper(), dataTypeName.Substring(1));
						string typeListLine = MechanicEventsFileMarker.eventListPropertyTemplate
							.Replace(MechanicEventsFileMarker.dataTypeTemplate, dataTypeName)
							.Replace(MechanicEventsFileMarker.dataNameTemplate, dataName);
						outputLines.Add(typeListLine);
					}
				}
				if (genListNameMark)
				{
					genListNameMark = false;
					foreach (var dataTypeName in MechanicEventsFileMarker.typeList)
					{
						string dataName = string.Concat(dataTypeName[0].ToString().ToUpper(), dataTypeName.Substring(1));
						string typeListLine =  MechanicEventsFileMarker.listNameTemplate
							.Replace(MechanicEventsFileMarker.dataTypeTemplate, dataTypeName)
							.Replace(MechanicEventsFileMarker.dataNameTemplate, dataName);
						outputLines.Add(typeListLine);
					}
				}
			}

			File.WriteAllLines(filePath, outputLines.ToArray());
		}
	}
}



