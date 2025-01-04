using System.Collections.Generic;
using Newtonsoft.Json;

namespace RelicBuddy.Models;

public class RelicData
{
    [JsonProperty("expansion")]
    public string Expansion { get; set; } = "N/A";

    [JsonProperty("name")]
    public string Name { get; set; } = "N/A";

    [JsonProperty("relics")]
    public Dictionary<string, RelicWeapon> Relics { get; set; } = [];

    [JsonProperty("steps")]
    public List<RelicStep> Steps { get; set; } = [];
}
