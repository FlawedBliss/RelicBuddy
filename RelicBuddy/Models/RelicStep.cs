using System.Collections.Generic;
using Newtonsoft.Json;

namespace RelicBuddy.Models;

public class RelicStep
{
    [JsonProperty("is_one_time")]
    public bool IsOneTime { get; set; }
    [JsonProperty("quest_id_first")]
    public uint QuestIdFirst { get; set; }
    [JsonProperty("quest_id_repeating")]
    public uint QuestIdRepeating { get; set; }
    [JsonProperty("prerequisites")]
    public RelicStepPrerequisites Prerequsites { get; set; } = new();

    [JsonProperty("requirements")]
    public RelicStepRequirements? Requirements { get; set; } = new();

    [JsonProperty("hint")]
    public string? Hint { get; set; } = null;
    
    [JsonProperty("hints")]
    public string[]? Hints { get; set; } = null;

    [JsonProperty("npc")]
    public uint? Npc = null;
    [JsonProperty("object")]
    public uint? Object = null;
}
