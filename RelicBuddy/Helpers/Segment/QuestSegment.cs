using System.Runtime.InteropServices.JavaScript;
using Dalamud.Interface.Textures;
using Dalamud.Bindings.ImGui;

namespace RelicBuddy.Helpers.Segment;

public class QuestSegment : BaseSegment
{
    private readonly uint questIcon = 0;
    private readonly uint iconSize = 16;
    private readonly uint questId;
    private WrappedTextSegment questNameSegment;
    
    private readonly QuestHelper QuestHelper = QuestHelper.Instance;

    public QuestSegment(uint questId)
    {
        this.questId = questId;
        questNameSegment = new(QuestHelper.GetQuestName(questId));

    }

    public override float CalcWidth()
    {
        return questNameSegment.CalcWidth() + iconSize + 2;
    }

    public override void Draw()
    {
        ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(Icons.QuestSquare)).GetWrapOrEmpty().Handle, new(iconSize, iconSize));
        ImGui.SameLine();
        questNameSegment.Draw();
    }
}
