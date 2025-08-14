using System;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using RelicBuddy.Helpers;
using RelicBuddy.Helpers.FGui;
using RelicBuddy.Helpers.Segment;

namespace RelicBuddy.Windows;

public class TextWindow : Window, IDisposable
{
    public TextWindow(Plugin plugin) : base("TextWindow##rb")
    {
        this.Plugin = plugin;
    }

    public Plugin Plugin { get; set; }

    public override void Draw()
    {
        var px = ImGui.GetCursorPosX();
        
        ImGui.Columns(2, "columns", true);
        new WrappedTextSegment("aaa bbb c dddd eeee lorem impsum blabla").Draw();
        ImGui.NextColumn();
        new ItemSegment(16064).Draw();
        ImGui.Text($"col B: {ImGui.GetColumnOffset()}");
        
    }

    public void Dispose()
    {
    }
}
