using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using RelicBuddy.Helpers;
using RelicBuddy.Helpers.FGui;
using RelicBuddy.Models;

namespace RelicBuddy.Windows;

public class ItemLocationWindow : Window, IDisposable
{
    
    private readonly ItemHelper ItemHelper = ItemHelper.Instance;
    private readonly InventoryHelper InventoryHelper = InventoryHelper.Instance;
    private uint displayItem;
    private IEnumerable<InventoryLocation> inventoryLocations = [];
    
    public uint DisplayItem
    {
        get => displayItem;
        set
        {
            displayItem = value;
            inventoryLocations = InventoryHelper.GetItemLocations(displayItem);
        }
    }

    public ItemLocationWindow(Plugin plugin) : base ("RelicBuddy - Item Search")
    {
        this.Plugin = plugin;
        Size = new Vector2(300, 200);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public Plugin Plugin { get; set; }

    public override void Draw()
    {
        ImGui.Image(ItemHelper.GetItemIcon(displayItem).Handle, new Vector2(16, 16));
        ImGui.SameLine();
        ImGui.TextUnformatted(ItemHelper.GetItemName(displayItem));
        ImGui.Separator();
        if (!inventoryLocations.Any())
        {
            ImGui.TextUnformatted("No items found in loaded inventories.");
            return;
        }
        foreach (var location in inventoryLocations)
        {
            // todo convert retainer page to 5-based
            ImGui.TextUnformatted(location.RetainerId is not null
                                      ? $"{InventoryHelper.GetRetainerName(location.RetainerId.Value)} - {location.InventoryType.ToString()}: {location.Count}"
                                      : $"{location.InventoryType.ToString()}: {location.Count}");
        }
    }

    public void Dispose()
    {
    }
}
