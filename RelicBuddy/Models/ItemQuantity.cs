using System.Collections.Generic;
using Newtonsoft.Json;

namespace RelicBuddy.Models;

public class ItemQuantity
{
    public ItemQuantity()
    {
    }

    public ItemQuantity(uint itemId, uint quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }

    [JsonProperty("item_id")]
    public uint ItemId { get; set; }
    [JsonProperty("quantity")]
    public uint Quantity { get; set; }

    [JsonProperty("hints")]
    public List<string> Hints { get; set; } = [];
}
