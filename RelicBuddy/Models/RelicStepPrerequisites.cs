using System.Collections.Generic;
using Newtonsoft.Json;

namespace RelicBuddy.Models;

public class RelicStepPrerequisites
{
    [JsonProperty("quests")]
    public List<uint> Quests { get; set; } = [];
}
