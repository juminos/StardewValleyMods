using StardewValley.Monsters;

namespace MonstersTheFramework;

public class MonstersTheFrameworkAPI
{
    public Monster GetCustomMonster(string key)
    {
        var monster = new CustomMonster(key);
        return monster;
    }
}
