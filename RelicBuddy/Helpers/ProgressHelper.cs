using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using RelicBuddy.Models;

namespace RelicBuddy.Helpers;

public class ProgressHelper
{
    private static ProgressHelper? _instance = null;

    public static ProgressHelper Instance => _instance ??= new ProgressHelper();
    private InventoryHelper inventoryHelper = InventoryHelper.Instance;
    private ProgressHelper() { }

    public int GetCurrentRelicItemStage(RelicWeapon relicWeapon)
    {
        for (var i = 0; i < relicWeapon.ItemIds.Count; ++i)
        {
            if (inventoryHelper.GetItemCount(relicWeapon.ItemIds[i]) > 0)
            {
                return i+1;
            }
        }

        return 0;
    }

    public int GetCurrentRelicQuestStage(RelicWeapon relicWeapon, RelicData expansionData)
    {
        var currentQuestStage = GetCurrentRelicItemStage(relicWeapon);
        for (var i = 0; i < expansionData.Steps.Count; ++i)
        {
            // Plugin.PluginLog.Debug($"{currentQuestStage} >= {i}");
            if (currentQuestStage >= i)
            {
                // Plugin.PluginLog.Debug($"{expansionData.Steps[i].IsOneTime}");
                if (expansionData.Steps[i].IsOneTime)
                {
                    // Plugin.PluginLog.Debug($"{QuestManager.IsQuestComplete(expansionData.Steps[i].QuestIdFirst)}");
                    if (!QuestManager.IsQuestComplete(expansionData.Steps[i].QuestIdFirst))
                    {
                        break;
                    }
                    currentQuestStage++;
                }
            }
        }

        return currentQuestStage;
    }

    public List<ItemQuantity> GetAllMissingItemsForExpansion(RelicData expansionData)
    {
        var dict = new Dictionary<uint, uint>();
        foreach (var relic in expansionData.Relics)
        {
            var progress = GetCurrentRelicQuestStage(relic.Value, expansionData);
            for (var i = 0; i < expansionData.Steps.Count; i++)
            {
                if (progress >= i) continue;
                var step = expansionData.Steps[i];
                foreach (var item in step.Requirements?.Item ?? [])
                {
                    dict.TryGetValue(item.ItemId, out var count);
                    dict[item.ItemId] = item.Quantity + count;
                }
            }
        }

        return dict.Select(x => new ItemQuantity(x.Key, x.Value)).ToList();
    }
}
