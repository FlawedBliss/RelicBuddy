using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using RelicBuddy.Models;

namespace RelicBuddy.Helpers;

public class QuestHelper
{
    private static QuestHelper? _instance = null;

    public static QuestHelper Instance => _instance ??= new QuestHelper();

    private ExcelSheet<Quest> questSheet;
    private ExcelSheet<Leve> leveSheet;
    private unsafe QuestManager* questManager = QuestManager.Instance();
    private QuestHelper()
    {
        questSheet = Plugin.DataManager.GetExcelSheet<Quest>()!;
        leveSheet = Plugin.DataManager.GetExcelSheet<Leve>()!;
    }

    public string GetQuestName(uint id)
    {
        return questSheet.GetRow(id).Name.ExtractText();
    }

    public string GetLeveName(uint id)
    {
        return leveSheet.GetRow(id).Name.ExtractText();
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

    public uint GetQuestIdForStep(RelicStep step, string? job)
    {
        uint questId = 0;
        if (step.QuestIdFirst is not null)
        {
            if (QuestManager.IsQuestComplete(step.QuestIdFirst.Value))
            {
                questId = step.QuestIdRepeating!.Value;
            }
            else
            {
                questId = step.QuestIdFirst.Value;
            }
        } else if (step.QuestIdJob is not null)
        {
            if (job is null)
            {
                Plugin.PluginLog.Warning($"GetQuestIdForStep called with null job for step {step.QuestIdJob}");
                return questId;
            }
            if (step.QuestIdJob.TryGetValue(job, out var jobQuestId))
            {
                questId = jobQuestId;
            }
        }

        return questId;
    }
}
