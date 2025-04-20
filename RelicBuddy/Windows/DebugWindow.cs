using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Quic;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Lua;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using RelicBuddy.Helpers;
using RelicBuddy.Models;

namespace RelicBuddy.Windows;

public class DebugWindow : Window, IDisposable
{

    private ExcelSheet<Aetheryte> aetheryteSheet;
    public DebugWindow(Plugin plugin) : base("RelicBuddy Debug")
    {
        this.Plugin = plugin;
        aetheryteSheet = Plugin.DataManager.GetExcelSheet<Aetheryte>()!;
    }


    private Plugin Plugin { get; set; }


    private ItemHelper itemHelper = ItemHelper.Instance;
    private QuestHelper questHelper = QuestHelper.Instance;
    private MapHelper mapHelper = MapHelper.Instance;
    private InventoryHelper inventoryHelper = InventoryHelper.Instance;
    private ShopHelper shopHelper = ShopHelper.Instance;
    private NpcHelper NpcHelper = NpcHelper.Instance;


    private int expansionChoice = 0;
    private uint relicChoice = 0;
    private int id = 0;

    public override void Draw()
    {
        ImGui.BeginTable("relic data by expansion", 4);
        ImGui.TableSetupColumn("Expansion");
        ImGui.TableSetupColumn("Series");
        ImGui.TableSetupColumn("Relic Count");
        ImGui.TableSetupColumn("Relic Stages");
        ImGui.TableHeadersRow();
        foreach (var data in Plugin.RelicData)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(data.Expansion);
            ImGui.TableNextColumn();
            ImGui.Text(data.Name);
            ImGui.TableNextColumn();
            ImGui.Text($"{data.Relics.Count}");
            ImGui.TableNextColumn();
            ImGui.Text(data.Relics.Count == 0 ? "0" : $"{data.Relics.First().Value.ItemIds.Count}");
        }
        
