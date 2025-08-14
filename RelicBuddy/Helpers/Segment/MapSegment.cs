using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Textures;
using Dalamud.Bindings.ImGui;

namespace RelicBuddy.Helpers.Segment;

public class MapSegment : BaseSegment
{
    private readonly MapHelper MapHelper = MapHelper.Instance;
    
    private readonly uint iconSize = 16;
    private readonly Vector4 textColor = ImGuiColors.ParsedOrange;

    private readonly WrappedTextSegment mapNameSegment;
    
    private uint MapId { get; init; }
    
    public MapSegment(uint mapId)
    {
        MapId = mapId;
        mapNameSegment = new WrappedTextSegment(MapHelper.GetMapName(MapId));
    }
    
    public override float CalcWidth()
    {
        return mapNameSegment.CalcWidth() + iconSize + 2; //add 2 as padding
    }

    public override void Draw()
    {
        if (GetAvailableSpace() < CalcWidth())
            ImGui.NewLine();
        ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(60453)).GetWrapOrEmpty().Handle, new Vector2(iconSize, iconSize));
        if(ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
        }
        if (ImGui.IsItemClicked())
        {
            MapHelper.OpenMap(MapId);
        }
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, textColor);
        mapNameSegment.Draw();
        ImGui.PopStyleColor();
    }
}
