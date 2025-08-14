using System;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Bindings.ImGui;
using RelicBuddy.Helpers;

namespace RelicBuddy.Windows;

public class RelicWindow : Window, IDisposable
{
    private Plugin Plugin;
    private InventoryHelper inventoryHelper = InventoryHelper.Instance;

    public RelicWindow(Plugin plugin) : base("a window thing")
    {
        Plugin = plugin;
    }
    
    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("this is the relic tracker window");
        ImGui.Spacing();
        ImGui.Text($"You currently have {inventoryHelper.GetItemCount(24511)} wooden lofts.");

    }
}
