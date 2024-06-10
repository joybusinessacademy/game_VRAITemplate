using System.Linq;
using GraphProcessor;
using SkillsVRNodes.Scripts.Nodes;

namespace SkillsVRNodes.Editor.NodeViews
{
	public static class BaseNodeViewListItemPortExtensions
	{
		/// <summary>
		/// In case of some nodes contain choices with ports (i.e. MCQ and marker), 
		/// recent design (fixed 10 output ports in node) will cause an issue:
		/// when deleteing choices in list, edges in after ports will not connect to correct port but 1 port before because the node use fixed name as id.
		/// This method will update all edge's output field name (from OutputX to OutputX-1) and keep connection correctly.
		/// </summary>
		/// <param name="baseNodeView">the node view to be processed, which need to have a node target as IDynamicOutputPortCollection type</param>
		/// <param name="indexToRemove">The removed item/choice index</param>
		/// <returns>true if any edge data changed (which may need refresh graph view)</returns>
		public static bool ResetEdgeFieldNameBeforeRemoveDynamicPortItem(this BaseNodeView baseNodeView, int indexToRemove)
		{
			if (null == baseNodeView)
			{
				return false;
			}

			IDynamicOutputPortCollection dyanmicPortNode = baseNodeView.nodeTarget as IDynamicOutputPortCollection;
			if (null == dyanmicPortNode)
			{
				return false;
			}
			bool dirty = false;
			for (int index = indexToRemove + 1; index < 10; index++)
			{
				PortView portView = baseNodeView.GetPortViewsFromFieldName(dyanmicPortNode.GetOutputPortNameByIndex(index)).FirstOrDefault();
				if (null == portView)
				{
					continue;
				}
				foreach (var edge in portView.GetEdges().ToArray())
				{
					if (null == edge.serializedEdge)
					{
						continue;
					}
					string path = dyanmicPortNode.GetOutputPortNameByIndex(index - 1);
					edge.serializedEdge.outputFieldName = path;
					dirty = true;
				}
			}
			return dirty;
		}
	}
}
