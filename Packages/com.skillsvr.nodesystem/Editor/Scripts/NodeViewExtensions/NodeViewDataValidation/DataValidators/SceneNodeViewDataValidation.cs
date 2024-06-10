using SkillsVRNodes.Managers.Utility;
using System.Collections;
using System.Linq;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
    [CustomDataValidation(typeof(SceneNodeView))]
	public class SceneNodeViewDataValidation : AbstractNodeViewValidation<SceneNodeView>
	{
		public override void OnValidate()
		{
			var node = TargetNodeView.AttachedNode;

			string scenePathName = "ScenePath";
			string openSceneButtonName = "OpenSceneButton";
			string additiveScenePathPattern = "AdditiveScenePaths[{0}]";

			ErrorIf(InvalidPath(node.scenePath), scenePathName, "Scene path should be not none.\r\nSelect or create a scene.");
			ErrorIf(AssetNotExist(node.scenePath), scenePathName, GetSceneNotExistMsg(node.scenePath));

			string graphPath = GraphScenePathExtensions.GetDefaultGraphAssetPathFromScenePath(node.scenePath);
			ErrorIf(AssetNotExist(graphPath), scenePathName, GetNotExistMsg("Graph", graphPath));

			if (null != node.additiveScenePaths && node.additiveScenePaths.Count > 0)
			{
				int index = 0;
				foreach (var additiveScenePath in node.additiveScenePaths)
				{
					if (string.IsNullOrWhiteSpace(additiveScenePath))
					{
						continue;
					}
					string propPath = string.Format(additiveScenePathPattern, index);
					ErrorIf(AssetNotExist(additiveScenePath), propPath, GetSceneNotExistMsg(additiveScenePath, "Remove or select a new scene."));
					WarningIf(AssetNotExist(additiveScenePath), openSceneButtonName, GetSceneNotExistMsg(additiveScenePath, "Additive scene will not opened."));
					++index;
				}
			}
			ErrorIf(AssetNotExist(node.scenePath), openSceneButtonName, GetSceneNotExistMsg(node.scenePath, "No scene will be opened."));
		}

		public override VisualElement OnGetVisualSourceFromPath(string path)
		{
			switch (path)
			{
				case "ScenePath": return TargetNodeView.Q<DropdownField>();
				case "OpenSceneButton": return TargetNodeView.Q("open-scene-button");
				default: break;
			}

			if (path.StartsWith("AdditiveScenePaths["))
			{
				return TargetNodeView.Q("additive-scene");
			}
			return null;
		}
	}
}

