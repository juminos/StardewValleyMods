using StardewModdingAPI.Events;
using StardewValley;

namespace MonsterHutchFramework;

public class AssetHandler
{
    private static Dictionary<string, MonsterHutchData>? privateMonsterHutchData = null;
    private static Dictionary<string, CharmerRingData>? privateCharmerRingData = null;
    public static Dictionary<string, MonsterHutchData> monsterHutchData
    {
        get
        {
            if (privateMonsterHutchData == null)
            {
                privateMonsterHutchData = Game1.content.Load<Dictionary<string, MonsterHutchData>>($"{ModEntry.Mod?.ModManifest?.UniqueID}/MonsterHutchData");
                ModEntry.SMonitor.Log($"Loaded asset {ModEntry.Mod?.ModManifest?.UniqueID}/MonsterHutchData with {privateMonsterHutchData.Count} entries.");
            }
            return privateMonsterHutchData!;
        }
    }
    public static Dictionary<string, CharmerRingData> charmerRingData
    {
        get
        {
            if (privateCharmerRingData == null)
            {
                privateCharmerRingData = Game1.content.Load<Dictionary<string, CharmerRingData>>($"{ModEntry.Mod?.ModManifest?.UniqueID}/CharmerRingData");
                ModEntry.SMonitor.Log($"Loaded asset {ModEntry.Mod?.ModManifest?.UniqueID}/CharmerRingData with {privateCharmerRingData.Count} entries.");
            }
            return privateCharmerRingData!;
        }
    }
    internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo($"{ModEntry.Mod?.ModManifest?.UniqueID}/MonsterHutchData"))
        {
            // Initialize a new instance every time
            e.LoadFrom(() => new Dictionary<string, MonsterHutchData>(), AssetLoadPriority.Exclusive);
        }
        if (e.NameWithoutLocale.IsEquivalentTo($"{ModEntry.Mod?.ModManifest?.UniqueID}/CharmerRingData"))
            // Initialize a new instance every time
            e.LoadFrom(() => new Dictionary<string, CharmerRingData>(), AssetLoadPriority.Exclusive);
    }
    internal static void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        foreach (var name in e.NamesWithoutLocale)
        {
            if (name.IsEquivalentTo($"{ModEntry.Mod?.ModManifest?.UniqueID}/MonsterHutchData"))
            {
                ModEntry.SMonitor.Log($"Asset {ModEntry.Mod?.ModManifest?.UniqueID}/MonsterHutchData invalidated, reloading.");
                privateMonsterHutchData = null;
            }
            if (name.IsEquivalentTo($"{ModEntry.Mod?.ModManifest?.UniqueID}/CharmerRingData"))
            {
                ModEntry.SMonitor.Log($"Asset {ModEntry.Mod?.ModManifest?.UniqueID}/CharmerRingData invalidated, reloading.");
                privateCharmerRingData = null;
            }
        }
    }
}
