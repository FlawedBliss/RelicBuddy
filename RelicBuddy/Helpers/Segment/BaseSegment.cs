using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;

namespace RelicBuddy.Helpers;

public abstract class BaseSegment
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public abstract float CalcWidth();

    public abstract void Draw();
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected float GetAvailableSpace()
    {
        return ImGui.GetContentRegionAvail().X;
    }
}
