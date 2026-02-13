using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel;

namespace RelicBuddy.Helpers;

public class RelicNoteHelper
{
    private static RelicNoteHelper? _instance = null;

    public static RelicNoteHelper Instance => _instance ??= new RelicNoteHelper();
    
    private ExcelSheet<Lumina.Excel.Sheets.RelicNote> relicNoteSheet;
    private RelicNoteHelper()
    {
        relicNoteSheet = Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.RelicNote>();
    }


    public unsafe Lumina.Excel.Sheets.RelicNote? GetCurrentNoteData()
    {
        if (RelicNote.Instance() is null) return null;
        return relicNoteSheet.GetRow(RelicNote.Instance()->RelicNoteId);
    }

    public unsafe bool IsDutyComplete(int dutyId)
    {
        return RelicNote.Instance() is not null && RelicNote.Instance()->IsDungeonComplete(dutyId);
    }

    public unsafe int GetMonsterProgress(int index)
    {
        return RelicNote.Instance() is null ? 0 : RelicNote.Instance()->GetMonsterProgress(index);
    }

    public unsafe bool IsFateComplete(int index)
    {
        return RelicNote.Instance() is not null && RelicNote.Instance()->IsFateComplete(index);
    }
}
