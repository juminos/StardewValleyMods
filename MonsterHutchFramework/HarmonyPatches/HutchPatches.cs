using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;

namespace MonsterHutchFramework.HarmonyPatches
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

                foreach (var monsterData in AssetHandler.monsterHutchData)
                {
                    if (__instance.heldObject.Value.QualifiedItemId == monsterData.Value.InputItemId)
                    {
                        monster = MonsterBuilder.CreateMonster(v, monsterData.Value);
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
                if (__instance.characters[i] is Monster monster && monster is not GreenSlime)
                {
                    __state.Add(monster);
                    __instance.characters.RemoveAt(i);
                }
            }
            int startIndex = Game1.random.Next(__instance.waterSpots.Length);
            int usedWater = 0;
            foreach (var monsterType in AssetHandler.monsterHutchData)
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

                    usedWater += (int)((float)monstersWatered / monsterType.Value.NumberWatered);

                    ModEntry.SMonitor.Log($"{monsterCount} {monsterType.Value.Name} found, {monstersWatered} watered, {usedWater} water used.", StardewModdingAPI.LogLevel.Trace);

                    if (monsterType.Value.ProduceData.Count > 0)
                    {
                        for (int j = 0; j < (int)((float)monstersWatered / monsterType.Value.NumberToProduce); j++)
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
                                //var produceChance = Math.Clamp(monsterType.Value.ProduceChance + Game1.player.DailyLuck, 0, 1);
                                if (Game1.random.NextDouble() < monsterType.Value.ProduceChance)
                                {
                                    bool spawn_object = true;
                                    bool dropDeluxe = false;
                                    var produceIndex = 0;
                                    var produceId = monsterType.Value.ProduceData[produceIndex].ItemId;
                                    if (monsterType.Value.ProduceData.Count > 1)
                                    {
                                        var weightedList = new List<int>();
                                        for (int i = 0; i < monsterType.Value.ProduceData.Count; i++)
                                        {
                                            if (monsterType.Value.ProduceData[i].ItemId != null)
                                            {
                                                for (int k = 0; k < monsterType.Value.ProduceData[i].Weight; k++)
                                                {
                                                    weightedList.Add(i);
                                                }
                                            }
                                        }
                                        var random = new Random();
                                        int index = random.Next(weightedList.Count);
                                        produceIndex = weightedList[index];
                                        produceId = monsterType.Value.ProduceData[produceIndex].ItemId;
                                    }
                                    var deluxeChance = Math.Clamp(monsterType.Value.DeluxeChance + Game1.player.DailyLuck, 0, 1);
                                    if (Game1.random.NextDouble() < deluxeChance && monsterType.Value.DeluxeProduceData.Count > 0)
                                    {
                                        ModEntry.SMonitor.Log($"Deluxe chance {deluxeChance} check passed", LogLevel.Trace);

                                        var weightedList = new List<int>();
                                        for (int i = 0; i < monsterType.Value.DeluxeProduceData.Count; i++)
                                        {
                                            if (monsterType.Value.DeluxeProduceData[i].ItemId != null)
                                            {
                                                for (int k = 0; k < monsterType.Value.DeluxeProduceData[i].Weight; k++)
                                                {
                                                    weightedList.Add(i);
                                                }
                                            }
                                        }
                                        var random = new Random();
                                        int index = random.Next(weightedList.Count);
                                        produceIndex = weightedList[index];
                                        produceId = monsterType.Value.DeluxeProduceData[produceIndex].ItemId;
                                        dropDeluxe = true;
                                    }
                                    var produce = ItemRegistry.Create<StardewValley.Object>(produceId);
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
                                    if (spawn_object)
                                    {
                                        if (dropDeluxe)
                                        {
                                            for (int i = 0; i < monsterType.Value.DeluxeProduceData[produceIndex].Count; i++)
                                            {
                                                if (monsterType.Value.DeluxeProduceData[produceIndex].IsDropped)
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
                                        else
                                        {
                                            for (int i = 0; i < monsterType.Value.ProduceData[produceIndex].Count; i++)
                                            {
                                                if (monsterType.Value.ProduceData[produceIndex].IsDropped)
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
                }
            }
            for (int i = 0; i < __instance.waterSpots.Length; i++)
            {
                if (
                    usedWater > 4 ||
                    usedWater == 4 && Game1.random.NextDouble() < 0.8 ||
                    usedWater == 3 && Game1.random.NextDouble() < 0.6 ||
                    usedWater == 2 && Game1.random.NextDouble() < 0.4 ||
                    usedWater == 1 && Game1.random.NextDouble() < 0.2
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
