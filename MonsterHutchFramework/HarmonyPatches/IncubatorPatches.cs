using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley;
using Microsoft.Xna.Framework;

namespace MonsterHutchFramework.HarmonyPatches;

internal class IncubatorPatches
{
    private static ModEntry mod;

    public static void PatchAll(ModEntry monsterHutch)
    {
        mod = monsterHutch;

        var harmony = new Harmony(mod.ModManifest.UniqueID);

        harmony.Patch(
           original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.DayUpdate)),
           postfix: new HarmonyMethod(typeof(IncubatorPatches), nameof(MonsterIncubatorUpdate_Post)));
    }
    public static void MonsterIncubatorUpdate_Post(StardewValley.Object __instance)
    {
        if (__instance.QualifiedItemId == null)
        {
            return;
        }

        if (__instance.QualifiedItemId != MonsterIncubator.monsterIncubatorQID)
        {
            return;
        }

        if (__instance.MinutesUntilReady > 0 || __instance.heldObject.Value == null)
        {
            return;
        }

        GameLocation location = __instance.Location;

        if (location is SlimeHutch hutch &&
            (hutch.Name.Contains("MonsterHutchFramework") || hutch.Name.Contains("Winery")) &&
            location.canSlimeHatchHere() &&
            location.characters.Count < ModEntry.Config.HutchMonsterCapacity)
        {
            Monster? monster = null;
            Vector2 v = new Vector2((int)__instance.TileLocation.X, (int)__instance.TileLocation.Y + 1) * 64f;

            // Collect all valid monster outputs in list
            var monsterMatches = new List<MonsterHutchData>();
            foreach (var monsterData in AssetHandler.monsterHutchData)
            {
                if (__instance.heldObject.Value.QualifiedItemId == monsterData.Value.InputItemId)
                {
                    for (int i = 0; i < monsterData.Value.OutputWeight; i++)
                        monsterMatches.Add(monsterData.Value);
                }
            }
            if (monsterMatches.Count > 0)
            {
                var r = new Random();
                var matchIndex = r.Next(monsterMatches.Count);
                monster = MonsterBuilder.CreateMonster(v, monsterMatches[matchIndex]);
            }
            if (monster != null)
            {
                // Game1.showGlobalMessage(slime.cute ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12689") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12691"));
                Vector2 openSpot = Utility.recursiveFindOpenTileForCharacter(monster, location, __instance.TileLocation + new Vector2(0f, 1f), 10, allowOffMap: false);
                monster.setTilePosition((int)openSpot.X, (int)openSpot.Y);
                location.characters.Add(monster);
                __instance.ResetParentSheetIndex();
                __instance.heldObject.Value = null;
                __instance.MinutesUntilReady = -1;
            }
            else
            {
                ModEntry.SMonitor.Log($"No valid output found for {__instance.heldObject.Value.QualifiedItemId} in monster data", LogLevel.Error);
            }
        }
        else
        {
            __instance.MinutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
            __instance.readyForHarvest.Value = false;
        }
    }
}
