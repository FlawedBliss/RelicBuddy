using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.Sheets;
using RelicBuddy.Helpers;
using RelicBuddy.Helpers.FGui;
using RelicBuddy.Models;

namespace RelicBuddy.Windows;

public class ItemSourceWindow : Window, IDisposable
{
    private uint displayItem;
    private List<ShopInfo> shopInfos;
    private Item itemInfo;
    private readonly ShopHelper ShopHelper = Helpers.ShopHelper.Instance;
    private readonly ItemHelper ItemHelper = Helpers.ItemHelper.Instance;
    private readonly MapHelper MapHelper = Helpers.MapHelper.Instance;
    
    public uint DisplayItem
    {
        get => displayItem;
        set
        {
            displayItem = value;
            shopInfos = ShopHelper.GetNpcsWithItem(displayItem);
        }
    }

    public ItemSourceWindow(Plugin plugin) : base ("RelicBuddy - Item Shops")
    {
        this.Plugin = plugin;
    }

    public Plugin Plugin { get; set; }

    public override void Draw()
    {
        ImGui.Image(ItemHelper.GetItemIcon(displayItem).ImGuiHandle, new Vector2(16, 16));
        ImGui.SameLine();
        ImGui.TextUnformatted(ItemHelper.GetItemName(displayItem));
        ImGui.Separator();
        foreach(var shop in this.shopInfos) {
            FGui.DrawItemShopRow(displayItem, shop);
            // ImGui.Image(
            //     ShopHelper.GetCurrencyTypeIcon(shop.SpecialShop.UseCurrencyType).GetWrapOrEmpty().ImGuiHandle,
            //     new Vector2(20, 20));
            // if (ImGui.IsItemClicked())
            // {
            //     MapHelper.ShowFlag(shop.ShopLevel!.Value);
            // }
            //
            // if (ImGui.IsItemHovered()) 
            // {
            //     // ImGui.SetTooltip("Click to show shop location on map");
            //     ImGui.SetTooltip($"{shop.SpecialShop.RowId}");
            // }
            //
            // ImGui.SameLine();
            // ImGui.TextUnformatted(shop.ShopResident.Singular.ExtractText());
        }
    }

    public void Dispose()
    {
    }
}
