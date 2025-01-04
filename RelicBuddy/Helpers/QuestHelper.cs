using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace RelicBuddy.Helpers;

public class QuestHelper
{
    private static QuestHelper? _instance = null;

    public static QuestHelper Instance => _instance ??= new QuestHelper();

    private ExcelSheet<Quest> questSheet;
    private unsafe QuestManager* questManager = QuestManager.Instance();
    private QuestHelper()
    {
        questSheet = Plugin.DataManager.GetExcelSheet<Quest>()!;
    }

    public string GetQuestName(uint id)
    {
        return questSheet.GetRow(id).Name.ExtractText();
    }

    public Level GetQuestLocation(uint id)
    {
        return questSheet.GetRow(id).IssuerLocation.Value;
    }

    public bool IsQuestCompleted(ushort id)
    {
        return QuestManager.IsQuestComplete(id);
    }

    public unsafe bool IsQuestAccepted(uint id)
    {
        return questManager->IsQuestAccepted(id);
    }
}
