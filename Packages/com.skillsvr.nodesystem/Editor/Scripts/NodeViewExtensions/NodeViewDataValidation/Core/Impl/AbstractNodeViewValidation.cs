using GraphProcessor;
using Props;
using Props.PropInterfaces;
using Scripts.VisualElements;
using SkillsVRNodes.Scripts.Nodes;
using System;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	public abstract class AbstractNodeViewValidation<T> : AbstractGraphElementDataValidation<T> where T : BaseNodeView
	{
        public T TargetNodeView => TargetVisual as T;

        public override string GetTargetId()
        {
            return TargetNodeView.nodeTarget.GUID;
        }

        [Obsolete]
        protected virtual SceneTransform CheckSceneSpawnPosition()
        {
            var spNode = TargetNodeView.nodeTarget as ISpawnerNode;
            string path = "SpawnPosition";
            if (null == spNode)
            {
                WarningIf(null == spNode, path, "Not suitable validation. Node is not a spanwer node type.");
                return null;
            }

			
			var sp = spNode.GetSpawnPosition();
            return Check1V1NamedSceneObjectBinding<SceneTransform>(sp, x => x.elementName == sp, path,
                "Position must be not null. Create or select a position.");
        }

        protected virtual void CheckSpawnPosition()
        {
            var spawnerNode = TargetNodeView.nodeTarget as ISpawnerNode;
			string preText = "SpawnPosition";
			if (spawnerNode == null)
            {
				WarningIf(null == spawnerNode, preText, "Not suitable validation. Node is not a spanwer node type.");
				return;
			}

			//PropGUID<IPropTransform> - Returns GUID
			var spawnPositionGUID = spawnerNode.GetSpawnPosition();
			ErrorIf(spawnPositionGUID == string.Empty, preText, "Prop GUID not found. - Make sure a prop is set");

            var prop = PropManager.GetProp<IPropPanel>(spawnPositionGUID);
            ErrorIf(prop == null, preText, "Prop not found in Scene Manager - Make sure the prop is set");
		}

		protected virtual void CheckTeleportPosition()
		{
			var spawnerNode = TargetNodeView.nodeTarget as ISpawnerNode;
			string preText = "SpawnPosition";
			if (spawnerNode == null)
			{
				WarningIf(null == spawnerNode, preText, "Not suitable validation. Node is not a spanwer node type.");
				return;
			}

			//PropGUID<IPropTransform> - Returns GUID
			var spawnPositionGUID = spawnerNode.GetSpawnPosition();
			ErrorIf(spawnPositionGUID == string.Empty, preText, "Prop GUID not found. - Make sure a prop is set");

			var prop = PropManager.GetProp<IPropPlayerPosition>(spawnPositionGUID);
			ErrorIf(prop == null, preText, "Prop not found in Scene Manager - Make sure the prop is set");
		}
	}
}