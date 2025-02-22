using StardewValley;

namespace MonstersTheFramework
{
    public static class Extensions
    {
        public static bool HasEnchantment(this Tool tool, string type)
        {
            foreach (var ench in tool.enchantments)
            {
                if (ench.GetName() == type)
                    return true;
            }
            return false;
        }
    }
}
