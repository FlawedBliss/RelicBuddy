using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace RelicBuddy;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public int SelectedJob { get; set; } = 0;

    public string SelectedExpansion = "ARR";
    
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
