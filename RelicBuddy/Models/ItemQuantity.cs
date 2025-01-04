using System.Collections.Generic;
using Newtonsoft.Json;

namespace RelicBuddy.Models;

public class ItemQuantity
{
    [JsonProperty("item_id")]
    public uint ItemId { get; set; }
    [JsonProperty("quantity")]
    public uint Quantity { get; set; }

    [JsonProperty("hint")]
    public List<string> Hints { get; set; } = [];
}
