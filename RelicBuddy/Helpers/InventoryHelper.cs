using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using RelicBuddy.Models;

namespace RelicBuddy.Helpers;

//TODO: cache results until inventory changes
public class InventoryHelper
{
    private readonly unsafe InventoryManager* inventoryManager = InventoryManager.Instance();
    private static InventoryHelper? _instance = null;

    public static InventoryHelper Instance => _instance ??= new InventoryHelper();
    public const ushort InventorySize = 35;

    public bool SaddlebagLoaded { private set; get; } = false;
    
    private InventoryHelper()
    {
        allInventories = playerInventories.Concat(saddlebagInventories).Concat(gearchestInventories).ToArray();
    }

    public int GetItemCount(uint itemId)
    {
        return GetInventoryItemCount(itemId) + GetSaddlebagItemCount(itemId);
    }

    public unsafe int GetInventoryItemCount(uint itemId, bool searchArmory = false)
    {
        var numNq = inventoryManager->GetInventoryItemCount(itemId);
        var numHq = inventoryManager->GetInventoryItemCount(itemId, true);
        return numNq + numHq;
    }

    private readonly InventoryType[] saddlebagInventories =
    [
        InventoryType.SaddleBag1, InventoryType.SaddleBag2,
        InventoryType.PremiumSaddleBag1, InventoryType.PremiumSaddleBag2
    ];

    private readonly InventoryType[] playerInventories =
    [
        InventoryType.Inventory1, InventoryType.Inventory2,
        InventoryType.Inventory4, InventoryType.Inventory4
    ];

    private readonly InventoryType[] allInventories;

    //there are more but for this plugin, we only need these
    private readonly InventoryType[] gearchestInventories = [
        InventoryType.ArmoryMainHand, InventoryType.ArmoryOffHand
    ];

    public unsafe int GetSaddlebagItemCount(uint itemId)
    {
        if (Plugin.ClientState.LocalPlayer is null) return 0;
        if (inventoryManager->GetInventoryContainer(saddlebagInventories[0])->Loaded == 0)
        {
            return 0;
        }
        if(!SaddlebagLoaded) SaddlebagLoaded = true;
        var sum = 0;
        foreach (var saddlebagInventory in saddlebagInventories)
        {
            sum += inventoryManager->GetItemCountInContainer(itemId, saddlebagInventory);
            sum += inventoryManager->GetItemCountInContainer(itemId, saddlebagInventory, true);
        }

        return sum;
    }

    public unsafe List<InventoryLocation> GetItemLocations(uint itemId)
    {
        if (Plugin.ClientState.LocalPlayer is null) return [];
        var locations = new List<InventoryLocation>();
        foreach (var invType in allInventories)
        {
            if (inventoryManager->GetItemCountInContainer(itemId, invType) > 0)
            {
                for (var i = 0; i < InventorySize; ++i)
                {
                    if (inventoryManager->GetInventorySlot(invType, i)->ItemId == itemId)
                    {
                        locations.Add(new InventoryLocation(invType, i));
                    }
                }
            }
        }

        return locations;
    }
}
