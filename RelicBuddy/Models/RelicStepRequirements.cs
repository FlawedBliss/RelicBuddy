using System.Collections.Generic;
using Newtonsoft.Json;

namespace RelicBuddy.Models;

public class RelicStepRequirements
{
    [JsonProperty("item")]
    public List<ItemQuantity> Item { get; set; } = [];
}
