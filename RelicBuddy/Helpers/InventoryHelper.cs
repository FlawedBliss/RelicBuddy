using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using RelicBuddy.Models;

namespace RelicBuddy.Helpers;

//TODO: cache results until inventory changes
public class InventoryHelper
{
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

    private readonly InventoryType[] retainerInventories =
    [
        InventoryType.RetainerPage1, InventoryType.RetainerPage2, InventoryType.RetainerPage3,
        InventoryType.RetainerPage4, InventoryType.RetainerPage5, InventoryType.RetainerPage6,
        InventoryType.RetainerPage7
    ];

    private readonly InventoryType[] allInventories;

    //todo add other gear chests when adding support for relic armor
    private readonly InventoryType[] gearchestInventories =
    [
        InventoryType.ArmoryMainHand, InventoryType.ArmoryOffHand
    ];

    private readonly unsafe InventoryManager* inventoryManager = InventoryManager.Instance();
    private readonly unsafe RetainerManager* retainerManager = RetainerManager.Instance();
    public int ActiveRetainers { get; private init; }
    private static InventoryHelper? _instance = null;

    public static InventoryHelper Instance => _instance ??= new InventoryHelper();

    public unsafe bool SaddlebagLoaded => inventoryManager->GetInventoryContainer(saddlebagInventories[0])->Loaded == 0;
    public bool RetainersLoaded => RetainerCache.Count == ActiveRetainers;

    public Dictionary<ulong, Dictionary<InventoryType, List<InventoryLocation>>> RetainerCache { get; private set; } = new();

    private unsafe InventoryHelper()
    {
        for (int i = 0; i < retainerManager->Retainers.Length; i++)
        {
            if (!retainerManager->Retainers[i].Available)
            {
                ActiveRetainers = i;
                break;
            }
        }
        allInventories = playerInventories.Concat(saddlebagInventories).Concat(gearchestInventories).ToArray();
    }

    public int GetItemCount(uint itemId)
    {
        return GetInventoryItemCount(itemId) + GetSaddlebagItemCount(itemId) + GetRetainerItemCount(itemId);
    }

    public unsafe int GetInventoryItemCount(uint itemId, bool searchArmory = false)
    {
        var numNq = inventoryManager->GetInventoryItemCount(itemId);
        var numHq = inventoryManager->GetInventoryItemCount(itemId, true);
        return numNq + numHq;
    }

    public unsafe int GetSaddlebagItemCount(uint itemId)
    {
        if (Plugin.ClientState.LocalPlayer is null) return 0;
        if (inventoryManager->GetInventoryContainer(saddlebagInventories[0])->Loaded == 0)
        {
            return 0;
        }

        var sum = 0;
        foreach (var saddlebagInventory in saddlebagInventories)
        {
            sum += inventoryManager->GetItemCountInContainer(itemId, saddlebagInventory);
            sum += inventoryManager->GetItemCountInContainer(itemId, saddlebagInventory, true);
        }

        return sum;
    }

    public int GetRetainerItemCount(uint itemId)
    {
        return RetainerCache.Values
                            .SelectMany(e => e.Values)
                            .SelectMany(i => i)
                            .Where(location => location.ItemId == itemId)
                            .Sum(loc => loc.Count);
    }

    public unsafe List<InventoryLocation> GetItemLocations(uint itemId)
    {
        if (Plugin.ClientState.LocalPlayer is null) return [];
        var locations = new List<InventoryLocation>();
        foreach (var invType in allInventories)
        {
            if (inventoryManager->GetItemCountInContainer(itemId, invType) > 0)
            {
                for (var i = 0; i < inventoryManager->GetInventoryContainer(invType)->Size; ++i)
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

    public unsafe void UpdateRetainerInventory(AddonEvent type, AddonArgs args)
    {
        if (inventoryManager->GetInventoryContainer(InventoryType.RetainerPage1)->Loaded == 0)
            return;
        var retainerInventory = new Dictionary<InventoryType, List<InventoryLocation>>();
        foreach (var inventory in retainerInventories)
        {
            retainerInventory[inventory] = new();
            var container = inventoryManager->GetInventoryContainer(inventory);
            for (var i = 0; i < container->Size; ++i)
            {
                var loc = new InventoryLocation(inventory, i, container->GetInventorySlot(i)->ItemId,
                                                container->GetInventorySlot(i)->Quantity);
                retainerInventory[inventory].Add(loc);
            }
        }

        RetainerCache[retainerManager->LastSelectedRetainerId] = retainerInventory;
    }
}
