using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.Havok.Animation.Playback;
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
                return i;
            }
        }

        return -1;
    }

    public int GetCurrentRelicQuestStage(RelicWeapon relicWeapon, RelicData expansionData)
    {
        var currentQuestStage = GetCurrentRelicItemStage(relicWeapon)+1;
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
}
