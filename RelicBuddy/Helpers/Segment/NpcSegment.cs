using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Bindings.ImGui;

namespace RelicBuddy.Helpers.Segment;

public class NpcSegment : BaseSegment
{
    private readonly NpcHelper NpcHelper = NpcHelper.Instance;
    private readonly MapHelper MapHelper = MapHelper.Instance;
    
    private readonly uint iconSize = 16;
    private readonly Vector4 textColor = ImGuiColors.ParsedGold;

    private readonly WrappedTextSegment npcNameSegment;
    
    private uint NpcId { get; init; }
    
    public NpcSegment(uint npcId)
    {
        NpcId = npcId;
        npcNameSegment = new WrappedTextSegment(NpcHelper.GetNpc(npcId).Singular.ExtractText());
    }
    
    public override float CalcWidth()
    {
        return npcNameSegment.CalcWidth() + iconSize + 2; //add 2 as padding
    }

    public override void Draw()
    {
        if (GetAvailableSpace() < CalcWidth())
            ImGui.NewLine();
        var level = NpcHelper.GetNpcLevel(NpcHelper.GetNpc(NpcId));
        if(level is not null) {
            ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(60453)).GetWrapOrEmpty().Handle, new Vector2(iconSize, iconSize));
            if(ImGui.IsItemHovered())
            {
                ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            }
            if (ImGui.IsItemClicked())
            {
                MapHelper.ShowFlag(level.Value);
            }
            ImGui.SameLine();
        }
        ImGui.PushStyleColor(ImGuiCol.Text, textColor);
        npcNameSegment.Draw();
        ImGui.PopStyleColor();
    }
}
