using Dalamud.Bindings.ImGui;

namespace RelicBuddy.Helpers.Segment;

public class BulletSegment : BaseSegment
{
    public override float CalcWidth()
    {
        return 16; //dont know the actual size of an imgui bullet, 16 should be fine here, bullets should usually be left-aligned anyway
    }

    public override void Draw()
    {
        ImGui.Bullet();
    }
}
