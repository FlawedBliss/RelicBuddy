using System.Collections.Generic;
using Newtonsoft.Json;

namespace RelicBuddy.Models;

public class RelicWeapon
{
    // [JsonProperty("job")]
    // public string Job { get; set; } = "N/A";

    [JsonProperty("item_ids")]
    public IList<uint> ItemIds { get; set; } = [];
}
