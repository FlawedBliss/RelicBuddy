using Dalamud.Interface.Textures;
using ImGuiNET;

namespace RelicBuddy.Helpers.Segment;

public class ObjectSegment : BaseSegment
{
    private readonly uint objectId;
    private readonly uint iconSize = 16;
    private readonly NpcHelper NpcHelper = NpcHelper.Instance;
    private readonly WrappedTextSegment objectNameSegment;
    
    public ObjectSegment(uint objectId)
    {
        this.objectId = objectId;
        objectNameSegment = new(NpcHelper.GetObjName(objectId).Singular.ExtractText());
    }


    public override float CalcWidth()
    {
        return objectNameSegment.CalcWidth() + iconSize + 2;
    }

    public override void Draw()
    {
        ImGui.Image(Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(Icons.QuestSquare)).GetWrapOrEmpty().ImGuiHandle, new(iconSize, iconSize));
        ImGui.SameLine();
        objectNameSegment.Draw();
    }
}
