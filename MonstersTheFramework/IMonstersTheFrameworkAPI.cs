using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace MonstersTheFramework;

public interface IMonstersTheFrameworkAPI
{
    public IEnumerable<IMonsterType> GetAllMonsters();
    public IEnumerable<(string Id, IMonsterType Data)> GetData();
    public bool TryGetMonster(Monster monster, [NotNullWhen(true)] out IMonsterType? customMonster,
    [NotNullWhen(true)] out string? id);
    public bool TryGetCustomMonster(Monster monster, [NotNullWhen(true)] out IMonsterType? customMonster);
    public bool TryGetCustomMonster(
        Monster monster,
        [NotNullWhen(true)] out IMonsterType? customMonster,
        [NotNullWhen(true)] out string? id);
    public interface IMonsterType
    {
        public string Name { get; }
        public string CorrespondingMonsterGoal { get; }

    }
}
