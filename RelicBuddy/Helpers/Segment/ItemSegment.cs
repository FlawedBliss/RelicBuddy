using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Common.Math;
using Dalamud.Bindings.ImGui;

namespace RelicBuddy.Helpers.Segment;

public class ItemSegment : BaseSegment
{
    
    private readonly ItemHelper ItemHelper = ItemHelper.Instance;
    
    private readonly uint iconSize = 16;
    private readonly Vector4 textColor = ImGuiColors.ParsedBlue;

    private readonly WrappedTextSegment itemNameSegment;
    
    private uint ItemId { get; init; }
    
    public ItemSegment(uint itemId)
    {
        ItemId = itemId;
        itemNameSegment = new WrappedTextSegment(ItemHelper.GetItemName(ItemId));
    }
    
    public override float CalcWidth()
    {
        return itemNameSegment.CalcWidth() + iconSize + 2; //add 2 as padding
    }

    public override void Draw()
    {
        if (GetAvailableSpace() < CalcWidth())
            ImGui.NewLine();
        ImGui.Image(ItemHelper.GetItemIcon(ItemId).Handle, new Vector2(iconSize, iconSize));
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, textColor);
        itemNameSegment.Draw();
        ImGui.PopStyleColor();
    }
}
