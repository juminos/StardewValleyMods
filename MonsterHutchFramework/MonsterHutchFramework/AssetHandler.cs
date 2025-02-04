using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StardewModdingAPI.Events;
using StardewValley;

namespace MonsterHutchFramework.MonsterHutchFramework;

public class AssetHandler
{
    private static Dictionary<string, MonsterHutchData>? privateData = null;
    public static Dictionary<string, MonsterHutchData> data
    {
        get
        {
            if (privateData == null)
            {
                privateData = Game1.content.Load<Dictionary<string, MonsterHutchData>>("juminos.MonsterHutchFramework/MonsterHutchData");
                ModEntry.SMonitor.Log($"Loaded asset juminos.MonsterHutchFramework/MonsterHutchData with {privateData.Count} entries.");
            }
            return privateData!;
        }
    }
    internal static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("juminos.MonsterHutchFramework/MonsterHutchData"))
        {
            // Initialize a new instance every time
            e.LoadFrom(() => new Dictionary<string, MonsterHutchData>(), AssetLoadPriority.Exclusive);
        }
    }
    internal static void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        foreach (var name in e.NamesWithoutLocale)
        {
            if (name.IsEquivalentTo("juminos.MonsterHutchFramework/MonsterHutchData"))
            {
                ModEntry.SMonitor.Log($"Asset juminos.MonsterHutchFramework/MonsterHutchData invalidated, reloading.");
                privateData = null;
            }
        }
    }
}
