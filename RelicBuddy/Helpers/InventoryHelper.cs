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

    private int _activeRetainers = -1;

    public int ActiveRetainers
    {
        get
        {
            if (_activeRetainers == -1)
            {
                UpdateActiveRetainers();
            }
            return _activeRetainers;
        }
    }

    private static InventoryHelper? _instance = null;

    public static InventoryHelper Instance => _instance ??= new InventoryHelper();

    public unsafe bool SaddlebagLoaded
    {
        get
        {
            if (inventoryManager->GetInventoryContainer(saddlebagInventories[0]) == null)
            {
                return false;
            }
            return inventoryManager->GetInventoryContainer(saddlebagInventories[0])->IsLoaded;
        }
    }

    public bool RetainersLoaded => RetainerCache.Count == ActiveRetainers;

    public Dictionary<ulong, Dictionary<InventoryType, List<InventoryLocation>>> RetainerCache { get; private set; } = new();

    private unsafe InventoryHelper()
    {
        allInventories = playerInventories.Concat(saddlebagInventories).Concat(gearchestInventories).ToArray();
    }

    private unsafe void UpdateActiveRetainers()
    {
        if (retainerManager->IsReady)
        {
            return;
        }
        for (var i = 0; i < retainerManager->Retainers.Length; i++)
        {
            if (!retainerManager->Retainers[i].Available)
            {
                _activeRetainers = i;
                break;
            }
        }
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
        if (!SaddlebagLoaded) return 0;

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

    public IEnumerable<InventoryLocation> GetItemLocations(uint itemId)
    {
       return new List<InventoryLocation>(GetInventoryItemLocations(itemId)).Concat(GetRetainerItemLocations(itemId));
    }

    public unsafe List<InventoryLocation> GetInventoryItemLocations(uint itemId)
    {
        if (Plugin.ClientState.LocalPlayer is null) return [];
        var locations = new List<InventoryLocation>();
        foreach (var invType in playerInventories)
        {
            if (inventoryManager->GetItemCountInContainer(itemId, invType) > 0)
            {
                for (var i = 0; i < inventoryManager->GetInventoryContainer(invType)->Size; ++i)
                {
                    var slot = inventoryManager->GetInventorySlot(invType, i);
                    if (slot->ItemId == itemId)
                    {
                        locations.Add(new InventoryLocation(invType, i, itemId, slot->Quantity));
                    }
                }
            }
        }
        return locations;
    }

    public List<InventoryLocation> GetRetainerItemLocations(uint itemId)
    {
        var locations = new List<InventoryLocation>();
        foreach (var retainer in RetainerCache)
        {
            foreach (var page in retainer.Value)
            {
                foreach (var item in page.Value)
                {
                    if (item.ItemId == itemId)
                    {
                        locations.Add(item);
                    }
                }
            }
        }

        return locations;
    }
    public unsafe void UpdateRetainerInventory(AddonEvent type, AddonArgs args)
    {
        if (inventoryManager->GetInventoryContainer(InventoryType.RetainerPage1)->IsLoaded)
            return;
        var retainerInventory = new Dictionary<InventoryType, List<InventoryLocation>>();
        foreach (var inventory in retainerInventories)
        {
            retainerInventory[inventory] = new();
            var container = inventoryManager->GetInventoryContainer(inventory);
            for (var i = 0; i < container->Size; ++i)
            {
                var loc = new InventoryLocation(inventory, i, container->GetInventorySlot(i)->ItemId,
                                                container->GetInventorySlot(i)->Quantity, retainerManager->LastSelectedRetainerId);
                retainerInventory[inventory].Add(loc);
            }
        }

        RetainerCache[retainerManager->LastSelectedRetainerId] = retainerInventory;
    }

    public unsafe string GetRetainerName(ulong retainerId)
    {
        for (var i = 0; i < ActiveRetainers; ++i)
        {
            if (retainerManager->Retainers[i].RetainerId == retainerId)
            {
                return retainerManager->Retainers[i].NameString;
            }
        }

        return "unknown retainer";
    }
}
