using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using RelicBuddy.Helpers;
using RelicBuddy.Helpers.FGui;
using RelicBuddy.Helpers.Strings;
using RelicBuddy.Models;

namespace RelicBuddy.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private static readonly ItemHelper ItemHelper = ItemHelper.Instance;
    private static readonly InventoryHelper InventoryHelper = InventoryHelper.Instance;
    private static readonly ProgressHelper ProgressHelper = ProgressHelper.Instance;
    private static readonly ShopHelper ShopHelper = ShopHelper.Instance;
    private static readonly MapHelper MapHelper = MapHelper.Instance;
    private static readonly QuestHelper QuestHelper = QuestHelper.Instance;
    private static readonly NpcHelper NpcHelper = NpcHelper.Instance;
    private static readonly StringsDict StringsDict = StringsDict.Instance;

    public MainWindow(Plugin plugin) : base("RelicBuddy##rb_mw")
    {
        this.plugin = plugin;
        Flags |= ImGuiWindowFlags.AlwaysVerticalScrollbar;
        
        selectedExpansion = plugin.Configuration.SelectedExpansion;
        selectedJob = plugin.Configuration.SelectedJob;
        UpdateGlobals();
    }

    public void Dispose() { }

    private string selectedExpansion;
    private int selectedJob;
    private RelicData expansionData;
    private RelicWeapon weaponData;
    private List<string> selectableJobs = [];
    private bool resetQuestingColumn = true;
    private int relicQuestStage;
    private int displayedStep = 0;

    public override void Draw()
    {
        if (!InventoryHelper.SaddlebagLoaded)
        {
            FGui.DrawWarningText("Saddlebag is not loaded. Please open it to include saddlebag content in inventory info.");
        }
        ImGui.Columns(3);
        ImGui.SetColumnWidth(0, 200);
        DrawExpansionColumn();
        ImGui.NextColumn();
        DrawDetailColumn();
        ImGui.NextColumn();
        DrawQuestColumn();
    }

    private void DrawExpansionColumn()
    {
        var w = ImGui.GetColumnWidth();
        ImGui.SetCursorPosX((w * 0.5f) - 64);
        ImGui.Image(ItemHelper.GetItemIcon(40949).ImGuiHandle, new Vector2(128, 128));

        FGui.DrawSeparatorText("Expansion");
        foreach (var d in plugin.RelicData)
        {
            ImGui.Selectable($"{d.Name} ({d.Expansion})", selectedExpansion == d.Expansion);
            if (ImGui.IsItemClicked())
            {
                selectedExpansion = d.Expansion;
                plugin.Configuration.SelectedExpansion = selectedExpansion;
                plugin.Configuration.Save();
                UpdateGlobals();
                expansionData = plugin.RelicData.First(d2 => d2.Expansion == selectedExpansion);
            }
        }
    }

    private void DrawDetailColumn()
    {
        DrawJobSelector();

        FGui.DrawSeparatorText($"Current Step: {displayedStep + 1}/{expansionData.Steps.Count}");

        if (ImGui.Button("< Prev"))
        {
            displayedStep = Math.Max(displayedStep - 1, 0);
        }
        ImGui.SameLine();
        ImGui.SetCursorPosX(ImGui.GetColumnOffset()+ImGui.GetColumnWidth() - ImGui.CalcTextSize("Next >").X - 16);
        if (ImGui.Button("Next >"))
        {
            displayedStep = Math.Min(displayedStep +1, expansionData.Steps.Count-1);
        }
        //todo what did this todo mean ?? 
        // TODO temp
        FGui.DrawSeparatorText("Details");
        if (displayedStep < expansionData.Steps.Count) {
            // ImGui.TextWrapped(StringsDict.expansionData.Steps[relicQuestStage].Hint ??
            //                   "No description has been set for this step.");
            var hints = expansionData.Steps[displayedStep].Hints;
            if (hints is not null)
            {
                foreach (var se in hints)
                {
                    StringsDict.GetTemplateRenderer(se)?.Render();
                }
            } else {
                var hint = expansionData.Steps[displayedStep].Hint;
                if (hint != null)
                {
                    var renderer = StringsDict.GetTemplateRenderer(hint);
                    renderer?.Render();
                }
            }
            
        }

        // if (relicItemStage != -1)
        // {
        //     var itemLocations = InventoryHelper.GetItemLocations(weaponData.ItemIds[relicItemStage]);
        //     new WrappedTextSegment("Also, I found your relic in the following locations: ").Draw();
        //     ImGui.Indent();
        //     foreach (var location in itemLocations)
        //     {
        //         ImGui.BulletText($"{location.InventoryType.ToString()}[{location.Slot}]");
        //     }
        //
        //     ImGui.Unindent();
        // }
        // else
        // {
        //     new WrappedTextSegment("I did not find the relic in any of your inventories.").Draw();
        // }
    }

    private void DrawJobSelector()
    {
        FGui.DrawSeparatorText("Job");
        ImGui.Spacing();
        ImGui.PushItemWidth(ImGui.GetColumnWidth() - 16);
        if (ImGui.Combo("", ref selectedJob, selectableJobs.ToArray(), selectableJobs.Count))
        {
            UpdateGlobals();
        }

        ImGui.PopItemWidth();
    }

    private void DrawQuestColumn()
    {
        var titleOffset = 0;
        for (var i = 0; i < expansionData.Steps.Count; ++i)
        {
            var questStep = expansionData.Steps[i];
            if (questStep.IsOneTime) titleOffset++;

            if (relicQuestStage > i)
            {
                ImGui.PushStyleColor(ImGuiCol.Header, KnownColor.ForestGreen.Vector());
            }
            else //since we pop the color, we also need to push the color even if we dont change it
            {
                ImGui.PushStyleColor(ImGuiCol.Header, ImGui.GetColorU32(ImGuiCol.Header));
            }

            var title = $"Stage {i - titleOffset + 1}";
            if (questStep.IsOneTime)
            {
                title = $"One-Time Step {titleOffset}";
            }

            if (resetQuestingColumn)
            {
                ImGui.SetNextItemOpen(relicQuestStage == i);
                if (i == expansionData.Steps.Count - 1)
                {
                    resetQuestingColumn = false;
                }
            }

            if (ImGui.CollapsingHeader(title))
            {
                ImGui.PopStyleColor();
                if (questStep.QuestIdRepeating != 0)
                {
                    DrawQuestStatus(questStep);
                }

                DrawNpcInfo(questStep, i);
                if (questStep.Requirements is not null)
                {
                    if (questStep.Requirements.Item.Count > 0)
                    {
                        FGui.DrawSeparatorText("Required Items");
                        ImGui.Spacing();
                        DrawItemTable(questStep.Requirements.Item, i);
                    }
                }
            }
            else
            {
                ImGui.PopStyleColor();
            }
        }
    }

    private void DrawItemTable(List<ItemQuantity> items, int i)
    {
        ImGui.BeginTable($"ItemTable#ITableStep{i}", 4, ImGuiTableFlags.SizingStretchProp);
        ImGui.TableSetupColumn("Item");
        ImGui.TableSetupColumn("Have");
        ImGui.TableSetupColumn("Need");
        ImGui.TableSetupColumn("Source");
        ImGui.TableHeadersRow();
        foreach (var item in items)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Image(ItemHelper.GetItemIcon(item.ItemId).ImGuiHandle, new Vector2(20, 20));
            ImGui.SameLine();
            ImGui.TextUnformatted($"{ItemHelper.GetItemName(item.ItemId)}");
            ImGui.TableNextColumn();
            ImGui.TextWrapped($"{InventoryHelper.GetItemCount(item.ItemId)}");
            ImGui.TableNextColumn();
            ImGui.TextWrapped($"{item.Quantity}");
            ImGui.TableNextColumn();
            var shop = ShopHelper.GetFirstShopForItem(item.ItemId);
            if (shop is null)
            {
                if (item.Hints.Count > 0)
                {
                    foreach (var hint in item.Hints)
                    {
                        if(StringsDict.dict.TryGetValue(hint, out var renderer))
                        {
                            if (renderer is not null)
                            {
                                renderer.Render();
                            }
                            else
                            {
                                ImGui.TextWrapped(hint);
                                Plugin.PluginLog.Warning($"Unprocessed String: '{hint}'");
                            }
                        }
                    }
                }
                else
                {
                    ImGui.TextWrapped("N/A");
                }
            }
            else
            {
                DrawItemSourceCol(item.ItemId);
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
                //     ImGui.SetTooltip("Click to show shop location on map");
                //     ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
                // }
                //
                // ImGui.SameLine();
                // ImGui.TextUnformatted(shop.ShopResident.Singular.ExtractText());
            }
        }

        ImGui.EndTable();
    }

    private static void DrawQuestStatus(RelicStep step)
    {
        if (step.Prerequsites.Quests.Count > 0)
        {
            FGui.DrawSeparatorText("Prerequisite Quests");
            foreach (var quest in step.Prerequsites.Quests)
            {
                DrawQuestLink(quest);
            }
        }

        FGui.DrawSeparatorText("Relic Quest");
        DrawQuestLink(QuestManager.IsQuestComplete(step.QuestIdFirst) ? step.QuestIdRepeating : step.QuestIdFirst);
    }

    private static void DrawNpcInfo(RelicStep step, int i)
    {
        if (step.Npc is null && step.Object is null) return;
        FGui.DrawSeparatorText("Location");
        ImGui.BeginTable($"LocationTable#LTableStep{i}", 3, ImGuiTableFlags.SizingStretchProp);
        ImGui.TableSetupColumn("Link");
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Map");
        ImGui.TableHeadersRow();
        
        if (step.Npc is not null)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            var npc = NpcHelper.GetNpc(step.Npc!.Value);
            FGui.DrawAetheryteLink(NpcHelper.GetNpcLevel(npc));
            ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(114054)).GetWrapOrEmpty().ImGuiHandle,
                        new Vector2(24, 24));
            
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            if (ImGui.IsItemClicked())
            {
                MapHelper.ShowFlag(NpcHelper.GetNpcLevel(npc));
            }

            ImGui.TableNextColumn();
            ImGui.TextWrapped(npc.Singular.ExtractText());
            ImGui.TableNextColumn();
            ImGui.TextWrapped(NpcHelper.GetNpcLevel(npc).Map.Value.PlaceName.Value.Name.ExtractText());
        }

        if (step.Object is not null)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(114054)).GetWrapOrEmpty().ImGuiHandle,
                        new Vector2(24, 24));
            var obj = NpcHelper.GetObj(step.Object!.Value)!;
            if (ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }

            if (ImGui.IsItemClicked())
            {
                MapHelper.ShowFlag(NpcHelper.GetObjLevel(obj.Value));
            }

            ImGui.TableNextColumn();
            ImGui.TextWrapped(NpcHelper.GetObjName(obj.Value).Singular.ExtractText());
            ImGui.TableNextColumn();
            ImGui.TextWrapped(NpcHelper.GetObjLevel(obj.Value).Map.Value.PlaceName.Value.Name.ExtractText());
        }

        ImGui.EndTable();
    }

    private static void DrawQuestLink(uint id)
    {
        var location = QuestHelper.GetQuestLocation(id);
        FGui.DrawAetheryteLink(location);

        ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(114054)).GetWrapOrEmpty().ImGuiHandle,
                    new Vector2(24, 24));
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            var map = MapUtil.WorldToMap(new Vector2(location.X, location.Z), location.Map.Value.OffsetX,
                                         location.Map.Value.OffsetY, location.Map.Value.SizeFactor);
            ImGui.SetTooltip(
                $"{location.Territory.Value.PlaceName.Value.Name} ({Math.Round(map.X, 2)} {Math.Round(map.Y, 2)})");
        }

        if (ImGui.IsItemClicked())
        {
            MapHelper.ShowFlag(QuestHelper.GetQuestLocation(id));
        }

        ImGui.SameLine();
        ImGui.TextColored(
            QuestManager.IsQuestComplete(id) ? KnownColor.Green.Vector() :
            QuestHelper.IsQuestAccepted(id) ? KnownColor.DodgerBlue.Vector() : KnownColor.DarkRed.Vector(),
            QuestHelper.GetQuestName(id));
    }

    private void UpdateGlobals()
    {
        expansionData = plugin.RelicData.First(d => d.Expansion == selectedExpansion);
        if (plugin.RelicData.Count == 0) return;
        selectableJobs = expansionData.Relics.Keys.ToList();
        if (selectedJob > selectableJobs.Count)
        {
            selectedJob = 0;
        }

        weaponData = expansionData.Relics[selectableJobs[selectedJob]];
        relicQuestStage = ProgressHelper.GetCurrentRelicQuestStage(weaponData, expansionData);
        displayedStep = relicQuestStage;
        plugin.Configuration.SelectedExpansion = selectedExpansion;
        plugin.Configuration.SelectedJob = selectedJob;
        plugin.Configuration.Save();
        resetQuestingColumn = true;
    }

    private void DrawItemSourceCol(uint itemId)
    {
        var npcs = ShopHelper.GetNpcsWithItem(itemId);
        if (npcs.Count > 1)
        {
            if (ImGui.Button($"From {npcs.Count} NPCs##{itemId}"))
            {
                plugin.ItemSourceWindow.DisplayItem = itemId;
                plugin.ItemSourceWindow.IsOpen = true;
            }
        }
        else
        {
            FGui.DrawItemShopRow(itemId, npcs[0], false);
        }
    }
}
