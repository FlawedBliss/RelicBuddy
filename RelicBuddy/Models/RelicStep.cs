using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RelicBuddy.Models;

public class RelicStep
{
    [JsonProperty("is_one_time")]
    public bool IsOneTime { get; set; }

    [JsonProperty("quest_ids")]
    public List<QuestId> QuestIds = [];
    
    [JsonProperty("quest_id_first")]
    public uint? QuestIdFirst { get; set; }
    [JsonProperty("quest_id_repeating")]
    public uint? QuestIdRepeating { get; set; }
    [JsonProperty("prerequisites")]
    public RelicStepPrerequisites Prerequsites { get; set; } = new();

    [JsonProperty("requirements")]
    public RelicStepRequirements? Requirements { get; set; } = new();
    
    [JsonProperty("hints")]
    public string[]? Hints { get; set; } = null;

    [JsonProperty("quest_id_job")]
    public Dictionary<String, uint>? QuestIdJob { get; set; } = null;

    [JsonProperty("npcs")]
    public uint[] Npcs = [];
    [JsonProperty("objects")]
    public uint[] Objects = [];
}
