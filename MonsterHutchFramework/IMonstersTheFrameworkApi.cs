using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace MonsterHutchFramework;

public interface IMonstersTheFrameworkApi
{
    void SpawnCustomMonster(string key, out Monster newMonster);
    Monster GetCustomMonster(string key);
}
