using Lumina.Excel.Sheets;

namespace RelicBuddy.Models;

public class ShopInfo(Level? l, ENpcResident r, SpecialShop s)
{
    public Level? ShopLevel { get; init; } = l;
    public ENpcResident ShopResident { get; init; } = r;
    public SpecialShop SpecialShop { get; init; } = s;
}
