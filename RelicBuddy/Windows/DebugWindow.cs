using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Bindings.ImGui;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using RelicBuddy.Helpers;
using RelicBuddy.Models;
using RelicNote = FFXIVClientStructs.FFXIV.Client.Game.UI.RelicNote;

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
        var expansions = Plugin.RelicData.Select(d => d.Expansion).Distinct().ToArray();
        ImGui.Combo("Expansion", ref expansionChoice, expansions, expansions.Length);
        var expansionData = Plugin.RelicData.First(d => d.Expansion == expansions[expansionChoice]);

        ImGui.Spacing();
        MapMarkerStatus();
        var pos = Plugin.ClientState.LocalPlayer?.Position ?? new(0, 0, 0);
        ImGui.TextUnformatted($"PlayerPos: {pos.X} {pos.Y} {pos.Z}");
        ImGui.Spacing();
        DrawRelicNoteInfo();

        if (ImGui.CollapsingHeader("Map"))
        {
            DrawMapInfo();
        }

        DrawLeveInfo();
        DrawObjectTable();
        
    }

    private ExcelSheet<SpecialShop> shopSheet = RelicBuddy.Plugin.DataManager.GetExcelSheet<SpecialShop>();
    
    private unsafe void DrawRelicNoteInfo()
    {
        if (ImGui.CollapsingHeader("RelicNote"))
        {
            var noteInfo = RelicNoteHelper.Instance.GetCurrentNoteData();
            if (noteInfo is null) return;
            var note = RelicNote.Instance();
            if (note is null) return;
            ImGui.BeginTable("RelicNote##Dungeon", 2, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("DutyFinderCondition");
            ImGui.TableSetupColumn("Completion");
            ImGui.TableHeadersRow();
            for (var i=0; i < noteInfo.Value.MonsterNoteTargetNM.Count; ++i)
            {
                var nm = noteInfo.Value.MonsterNoteTargetNM[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                var condition = DutyHelper.Instance.GetContentFinderConditionByName(
                    nm.Value.PlaceNameLocation.First().Value.Name.ExtractText());
                if(condition is not null) {
                    if (ImGui.Button(condition.Value.Name.ExtractText()))
                    {
                        DutyHelper.Instance.OpenDutyFinder(condition.Value.RowId);
                    }
                }
                else
                {
                    ImGui.TextUnformatted(nm.Value.PlaceNameLocation.First().Value.Name.ExtractText());
                }

                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{note->IsDungeonComplete(i)}");
                
            }
            ImGui.EndTable();

            ImGui.BeginTable("RelicNote##Monsters", 3, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Location");
            ImGui.TableSetupColumn("Progress");
            ImGui.TableHeadersRow();
            for (var i = 0; i < noteInfo.Value.MonsterNoteTargetCommon.Count; ++i)
            {
                var monster = noteInfo.Value.MonsterNoteTargetCommon[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{monster.Value.BNpcName.Value.Singular.ExtractText()}");
                ImGui.TableNextColumn();
                //ImGui.TextUnformatted($"{monster.Value.PlaceNameLocation.First().Value.Name.ExtractText()}, {monster.Value.PlaceNameZone.First().Value.Name.ExtractText()} ({monster.Value.PlaceNameZone.First().Value.RowId})");
                if (ImGui.Button($"{monster.Value.PlaceNameLocation.First().Value.Name.ExtractText()}"))
                {
                    mapHelper.OpenMap(mapHelper.GetMapByPlaceNameId(monster.Value.PlaceNameZone.First().Value.RowId).RowId);
                }
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{note->GetMonsterProgress(i)} / {noteInfo.Value.MonsterCount[i]}");
            } //mark them on map
            ImGui.EndTable();
            ImGui.TextUnformatted($"ObjectveProress: {note->ObjectiveProgress}");
        }
    }

    private unsafe void DrawLeveInfo()
    {
        if (ImGui.CollapsingHeader("Leve Info"))
        {
            var noteInfo = RelicNoteHelper.Instance.GetCurrentNoteData();
            if (noteInfo is null) return;
            var note = RelicNote.Instance();
            if (note is null) return;

            ImGui.BeginTable("LeveTable", 4, ImGuiTableFlags.SizingFixedFit);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Location");
            ImGui.TableSetupColumn("Issuer");
            ImGui.TableSetupColumn("Completion");
            ImGui.TableHeadersRow();
            for (var i = 0; i < noteInfo.Value.Leve.Count; ++i)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{noteInfo.Value.Leve[i].Value.Name.ExtractText()}");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{noteInfo.Value.Leve[i].Value.PlaceNameIssued.Value.Name.ExtractText()}");
                ImGui.TableNextColumn();
                ImGui.PushFont(UiBuilder.IconFont);
                if (ImGui.Button($"\uf0ac##levebtn{noteInfo.Value.Leve[i].RowId}")) 
                {
                    mapHelper.ShowFlag(noteInfo.Value.Leve[i].Value.LevelLevemete.Value);
                }
                ImGui.PopFont();
                ImGui.SameLine();
                ImGui.TextUnformatted($"{NpcHelper.GetNpcFromLevel(noteInfo.Value.Leve[i].Value.LevelLevemete.Value)?.Singular.ExtractText() ?? "Unknown NPC"}");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{note->IsLeveComplete(i)}");
            }

            ImGui.EndTable();
        }
    }

    private unsafe void DrawObjectTable()
    {
        var target = Plugin.ClientState.LocalPlayer?.TargetObject;
        if (target is not null)
        {
            ImGui.TextUnformatted(target.Name.TextValue);
            ImGui.TextUnformatted($"{target.DataId}");
            ImGui.TextUnformatted($"{target.ObjectKind}");
            ImGui.TextUnformatted($"{target.GetType()}");
            if (target is IBattleNpc bnpc)
            {
                ImGui.TextUnformatted($"{bnpc}");
            }
        }
    }
    private unsafe void DrawMapInfo()
    {
        ImGui.BeginTable("minimap##", 1, ImGuiTableFlags.SizingFixedFit);
        ImGui.TableSetupColumn("Entry");
        ImGui.TableHeadersRow();
        ImGui.TextUnformatted($"Count: {AgentMap.Instance()->MiniMapMarkers.Length}");

        foreach(var marker in AgentMap.Instance()->EventMarkers)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            // ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(marker.IconId).GetWrapOrDefault().ImGuiHandle, new(32, 32));
            // ImGui.SameLine();
            ImGui.TextUnformatted($"{marker.GetType()} | {marker.IconId} | {marker.TooltipString->StringPtr.ExtractText()}");
        }
        foreach (var marker in AgentMap.Instance()->MiniMapMarkers)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(marker.MapMarker.IconId).GetWrapOrDefault().Handle, new(32, 32));
            ImGui.SameLine();
            ImGui.TextUnformatted($"{marker.MapMarker.GetType()} | {marker.MapMarker.X},{marker.MapMarker.Y} | {marker.MapMarker.IconId} | {marker.MapMarker.Subtext.ExtractText()}");
        }
        ImGui.EndTable();
    }
    private void DrawRelicOverview()
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
                    ImGui.Image(itemHelper.GetItemIcon(itemId).Handle, new Vector2(32, 32));
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
    }

    private void DrawRelicSteps(RelicData expansionData)
    {
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
            var questId = QuestHelper.Instance.GetQuestIdForStep(step, null);
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
                ImGui.Image(itemHelper.GetItemIcon(item.ItemId).Handle, new Vector2(24, 24));
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
                        ImGui.Image(shopHelper.GetCurrencyTypeIcon(shop.UseCurrencyType).GetWrapOrEmpty().Handle,
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
        var pos = Plugin.ObjectTable.LocalPlayer?.Position ?? new(0,0,0);
        ImGui.TextUnformatted($"PlayerPos: {pos.X} {pos.Y} {pos.Z}");
        ImGui.Spacing();
    }

    private void DrawRelicList(RelicData expansionData)
    {
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
                    ImGui.Image(itemHelper.GetItemIcon(itemId).Handle, new Vector2(32, 32));
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
    }

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
        if (AgentMap.Instance()->FlagMapMarkers.Length > 0)
        {
            var markers = AgentMap.Instance()->FlagMapMarkers;
            foreach(var marker in markers) {
                ImGui.TextUnformatted($"MapMarker: {marker.XFloat} {marker.YFloat}");
            }
        }
    }

    private void HoverQuestLink(uint id)
    {
        ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(114054)).GetWrapOrEmpty().Handle,
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
