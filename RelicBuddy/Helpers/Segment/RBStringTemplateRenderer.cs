using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Plugin.Internal.Types.Manifest;
using ImGuiNET;
using RelicBuddy.Helpers.Segment;

namespace RelicBuddy.Helpers.Strings;

public partial class RBStringTemplateRenderer
{
    private List<BaseSegment> segments = [];
    
    private const string pattern = @"\{([^}]+):([^}]+)\}";
    [GeneratedRegex(pattern)]
    private static partial Regex ReferenceRegex();

    private bool logThis = false;
    public void ProcessTemplate(string str)
    {
        if (str.StartsWith("You need to acquire"))
            logThis = true;
        var matches = ReferenceRegex().Matches(str);
        if (matches.Count == 0)
        {
            segments.Add(new WrappedTextSegment(str));
            return;
        }
        
        for (var i = 0; i < matches.Count; i++)
        {
            // add string from before match to collection
            var from = i == 0 ? 0 : matches[i - 1].Index + matches[i - 1].Length + 1;
            var strBefore = str.Substring(from, matches[i].Index - from);
            if (strBefore.Trim().Length > 0)
            {
                segments.Add(new WrappedTextSegment(strBefore.Trim()));
            }
            var type = matches[i].Groups[1].Value;
            var value = matches[i].Groups[2].Value;
            switch (type)
            {
                case "npc":
                    segments.Add(new NpcSegment(uint.Parse(value)));
                    break;
                case "map":
                    segments.Add(new MapSegment(uint.Parse(value)));
                    break;
                case "item":
                    segments.Add(new ItemSegment(uint.Parse(value)));
                    break;
                case "icon":
                    segments.Add(new IconSegment(uint.Parse(value)));
                    break;
                case "bullet":
                    segments.Add(new BulletSegment());
                    break;
                case "quest":
                    segments.Add(new QuestSegment(uint.Parse(value)));
                    break;
                case "object":
                    segments.Add(new ObjectSegment(uint.Parse(value)));
                    break;
                default:
                    Plugin.PluginLog.Warning($"Missing renderer for template type: {type}");
                    segments.Add(new WrappedTextSegment($"unknown type: {type}"));
                    break;
            }

            if (i == matches.Count - 1)
            {
                segments.Add(new WrappedTextSegment(str[(matches[i].Index + matches[i].Length)..].Trim()));
            }
        }
    }

    public void Render()
    {
        for (var index = 0; index < segments.Count; index++)
        {
            var segment = segments[index];
            segment.Draw();
            if(index < segments.Count-1) ImGui.SameLine();
        }

    }
}
