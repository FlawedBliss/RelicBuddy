using System.Runtime.CompilerServices;
using ImGuiNET;

namespace RelicBuddy.Helpers;

public abstract class BaseSegment
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract float CalcWidth();

    public abstract void Draw();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected float GetAvailableSpace()
    {
        return ImGui.GetColumnWidth() - (ImGui.GetCursorPosX() - ImGui.GetColumnOffset()) - 48; // adding a -48 margin on the right avoids some funny text wrapping issues. it does not fix the issue, but it *does* make it disappear
    }
}
