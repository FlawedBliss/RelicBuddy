using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Achievement = FFXIVClientStructs.FFXIV.Client.Game.UI.Achievement;
using Map = Lumina.Excel.Sheets.Map;

namespace RelicBuddy.Helpers;

public class MapHelper
{
    private static MapHelper? _instance = null;
    private static unsafe Telepo* telepo = Telepo.Instance();

    private readonly ExcelSheet<Aetheryte> aetheryteSheet;
    private readonly SubrowExcelSheet<MapMarker> mapMarkerSheet;
    private readonly ExcelSheet<Map> mapSheet;

    
    public static MapHelper Instance => _instance ??= new MapHelper();


    private MapHelper()
    {

        aetheryteSheet = Plugin.DataManager.GetExcelSheet<Aetheryte>()!;
        mapMarkerSheet = Plugin.DataManager.GetSubrowExcelSheet<MapMarker>()!;
        mapSheet = Plugin.DataManager.GetExcelSheet<Map>()!;
    }

    public String GetMapName(uint map)
    {
        return mapSheet.GetRow(map).PlaceName.Value.Name.ExtractText();
    }


    public unsafe void ShowFlag(Level level)
    {
        AgentMap.Instance()->SetFlagMapMarker(level.Territory.RowId, level.Map.RowId,
                                              new Vector3(level.X, level.Y, level.Z));
        AgentMap.Instance()->OpenMap(level.Map.RowId, level.Territory.RowId);
    }

    public unsafe void OpenMap(uint mapId)
    {
        AgentMap.Instance()->OpenMap(mapId);
    }

    public unsafe void ShowFlag(uint territoryType, uint map, float x, float y)
    {
        AgentMap.Instance()->FlagMarkerCount = 0;
        AgentMap.Instance()->SetFlagMapMarker(territoryType, map, x, y);
        AgentMap.Instance()->OpenMap(map, territoryType);
    }


    public Vector2 GetAetherytePosition(Aetheryte aetheryte)
    {
        var marker = mapMarkerSheet.Flatten().FirstOrDefault(m => m.DataType == 3 && m.DataKey.RowId == aetheryte.RowId);
        return MapMarkerToWorldCoordinate(marker.X, marker.Y, aetheryte.Map.Value!
                                                                     .SizeFactor, aetheryte.Map.Value!.OffsetX, aetheryte.Map.Value!.OffsetY);
    }

    public Vector2 MapMarkerToWorldCoordinate(float x, float y, ushort scale, short offsetX, short offsetY)
    {
        return (new Vector2(x, y) - new Vector2(1024f, 1024f)) * (100f / scale) - new Vector2(offsetX, offsetY);
    }

    //TODO cache
    public Aetheryte? FindClosestAetheryte(Level level)
    {
        var inTerritory = GetAetherytesInTerritory(level.Territory!.Value!);
        if (inTerritory.Count == 0) return null;
        var hasBig = inTerritory.Where(a => a.IsAetheryte).ToList();
        switch (hasBig.Count)
        {
            case 1:
                return hasBig.First();
            case > 1:
                return hasBig.MinBy(a => SquaredDistance(new Vector2(level.X, level.Z),
                                                         MapMarkerToWorldCoordinate(
                                                             GetAetherytePosition(a).X, GetAetherytePosition(a).Y, level.Map.Value!.SizeFactor, level.Map.Value!.OffsetX, level.Map.Value.OffsetY)));
            default:
                return GetBigAetherytesInAethernet(inTerritory.First().AethernetGroup).First();
        }
    }

    // aetheryte icon 60453
    private List<Aetheryte> GetBigAetherytesInAethernet(byte aethernet)
    {
        return aetheryteSheet.Where(a => a.AethernetGroup == aethernet && a.IsAetheryte).ToList();
    }

    private List<Aetheryte> GetAetherytesInTerritory(TerritoryType territory)
    {
        return aetheryteSheet.Where(a => a.Territory.RowId == territory.RowId).ToList();
    }

    public unsafe bool IsAttunedTo(uint aetheryte)
    {
        if (telepo == null)
        {
            Plugin.PluginLog.Warning("Failed to check attunement: telepo is null");
            return false;
        }

        if (Plugin.ObjectTable.LocalPlayer == null) return false;
        telepo->UpdateAetheryteList();
        foreach (var teleportInfo in telepo->TeleportList)
        {
            if (teleportInfo.AetheryteId == aetheryte) return true;
        }

        return false;
    }

    public unsafe bool TeleportTo(uint aetheryte)
    {
        if (!IsAttunedTo(aetheryte))
        {
            Plugin.PluginLog.Warning("Won't teleport to unattuned aetheryte");
            return false;
        }

        telepo->Teleport(aetheryte, 0);
        return true;
    }

    private float SquaredDistance(Vector2 a, Vector2 b)
    {
        var x = a.X - b.X;
        var y = a.Y - b.Y;
        return x * x + y * y;
    }
}
