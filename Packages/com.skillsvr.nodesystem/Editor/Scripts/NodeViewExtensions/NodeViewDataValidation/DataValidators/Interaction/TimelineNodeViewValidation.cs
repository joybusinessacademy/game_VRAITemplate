using GraphProcessor;
using Scripts.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using VisualElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(TimelineNodeView))]
	public class TimelineNodeViewValidation : AbstractNodeViewValidation<TimelineNodeView>
	{
		
		public override void OnValidate()
		{
			return;
			var node = TargetNodeView.AttachedNode<TimelineNode>();

			string directorName = "Director";
			string timelineName = "Timeline";
			string bindingName = "Binding";

			ErrorIf(node.director == null, directorName, "Director cannot be null. Select or create a director.");

			bool invalidTimeline = ErrorIf(IsInvalidAsset(node.timeline), timelineName, "Timeline cannot be null. Select or create a timeline.");
			ErrorIf(IsMissingAsset(node.timeline), timelineName, "Timeline asset is already removed. Select or create a timeline.");

			// if (!invalidTimeline && null != node.bindings && 0 < node.bindings.Count)
			// {
			// 	int index = 0;
   //              foreach (var binding in node.bindings)
   //              {
			// 		index++;
			// 		
   //
			// 		string path = bindingName + index.ToString() + " - " + binding.track.name;
   //
			// 		
   //                  if (IsInvalidName(binding.bindTarget))
			// 		{
   //                      var output = binding.track.outputs.FirstOrDefault();
			// 			// Audio source binding accept null binding, so no error if is null.
   //                      if (typeof(AudioSource) == output.outputTargetType)
   //                      {
   //                          continue;
   //                      }
   //                  }
   //
   //                  Check1V1NamedSceneObjectBinding<SceneAnimation>(
   //                          binding.bindTarget,
   //                          x => x.elementName == binding.bindTarget,
   //                          path,
   //                          "Binding is required, create or select a binding target.");
   //              }
			// }
			WarningIf(invalidTimeline, bindingName, "Need timeline to bind.");
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "Director": return TargetNodeView.QueryInChain<SceneElementDropdown<SceneTimeline>, DropdownField>();
				case "Timeline": return TargetNodeView.QueryInChain<ScriptableObjectDropdown<TimelineAsset>, DropdownField>();
				default: break;
			}

			if (path.StartsWith("Binding"))
			{
				int index = ExtractIndexFromBindingPath(path);
				if (0 > index)
				{
					return null;
				}

				return TargetNodeView.Query<SceneElementDropdown<SceneAnimation>>().AtIndex(index - 1);
			}

			return null;
		}

        private int ExtractIndexFromBindingPath(string input)
        {
            string pattern = @"Binding(\d+) -";
            Match match = Regex.Match(input, pattern);
            if (match.Success)
            {
                string numberStr = match.Groups[1].Value;
                if (int.TryParse(numberStr, out int number))
                {
                    return number;
                }
            }
            return -1;
        }
    }
}
