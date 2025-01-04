using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace RelicBuddy.Helpers;

public class ItemHelper
{
    private ExcelSheet<Item> itemSheet;

    private static ItemHelper? _instance = null;

    public static ItemHelper Instance => _instance ??= new ItemHelper();

    private ItemHelper()
    {
        itemSheet = Plugin.DataManager.GetExcelSheet<Item>()!;
    }

    public string GetItemName(uint itemId)
    {
        return itemSheet.GetRow(itemId).Name.ExtractText();
    }

    public IDalamudTextureWrap GetItemIcon(uint itemId)
    {
        //default icon 61221
        return Plugin.TextureProvider.GetFromGameIcon(new GameIconLookup(itemSheet.GetRow(itemId).Icon)).GetWrapOrEmpty();
    }
}
