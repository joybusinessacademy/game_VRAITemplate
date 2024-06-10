using System.Collections;
using System.Runtime.CompilerServices;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Scripts.CustomWindows
{
    public class AssetPreviewItem : VisualElement
    {
        public AssetPreviewItem(Object item)
        {
            this.style.marginBottom = 5;
            this.style.marginTop = 5;
            this.style.marginLeft = 5;
            this.style.marginRight = 5;
            this.style.width = 100;
            this.style.height = 100;
            style.alignContent = Align.Center;
            
            SetItem(item);
        }

        public void SetItem(Object item)
        {
            Clear();
            tooltip = item.name;

            var image = AssetPreview.GetAssetPreview(item);
            if (image)
            {
                Add(new Image() { image = AssetPreview.GetAssetPreview(item), scaleMode = ScaleMode.ScaleToFit});
                return;
            }
            
            if (!AssetPreview.IsLoadingAssetPreview(item.GetInstanceID()))
            {
                var text = new Label("No Preview Available")
                {
                    pickingMode = PickingMode.Ignore,
                    style =
                    {
                        unityTextAlign = TextAnchor.MiddleCenter,
                        flexWrap = Wrap.Wrap,
                        whiteSpace = WhiteSpace.Normal,
                    }
                };
                Add(text);
                return;
            }
            
            Add(new Image() { image = AssetPreview.GetMiniThumbnail(item), scaleMode = ScaleMode.ScaleToFit});
            EditorCoroutineUtility.StartCoroutineOwnerless(WaitForPreview(item));
        }

        private IEnumerator WaitForPreview(Object item)
        {
            if (AssetPreview.IsLoadingAssetPreview(item.GetInstanceID()))
            {
                yield break;
            }
            
            Clear();
            Add(new Image() { image = AssetPreview.GetAssetPreview(item), scaleMode = ScaleMode.ScaleToFit});
        }
    }
}