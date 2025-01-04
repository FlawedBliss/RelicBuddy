using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Textures;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using RelicBuddy.Models;

namespace RelicBuddy.Helpers;

public class ShopHelper
{
    private static ShopHelper? _instance = null;

    public static ShopHelper Instance => _instance ??= new ShopHelper();
    private readonly NpcHelper NpcHelper = NpcHelper.Instance;

    private ExcelSheet<ENpcBase> npcSheet;
    private ExcelSheet<SpecialShop> shopSheet;
    private ExcelSheet<Level> levelSheet;
    
    private static Dictionary<uint, uint> tomeMap = new() {
        { 1, 28 },
        { 2, 46 },
        { 3, 47 },
    };

    private Dictionary<uint, List<SpecialShop>> itemShopCache = new();
    private Dictionary<uint, List<ENpcBase>> shopNpcCache = new();
    private Dictionary<uint, Level> npcLevelCache = new();
    private Dictionary<uint, List<ShopInfo>> itemNpcCache = new();
    
    //shops for which no npc was found in previous iterations
    private List<uint> invalidShops = [];

    private ShopHelper()
    {
        npcSheet = Plugin.DataManager.GetExcelSheet<ENpcBase>()!;
        shopSheet = Plugin.DataManager.GetExcelSheet<SpecialShop>()!;
        levelSheet = Plugin.DataManager.GetExcelSheet<Level>()!;
    }

    public List<SpecialShop> GetShopsForItem(uint itemId)
    {
        if (itemShopCache.TryGetValue(itemId, out var forItem))
        {
            return forItem;
        }

        List<SpecialShop> shops = [];
        foreach (var shop in shopSheet)
        {
            foreach (var item in shop.Item)
            {
                foreach (var item2 in item.ReceiveItems)
                {
                    if (item2.Item.RowId == itemId)
                    {
                        shops.Add(shop);
                    }
                }
            }
        }

        itemShopCache[itemId] = shops;
        return shops;
    }
    
    public List<ENpcBase> GetShopNpcs(params uint[] shopIds)
    {
        List<uint> shops = [..shopIds];
        shops.RemoveAll(e => invalidShops.Contains(e));
        List<ENpcBase> npcs = [];
        foreach (var shop in shops.ToList())
        {
            if (shopNpcCache.TryGetValue(shop, out var value))
            {
                npcs.AddRange(value);
                shops.Remove(shop);
            }
        }

        if (shops.Count > 0)
        {
            foreach (var shop in shops)
            {
                foreach (var npc in npcs)
                {
                    foreach (var data in npc.ENpcData) 
                    {
                        if (shop == data.RowId)
                        {
                            if (!shopNpcCache.TryGetValue(shop, out var value))
                            {
                                value = [];
                                shopNpcCache[shop] = value;
                            }
                            value.Add(npc);
                            npcs.Add(npc);
                        }
                    }
                }
            }
        }

        if (shops.Count > 0)
        {
            Plugin.PluginLog.Info($"No npc found for shops {string.Join(",", shops)}");
            invalidShops.AddRange(shops);
        }
        return npcs;
    }

    public Level? GetNpcLocation(uint npc)
    {
        if (npcLevelCache.TryGetValue(npc, out var fromLevel))
        {
            return fromLevel;
        }
        foreach (var level in levelSheet)
        {
            if (level.Object.RowId == npc)
            {
                npcLevelCache[npc] = level;
                return level;
            }
        }

        return null;
    }

    public ISharedImmediateTexture GetCurrencyTypeIcon(byte id)
    {
        return id switch
        {
            4 => Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(26286)),
            _ => Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(60412))
        };
    }

    public ShopInfo? GetShopInfoForItem(uint itemId)
    {
        var shop = GetShopsForItem(itemId);
        if (shop.Count == 0) return null;
        foreach(var npc in GetShopNpcs(shop.First().RowId))
        {
            var level = GetNpcLocation(npc.RowId);
            return new ShopInfo(level, NpcHelper.GetNpc(npc.RowId), shop.First());
            
        }
        return null;
    }

    public List<ENpcBase> GetNpcsWithShop(uint shop)
    {
        if (shopNpcCache.TryGetValue(shop, out var cached))
        {
            return cached;
        }

        var npcs = npcSheet.Where(npc => npc.ENpcData.Any(data => data.RowId == shop)).ToList();
        shopNpcCache[shop] = npcs;
        return npcs;
    }

    public List<SpecialShop> GetShopsWithItem(uint itemId)
    {
        if (itemShopCache.TryGetValue(itemId, out var cached))
        {
            return cached;
        }
        var shops = shopSheet.Where(shop => shop.Item.Any(data => data.ReceiveItems.Any(i => i.Item.RowId == itemId))).ToList();
        itemShopCache[itemId] = shops;
        return shops;
    }

    public List<ShopInfo> GetNpcsWithItem(uint itemId)
    {
        if (itemNpcCache.TryGetValue(itemId, out var cached))
        {
            return cached;
        }
        var shops = GetShopsWithItem(itemId);
        List<ShopInfo> npcs = [];
        foreach (var shop in shops)
        {
            foreach (var npc in GetNpcsWithShop(shop.RowId))
            {
                npcs.Add(new ShopInfo(GetNpcLocation(npc.RowId), NpcHelper.GetNpc(npc.RowId), shop));
            }
        }

        itemNpcCache[itemId] = npcs;
        return npcs;
    }

    public ShopInfo? GetFirstShopForItem(uint itemId)
    {
        var shop = GetShopsWithItem(itemId);
        if (shop.Count == 0) return null;
        foreach(var npc in GetNpcsWithShop(shop.First().RowId))
        {
            var level = GetNpcLocation(npc.RowId);
            return new ShopInfo(level, NpcHelper.GetNpc(npc.RowId), shop.First());
            
        }
        return null;
    }

    public uint ConvertCurrency(uint itemId, SpecialShop shop)
    {
        if (shop.UseCurrencyType != 4 || itemId > 3) return itemId;
        return tomeMap[itemId];
    }
}
