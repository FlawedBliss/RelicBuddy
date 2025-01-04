﻿using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using ImGuiNET;
using Lumina.Excel.Sheets;
using RelicBuddy.Models;

namespace RelicBuddy.Helpers.FGui;

public class FGui
{
    private static readonly MapHelper MapHelper = Helpers.MapHelper.Instance;
    private static readonly ItemHelper ItemHelper = ItemHelper.Instance;
    private static readonly ShopHelper ShopHelper = ShopHelper.Instance;
    private static readonly InventoryHelper InventoryHelper = InventoryHelper.Instance;
    public static void DrawItemShopRow(uint itemId, ShopInfo shopInfo, bool drawCosts = true)
    {
        var row = shopInfo.SpecialShop.Item.First(i => i.ReceiveItems.Any(r => r.Item.RowId == itemId));
        if (shopInfo.ShopLevel is not null)
        {
            DrawAetheryteLink(shopInfo.ShopLevel.Value);
        }
        ImGui.TextUnformatted($"{shopInfo.ShopResident.Singular.ExtractText()} - {shopInfo.SpecialShop.Name.ExtractText()}");
        if (ImGui.IsItemClicked())
        {
            MapHelper.Instance.ShowFlag(shopInfo.ShopLevel!.Value);
        }
        if(drawCosts) {
            ImGui.Indent();
            foreach (var costItem in row.ItemCosts.Where(r => r.ItemCost.RowId != 0))
            {
                var currencyItem = ShopHelper.ConvertCurrency(costItem.ItemCost.RowId, shopInfo.SpecialShop);
                Plugin.PluginInterface.UiBuilder.MonoFontHandle.Push();
                ImGui.TextUnformatted($"{costItem.CurrencyCost.ToString(),3}");
                Plugin.PluginInterface.UiBuilder.MonoFontHandle.Pop();
                ImGui.SameLine();
                ImGui.Image(ItemHelper.GetItemIcon(currencyItem).ImGuiHandle, new(16, 16));
                ImGui.SameLine();
                ImGui.TextUnformatted($"{ItemHelper.GetItemName(currencyItem)}");
            }
            ImGui.Unindent();
        }

    }

    public static void DrawAetheryteLink(Level level)
    {
        var aetheryte = MapHelper.FindClosestAetheryte(level);
        if (aetheryte is not null)
        {
            ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(Icons.Aetheryte)).GetWrapOrEmpty().ImGuiHandle,
                        new Vector2(24, 24));

            var pos = MapHelper.GetAetherytePosition(aetheryte.Value);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                ImGui.SetTooltip(
                    $"{aetheryte.Value.PlaceName.Value.Name.ExtractText()}{(MapHelper.IsAttunedTo(aetheryte.Value.RowId) ? "" : " (You are not attuned to this aetheryte)")}");
            }

            if (ImGui.IsItemClicked())
            {
                MapHelper.ShowFlag(aetheryte.Value.Territory.RowId, aetheryte.Value.Map.RowId, pos.X, pos.Y);
            }

            ImGui.SameLine();
        }
    }
}