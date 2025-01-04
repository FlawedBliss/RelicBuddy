using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace RelicBuddy.Helpers;

public class NpcHelper
{
    private static NpcHelper? _instance = null;

    public static NpcHelper Instance => _instance ??= new NpcHelper();

    private readonly ExcelSheet<Level> levelSheet;
    private readonly ExcelSheet<ENpcResident> residentSheet;
    private readonly ExcelSheet<EObj> objSheet;
    private readonly ExcelSheet<EObjName> objNameSheet;

    private Dictionary<uint, Level> npcLevelCache = new();
    private Dictionary<uint, Level> objLevelCache = new();
    public NpcHelper()
    {
        levelSheet = Plugin.DataManager.GetExcelSheet<Level>()!;
        residentSheet = Plugin.DataManager.GetExcelSheet<ENpcResident>()!;
        objSheet = Plugin.DataManager.GetExcelSheet<EObj>()!;
        objNameSheet = Plugin.DataManager.GetExcelSheet<EObjName>()!;
    }
    
    
    public ENpcResident GetNpc(uint npcId)
    {
        if (!residentSheet.HasRow(npcId)) return residentSheet.First();
        return residentSheet.GetRow(npcId);
    }

    public EObj? GetObj(uint objId)
    {
        return objSheet.GetRow(objId);
    }

    public EObjName GetObjName(EObj obj)
    {
        return objNameSheet.GetRow(obj.RowId)!;
    }

    public EObjName GetObjName(uint objId)
    {
        return objNameSheet.GetRow(objId);
    }
    

    public Level GetNpcLevel(ENpcResident npc)
    {
        if (npcLevelCache.TryGetValue(npc.RowId, value: out var level))
        {
            return level;
        }
        npcLevelCache[npc.RowId] = levelSheet.First(l => l.Object.RowId == npc.RowId);
        return npcLevelCache[npc.RowId];
    }

    public Level GetObjLevel(EObj obj)
    {
        if (objLevelCache.TryGetValue(obj.RowId, value: out var level))
        {
            return level;
        }
        objLevelCache[obj.RowId] = levelSheet.First(l => l.Object.RowId == obj.RowId);
        return objLevelCache[obj.RowId];
    }
}
