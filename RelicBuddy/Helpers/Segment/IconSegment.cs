using Dalamud.Interface.Textures;
using ImGuiNET;

namespace RelicBuddy.Helpers.Segment;

public class IconSegment : BaseSegment
{
    private uint iconId;
    private readonly int iconSize = 16;

    public IconSegment(uint iconId)
    {
        this.iconId = iconId;
    }
    
    public override float CalcWidth()
    {
        return iconSize;
    }

    public override void Draw()
    {
        ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(iconId)).GetWrapOrEmpty().ImGuiHandle, new(iconSize, iconSize));
    }
}
