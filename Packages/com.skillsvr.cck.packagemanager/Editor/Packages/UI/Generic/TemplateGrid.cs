using UnityEngine.UIElements;

namespace SkillsVR.CCK.PackageManager.UI.Views
{
    public class TemplateGrid: TemplateListView
    {
        public new class UxmlFactory : UxmlFactory<TemplateGrid, UxmlTraits> { }

        public TemplateGrid()
        {
            contentContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.style.flexWrap = Wrap.Wrap;
        }

        protected override VisualElement CreateItemView()
        {
            var item = base.CreateItemView();
            item.style.width = 100;
            return item;
        }
    }
}