        ImGui.EndTable();
        ImGui.Text($"Total: {Plugin.RelicData.Count}");
        ImGui.Spacing();
        var expansions = Plugin.RelicData.Select(d => d.Expansion).Distinct().ToArray();
        ImGui.Combo("Expansion", ref expansionChoice, expansions, expansions.Length);
        var expansionData = Plugin.RelicData.First(d => d.Expansion == expansions[expansionChoice]);
        if (ImGui.CollapsingHeader("Relic List"))
        {
            ImGui.BeginTable("Expansion Details", expansionData.Relics.First().Value.ItemIds.Count + 1,
                             ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("Job");

            for (var i = 0; i < expansionData.Relics.First().Value.ItemIds.Count; ++i)
            {
                ImGui.TableSetupColumn($"Stage {i + 1}");
            }
            ImGui.TableHeadersRow();
            foreach (var relic in expansionData.Relics)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(relic.Key);
                foreach (var itemId in relic.Value.ItemIds)
                {
                    ImGui.TableNextColumn();
                    ImGui.Image(itemHelper.GetItemIcon(itemId).ImGuiHandle, new Vector2(32, 32));
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip($"{itemHelper.GetItemName(itemId)}\nClick to link this item in chat");
                    }
                    if (ImGui.IsItemClicked())
                    {
                        Plugin.ChatGui.Print(new XivChatEntry
                        {
                            Type = XivChatType.Echo,
                            Message = SeString.CreateItemLink(itemId)
                        });
                    }
                }
            }
            ImGui.EndTable();
        }

        ImGui.Spacing();
        const int tableSize = 7;
        ImGui.BeginTable("Steps", tableSize, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("#");
        ImGui.TableSetupColumn("Quest");
        ImGui.TableSetupColumn("Prerequisite Quests");
        ImGui.TableSetupColumn("Required Item");
        ImGui.TableSetupColumn("Need");
        ImGui.TableSetupColumn("Have");
        ImGui.TableSetupColumn("Source");
        ImGui.TableHeadersRow();
        for (var i = 0; i < expansionData.Steps.Count; ++i)
        {
            var step = expansionData.Steps[i];
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text($"{i + 1}"); // step#
            ImGui.TableNextColumn();
            var questId = step.QuestIdFirst;
            if (QuestManager.IsQuestComplete(questId))
            {
                questId = step.QuestIdRepeating;
            }
            HoverQuestLink(questId);
            ImGui.TableNextColumn();
            for (var j = 0; j < step.Prerequsites.Quests.Count; ++j)
            {
                HoverQuestLink(step.Prerequsites.Quests[j]);
            }

            for (var j = 0; j < step.Requirements.Item.Count; ++j)
            {
                var item = step.Requirements.Item[j];
                ImGui.TableNextColumn();
                ImGui.Image(itemHelper.GetItemIcon(item.ItemId).ImGuiHandle, new Vector2(24, 24));
                ImGui.SameLine();
                ImGui.Text($"{itemHelper.GetItemName(item.ItemId)}");
                ImGui.TableNextColumn();
                ImGui.Text($"{item.Quantity}");
                ImGui.TableNextColumn();
                ImGui.Text($"{inventoryHelper.GetItemCount(item.ItemId)}");
                ImGui.TableNextColumn();
                var shops = shopHelper.GetShopsForItem(item.ItemId);
                foreach (var shop in shops)
                {
                    var npcs = shopHelper.GetShopNpcs(shop.RowId);
                    foreach (var npc in npcs)
                    {
                        ImGui.Image(shopHelper.GetCurrencyTypeIcon(shop.UseCurrencyType).GetWrapOrEmpty().ImGuiHandle,
                                    new Vector2(20, 20));
                        if (ImGui.IsItemClicked())
                        {
                            var location = shopHelper.GetNpcLocation(npc.RowId);
                            if (location is not null)
                            {
                                mapHelper.ShowFlag(location.Value);
                            }
                        }

                        if (ImGui.IsItemHovered())
                        {
                            ImGui.SetTooltip("Click to show shop location on map");
                        }

                        ImGui.SameLine();
                        ImGui.Text(NpcHelper.GetNpc(npc.RowId).Singular.ExtractText());
                    }
                }

                if (j < step.Requirements.Item.Count - 1)
                {
                    for (var skip = 3; skip < tableSize; ++skip)
                    {
                        ImGui.TableNextColumn();
                    }
                }
            }
        }

        ImGui.EndTable();
        MapMarkerStatus();
        // AetheryteTable();+
        var pos = Plugin.ClientState.LocalPlayer?.Position ?? new(0,0,0);
        ImGui.TextUnformatted($"PlayerPos: {pos.X} {pos.Y} {pos.Z}");
        ImGui.Spacing();
    }

    private ExcelSheet<SpecialShop> shopSheet = RelicBuddy.Plugin.DataManager.GetExcelSheet<SpecialShop>();

    
    private unsafe void AetheryteTable()
    {
        ImGui.BeginTable("DebugAetherTable", 7);
        ImGui.TableSetupColumn("#");
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Territory");
        ImGui.TableSetupColumn("Level[0]");
        ImGui.TableSetupColumn("Level[1]");
        ImGui.TableSetupColumn("Level[2]");
        ImGui.TableSetupColumn("Level[3]");
        ImGui.TableHeadersRow();
        foreach (var a in aetheryteSheet.Where(a => a.IsAetheryte))
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{a.RowId}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{a.PlaceName.Value.Name}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{a.Territory.Value.PlaceName.Value.Name}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{a.Level[0].RowId}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{a.Level[1].RowId}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{a.Level[2].RowId}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{a.Level[3].RowId}");
        }
        ImGui.EndTable();

    }

    private unsafe void MapMarkerStatus()
    {
        if (AgentMap.Instance()->IsFlagMarkerSet)
        {
            var marker = AgentMap.Instance()->FlagMapMarker;
            ImGui.TextUnformatted($"MapMarker: {marker.XFloat} {marker.YFloat}");
        }
    }

    private void HoverQuestLink(uint id)
    {
        ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(114054)).GetWrapOrEmpty().ImGuiHandle,
                    new Vector2(24, 24));
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Click to show quest location on map");
        }

        if (ImGui.IsItemClicked())
        {
            mapHelper.ShowFlag(questHelper.GetQuestLocation(id));
        }

        ImGui.SameLine();
        if (QuestManager.IsQuestComplete(id))
        {
            ImGui.TextColored(KnownColor.Green.Vector(), questHelper.GetQuestName(id));
        }
        else
        {
            ImGui.TextColored(KnownColor.DarkRed.Vector(), questHelper.GetQuestName(id));
        }
    }

    public void Dispose() { }
}
