using GraphProcessor;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
    [CustomDataValidation(typeof(FinishNode))]
    public class FinishNodeViewValidation : AbstractNodeViewValidation<BaseNodeView>
    {
        public override VisualElement OnGetVisualSourceFromPath(string path)
        {
            return null;
        }

        public override void OnValidate()
        {

        }
    }
}
