using FFXIVClientStructs.FFXIV.Client.Game;

namespace RelicBuddy.Models;

public class InventoryLocation(InventoryType inventoryType, int slot, uint itemId = 0, int count = 0, ulong? retainerId = null)
{
    public InventoryType InventoryType { get; set; } = inventoryType;
    public int Slot { get; set; } = slot;
    public uint ItemId { get; set; } = itemId;
    public int Count { get; set; } = count;
    public ulong? RetainerId { get; set; } = retainerId;
}
