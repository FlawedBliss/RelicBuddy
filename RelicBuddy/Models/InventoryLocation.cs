using FFXIVClientStructs.FFXIV.Client.Game;

namespace RelicBuddy.Models;

public class InventoryLocation(InventoryType inventoryType, int slot)
{
    public InventoryType InventoryType { get; set; } = inventoryType;
    public int Slot { get; set; } = slot;
}
