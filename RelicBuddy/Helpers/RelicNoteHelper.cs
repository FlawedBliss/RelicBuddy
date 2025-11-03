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
        var note = RelicNote.Instance();
        if (note is null) return null;
        return relicNoteSheet.GetRow(note->RelicNoteId);
    }
}
