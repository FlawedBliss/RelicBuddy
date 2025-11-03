using System.Collections.Generic;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace RelicBuddy.Helpers;

public class DutyHelper
{
    private static DutyHelper? _instance = null;

    public static DutyHelper Instance => _instance ??= new DutyHelper();

    private ExcelSheet<ContentFinderCondition> contentSheet;

    private Dictionary<string, uint?> nameToIdMap = new();
    
    private DutyHelper()
    {
        contentSheet = Plugin.DataManager.GetExcelSheet<ContentFinderCondition>();
    }

    public string GetDutyName(uint dutyId)
    {
        return contentSheet.GetRow(dutyId).Name.ExtractText();
    }

    public unsafe void OpenDutyFinder(uint dutyId)
    {
        if (IsInDuty || AgentContentsFinder.Instance() is null ) return;
        AgentContentsFinder.Instance()->OpenRegularDuty(dutyId);
    }

    public bool IsInDuty =>
        Plugin.ClientState.IsLoggedIn &&
        Plugin.Condition.Any(ConditionFlag.BoundByDuty, ConditionFlag.BoundByDuty56,
                             ConditionFlag.BoundByDuty95);

    public ContentFinderCondition GetContentFinderCondition(uint dutyId)
    {
        return contentSheet.GetRow(dutyId);
    }
    
    public ContentFinderCondition? GetContentFinderConditionByName(string name)
    {
        if (nameToIdMap.TryGetValue(name, out var value))
        {

            return contentSheet.GetRowOrDefault(value ?? 0);
        }
        // some duties have "the" in the content finder condition and the place name.
        // some duties have "the" in the content finder condition but not in the place name.
        // some duties have no "the" in the content finder condition but have it in the place name.
        // sigh
        var nameToSearch = name.ToLowerInvariant().Replace("the ", "");
        var row = contentSheet.FirstOrNull(entry => entry.Name.ExtractText().Replace("the ", "").ToLowerInvariant().Equals(nameToSearch));
        if (row is null)
        {
            Plugin.PluginLog.Warning($"GetContentFinderConditionByName: No duty found with name '{nameToSearch}'");
            nameToIdMap[name] = null;
            return null;
        }
        nameToIdMap[name] = row.Value.RowId;
        return row;
    }
}
