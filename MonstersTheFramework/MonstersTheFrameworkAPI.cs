using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace MonstersTheFramework;

public sealed class MonstersTheFrameworkAPI(IModHelper helper)
{
    public IEnumerable<IMonstersTheFrameworkAPI.IMonsterType> GetAllMonsters() =>
        helper.GameContent.Load<Dictionary<string, MonsterType>>("spacechase0.MonstersTheFramework/Monsters").Values;
    public IEnumerable<(string Id, IMonstersTheFrameworkAPI.IMonsterType Data)> GetData() =>
        this.GetAllMonsters().Select(static data => (data.Name, (IMonstersTheFrameworkAPI.IMonsterType)data));
    public bool TryGetMonster(Monster monster, [NotNullWhen(true)] out IMonstersTheFrameworkAPI.IMonsterType? customMonster,
    [NotNullWhen(true)] out string? id)
    {
        customMonster = null;
        if (!monster.modData.TryGetValue("MonstersTheFramework.ID", out id))
        {
            return false;
        }

        var data = helper.GameContent.Load<Dictionary<string, MonsterType>>("spacechase0.MonstersTheFramework/Monsters");
        if (!data.TryGetValue(id, out var monsterData))
        {
            return false;
        }

        customMonster = monsterData;
        return true;
    }
    public bool TryGetCustomMonster(Monster monster, [NotNullWhen(true)] out IMonstersTheFrameworkAPI.IMonsterType? customMonster) =>
        this.TryGetCustomMonster(monster, out customMonster, out _);
    public bool TryGetCustomMonster(
        Monster monster,
        [NotNullWhen(true)] out IMonstersTheFrameworkAPI.IMonsterType? customMonster,
        [NotNullWhen(true)] out string? id)
    {
        if (this.TryGetMonster(monster, out var customMonsterNew, out id))
        {
            customMonster = customMonsterNew;
            return true;
        }

        customMonster = null;
        return false;
    }

    //public static void SpawnCustomMonster(string key, out Monster newMonster)
    //{
    //    var customMonster = new CustomMonster(key);
    //    if (customMonster is Monster monster)
    //    {
    //        ModEntry.SMonitor.Log($"{customMonster} is a monster with name {monster.Name}", StardewModdingAPI.LogLevel.Trace);
    //        newMonster = monster;
    //    }
    //    else
    //        newMonster = null;
    //}
}
