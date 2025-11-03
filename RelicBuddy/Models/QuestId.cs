using Newtonsoft.Json;

namespace RelicBuddy.Models;

public class QuestId
{
    [JsonProperty("id")]
    public uint Id;
    [JsonProperty("first_time_only")]
    public bool FirstTimeOnly = false;
    [JsonProperty("repeat_only")]
    public bool RepeatOnly = false;
}
