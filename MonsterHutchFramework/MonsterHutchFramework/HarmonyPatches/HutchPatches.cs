using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;

namespace MonsterHutchFramework.MonsterHutchFramework.HarmonyPatches
{
    internal class HutchPatches
    {
        private static ModEntry mod;

        public static void PatchAll(ModEntry monsterHutch)
        {
            mod = monsterHutch;

            var harmony = new Harmony(mod.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.DayUpdate)),
               postfix: new HarmonyMethod(typeof(HutchPatches), nameof(MonsterIncubatorUpdate_Post)));

            harmony.Patch(
               original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
               prefix: new HarmonyMethod(typeof(HutchPatches), nameof(SlimeHutchDayUpdate_Pre)));

            harmony.Patch(
               original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
               postfix: new HarmonyMethod(typeof(HutchPatches), nameof(SlimeHutchDayUpdate_Post)));
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

            if (location is SlimeHutch && location.canSlimeHatchHere() && location.characters.Count < 40)
            {
                Monster monster = null;
                Vector2 v = new Vector2((int)__instance.TileLocation.X, (int)__instance.TileLocation.Y + 1) * 64f;

                foreach (var monsterData in AssetHandler.data)
                {
                    if (__instance.heldObject.Value.QualifiedItemId == monsterData.Value.InputItemId)
                    {
                        monster = ModEntry.CreateMonster(v, monsterData.Value);
                        break;
                    }
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
            }
            else
            {
                __instance.MinutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
                __instance.readyForHarvest.Value = false;
            }
        }
        public static void SlimeHutchDayUpdate_Pre(SlimeHutch __instance, ref List<Monster> __state)
        {
            // we collect and remove the non slimes, so we can run the base implementation for the slimes
            __state = new List<Monster>();
            for (int i = __instance.characters.Count - 1; i >= 0; i--)
            {
                if (__instance.characters[i] is Monster monster)
                {
                    __state.Add(monster);
                    __instance.characters.RemoveAt(i);
                }
            }
            int startIndex = Game1.random.Next(__instance.waterSpots.Length);
            float usedWater = 0f;
            foreach (var monsterType in AssetHandler.data)
            {
                int monsterCount = 0;
                foreach (var monster in __state)
                {
                    if (monsterType.Value.Name == monster.Name)
                    {
                        monsterCount++;
                    }
                }
                if (monsterCount > 0)
                {
                    int monstersWatered = 0;

                    for (int i = 0; i < __instance.waterSpots.Length; i++)
                    {
                        if (__instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length])
                        {
                            if (monstersWatered < monsterCount)
                            {
                                monstersWatered += Math.Min(monsterType.Value.NumberWatered, monsterCount - monstersWatered);
                            }
                        }
                    }
                    usedWater += (float)monstersWatered / (float)monsterType.Value.NumberWatered;

                    for (int j = 0; j < monstersWatered; j++)
                    {
                        int tries = 50;
                        Vector2 tile = __instance.getRandomTile();
                        while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                        {
                            tile = __instance.getRandomTile();
                            tries--;
                        }

                        if (tries > 0)
                        {
                            var produceChance = Math.Clamp(monsterType.Value.ProduceChance + Game1.player.DailyLuck, 0, 1);
                            if (Game1.random.NextDouble() < produceChance)
                            {
                                bool spawn_object = true;
                                bool dropDeluxe = false;

                                int randomProduceIndex = Game1.random.Next(0, monsterType.Value.ProduceData.Count);
                                var randomProduceId = monsterType.Value.ProduceData[randomProduceIndex].ItemId;
                                if (Game1.random.NextDouble() < Math.Clamp(monsterType.Value.DeluxeChance + Game1.player.DailyLuck, 0, 1))
                                {
                                    randomProduceIndex = Game1.random.Next(0, monsterType.Value.DeluxeProduce.Count);
                                    randomProduceId = monsterType.Value.DeluxeProduce[randomProduceIndex].ItemId;
                                    dropDeluxe = true;
                                }
                                var produce = ItemRegistry.Create<StardewValley.Object>(randomProduceId);
                                produce.CanBeSetDown = false;
                                if (produce.Type == "Crafting")
                                    produce.Type = "Basic";
                                foreach (StardewValley.Object location_object in __instance.objects.Values)
                                {
                                    if (location_object.QualifiedItemId == "(BC)165" && location_object.heldObject.Value is Chest chest && chest.addItem(produce) == null)
                                    {
                                        location_object.showNextIndex.Value = true;
                                        spawn_object = false;
                                        break;
                                    }
                                }
                                if (spawn_object && dropDeluxe)
                                {
                                    for (int i = 0; i < monsterType.Value.DeluxeProduce[randomProduceIndex].Count; i++)
                                    {
                                        if (monsterType.Value.DeluxeProduce[randomProduceIndex].IsDropped)
                                        {
                                            var drop = produce.getOne();
                                            __instance.debris.Add(new Debris(drop, new Vector2(tile.X * 64, tile.Y * 64), new Vector2(tile.X * 64, tile.Y * 64)));
                                        }
                                        else
                                        {
                                            var item = (StardewValley.Object)produce.getOne();
                                            Utility.spawnObjectAround(tile, item, __instance);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            int roundUsedWater = Convert.ToInt32(Math.Floor(usedWater));
            for (int i = 0; i < __instance.waterSpots.Length; i++)
            {
                if (
                    (roundUsedWater > 4) ||
                    (roundUsedWater == 4 && Game1.random.NextDouble() < 0.8) ||
                    (roundUsedWater == 3 && Game1.random.NextDouble() < 0.6) ||
                    (roundUsedWater == 2 && Game1.random.NextDouble() < 0.4) ||
                    (roundUsedWater == 1 && Game1.random.NextDouble() < 0.2)
                    )
                {
                    __instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length] = false;
                }
            }
        }
        public static void SlimeHutchDayUpdate_Post(SlimeHutch __instance, ref List<Monster> __state)
        {
            // there is no AddRange for NetCollections
            foreach (var item in __state)
            {
                Vector2 v = Utility.recursiveFindOpenTileForCharacter(item, __instance, item.Tile, 50, allowOffMap: false);
                if (v != Vector2.Zero)
                {
                    item.setTileLocation(v);
                    __instance.addCharacter(item);
                }
            }
        }
    }
}
