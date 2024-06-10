using Scripts.VisualElements;
using System;
using System.CodeDom;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.Graphs;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillsVRNodes.Editor.NodeViews.Validation.Impl
{
	[CustomDataValidation(typeof(RankOrderNodeView))]
	public class RankOrderNodeViewValidation : AbstractNodeViewValidation<RankOrderNodeView>
	{
        public override void OnValidate()
        {
            string slotListKey = "Slots List";
            string slotItemKey = "Slot";
            string itemListKey = "Items List";
            string itemKey = "Item";

            var node = TargetNodeView.AttachedNode;
            var data = node.MechanicData;

			CheckSpawnPosition();

            int currSlotCount = null == data.rankOrderSlots ? 0 : data.rankOrderSlots.Count;
            int currItemCount = null == data.rankOrderItems ? 0 : data.rankOrderItems.Count;

            int minItemCount = Mathf.Max(1, currSlotCount);

            ErrorIf(0 >= currSlotCount, slotListKey, 
                "Should have at least 1 slot. Click the + button to add new slot.");
            ErrorIf(0 >= currItemCount, itemListKey,
                "Sould have at least " + minItemCount + " item" + (minItemCount > 1 ? "s" : "") + ". Click the + button to add new item.");

            ErrorIf(0 < currItemCount && currItemCount < currSlotCount, itemListKey,
                "Item count must equal or larger than slot counts. Min item count: " + minItemCount + ". Current: " + currItemCount);

            ErrorIf(node.currentSelectedRankType == 0 && currItemCount > currSlotCount, slotItemKey, "You must have enough slots for all items with this preset");

            int index = 0;
            foreach(var slot in data.rankOrderSlots)
            {
                ++index;
                string key = slotItemKey + index;
                WarningIf("Slot Text" == slot.title, key + ".Title", "Default slot text in use. Type your own one.");
                ErrorIf(slot.hasCustomTitle && string.IsNullOrWhiteSpace(slot.customTitle), key + ".CustomTitle", "Custom title cannot be emtpy or white spaces.");
                WarningIf(slot.hasCustomTitle && "Custom Title Text" == slot.customTitle, key + ".CustomTitle", "Default custom title in use. Type your own one.");

				//Uncomment if wanting slotonsameID to have full unique Slot ID and Item ID
				//if (data.slotOnSameID)
				//{
				//    ErrorIf(data.rankOrderSlots.Any(x => null != x && slot != x && slot.rankSlotID == x.rankSlotID), key + ".ID", 
				//        "Duplicated slot id in use. Change to an unique id.");
				//}
			}

			index = 0;
            foreach (var item in data.rankOrderItems)
            {
                ++index;
                string key = itemKey + index;
                WarningIf("Item Text" == item.title, key + ".Text", "Default item text in use. Type your own one.");
                ErrorIf(string.IsNullOrWhiteSpace(item.title), key + ".Text", "Item text cannot be empty or white spaces. Type your own one.");
                if (item.hasCustomSprite)
                {
                    ErrorIf(IsInvalidAsset(item.customImageSprite), key + ".CustomImage", "Custom image cannot be empty. Add a new one");
                    ErrorIf(IsMissingAsset(item.customImageSprite), key + ".CustomImage", "Custom image is missing. Add a new one");
                }
            }

            if (data.slotOnSameID)
            {
                ErrorIf(!data.rankOrderItems.Any(x => null != x && data.rankOrderSlots.Any(s => s.rankSlotID == x.rankItemID)), itemListKey,
                "Item id must match slot id. Change item id to slot id value.");
            }
        }

        public override VisualElement OnGetVisualSourceFromPath(string path)
		{
            switch (path)
            {
                case "SpawnPosition": return TargetNodeView.Q("scene-element-dropdown").Q<DropdownField>();
                case "Slots List": return TargetNodeView.Query("list-container").AtIndex(0);
                case "Items List": return TargetNodeView.Query("list-container").AtIndex(1);
                default: break;
            }


            if (path.StartsWith("Slot"))
            {
                var slotListVisual = OnGetVisualSourceFromPath("Slots List");
                if (null == slotListVisual)
                {
                    return null;
                }

                int index = ExtractIndexFromPath(path, @"Slot(\d+).");
                if(0 > index || index >= slotListVisual.childCount)
                {
                    return null;
                }
                var slotVisual = slotListVisual.Children().ElementAt(index);
                if (null == slotVisual)
                {
                    return null;
                }

                if (path.EndsWith(".Title"))
                {
                    return slotVisual.Query<TextField>().AtIndex(0);
                }
                else if (path.EndsWith(".CustomTitle"))
                {
                    return slotVisual.Query<TextField>().AtIndex(1);
                }
                else if (path.EndsWith(".ID"))
                {
                    return slotVisual.Q<IntegerField>();
                }
            }

            if (path.StartsWith("Item"))
            {
                var itemListvisual = OnGetVisualSourceFromPath("Items List");
                if (null == itemListvisual)
                {
                    return null;
                }

                int index = ExtractIndexFromPath(path, @"Item(\d+).");
                if (0 > index || index >= itemListvisual.childCount)
                {
                    return null;
                }
                var itemVisual = itemListvisual.Children().ElementAt(index);
                if (null == itemVisual)
                {
                    return null;
                }

                if (path.EndsWith(".Text"))
                {
                    return itemVisual.Q<TextField>();
                }
                else if (path.EndsWith(".CustomImage"))
                {
                    return itemVisual.Q<ObjectField>();
                }
                else if (path.EndsWith(".ID"))
                {
                    return itemVisual.Q<IntegerField>();
                }
            }

            return null;
        }

        private int ExtractIndexFromPath(string input, string pattern)
        {
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
