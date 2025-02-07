using System.ComponentModel.Design;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace RelicBuddy.Helpers;

public class DutyHelper
{
    private static DutyHelper? _instance = null;

    public static DutyHelper Instance => _instance ??= new DutyHelper();

    private ExcelSheet<ContentFinderCondition> contentSheet;
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
}
