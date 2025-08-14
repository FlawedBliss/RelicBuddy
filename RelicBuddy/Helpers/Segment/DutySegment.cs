using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;

namespace RelicBuddy.Helpers.Segment;

public class DutySegment : BaseSegment
{

    private uint dutyId;
    private ContentFinderCondition condition;
    private uint iconSize = 16;
    private readonly WrappedTextSegment dutyNameSegment;
    public DutySegment(uint dutyId)
    {
        this.dutyId = dutyId;
        dutyNameSegment = new WrappedTextSegment(DutyHelper.Instance.GetDutyName(dutyId));
        condition = DutyHelper.Instance.GetContentFinderCondition(dutyId);
    }
    public override float CalcWidth()
    {
        return iconSize + dutyNameSegment.CalcWidth() + 2;
    }

    public override void Draw()
    {
        ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(condition.ContentType.Value.Icon)).GetWrapOrDefault()!.Handle, new(iconSize, iconSize));
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        if (ImGui.IsItemClicked())
        {
            DutyHelper.Instance.OpenDutyFinder(dutyId);
        }

        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedOrange);
        dutyNameSegment.Draw();
        ImGui.PopStyleColor();
    }
}
