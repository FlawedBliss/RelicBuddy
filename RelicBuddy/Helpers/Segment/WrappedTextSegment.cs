using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Dalamud.Bindings.ImGui;

namespace RelicBuddy.Helpers.Segment;

public class WrappedTextSegment : BaseSegment
{
    public WrappedTextSegment(string text)
    {
        Text = text;
    }

    private string Text { init; get; }
    
    public override float CalcWidth()
    {
        //only calculating the minimum needed width, that is until the first space when a wrap may happen
        return ImGui.CalcTextSize(Text[(Text.Contains(' ') ? Text.IndexOf(' ') : 0 )..]).X;
    }
    
    public override void Draw()
    {
        
        var textRemaining = Text;
        do
        {
            var textToDraw = textRemaining.Trim();
            while (ImGui.CalcTextSize(textToDraw).X > GetAvailableSpace())
            {
                if (textToDraw.LastIndexOf(' ') == -1)
                {
                    break;
                }
                textToDraw = textToDraw[..textToDraw.LastIndexOf(' ')];
            }
            textRemaining = textRemaining.Length > textToDraw.Length ? textRemaining[(textToDraw.Length+1)..] : "";
            ImGui.TextUnformatted(textToDraw);
        }
        while (textRemaining.Length > 0);
    }
}
