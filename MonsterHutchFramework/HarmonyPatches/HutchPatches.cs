using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), [typeof(string), typeof(int), typeof(int), typeof(int), typeof(long), typeof(GameLocation)]),
               postfix: new HarmonyMethod(typeof(HutchPatches), nameof(MultipleObjectDebris_Pre)));
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
            float usedWater = 0;
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

                    usedWater += ((float)monstersWatered / monsterType.Value.NumberWatered);

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
                                    if (monsterType.Value.DeluxeChance > 0 && monsterType.Value.DeluxeProduceData.Count > 0)
                                    {
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
                                    }
                                    var produce = ItemRegistry.Create<StardewValley.Object>(produceId);
                                    produce.CanBeSetDown = false;
                                    if (produce.ItemId == "395")
                                    {
                                        produce.Type = "Basic";
                                        produce.IsSpawnedObject = true;
                                        produce.CanBeGrabbed = true;
                                        produce.Category = -28;
                                    }
                                    if (produce.Type != "Litter")
                                    {
                                        ModEntry.SMonitor.Log($"{produce.Name} is {produce.Type}, not litter");
                                        foreach (StardewValley.Object location_object in __instance.objects.Values)
                                        {
                                            if (location_object.QualifiedItemId == "(BC)165" && location_object.heldObject.Value is Chest chest && chest.addItem(produce) == null)
                                            {
                                                location_object.showNextIndex.Value = true;
                                                spawn_object = false;
                                                break;
                                            }
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
                                                    if (item.Type == "Litter")
                                                    {
                                                        item.IsSpawnedObject = false;
                                                        item.CanBeGrabbed = false;
                                                    }
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
                                                    if (item.Type == "Litter")
                                                    {
                                                        item.IsSpawnedObject = false;
                                                        item.CanBeGrabbed = false;
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
            }

            ModEntry.SMonitor.Log($"used water count {usedWater} cast to double {(double)usedWater}", LogLevel.Trace);

            for (int i = 0; i < __instance.waterSpots.Length; i++)
            {
                if (__instance.waterSpots[i] &&
                    Game1.random.NextDouble() < ((double)usedWater) / 2)
                {
                    __instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length] = false;
                    usedWater -= 2;
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
        public static void MultipleObjectDebris_Pre(string id, ref int number)
        {
            if (Game1.currentLocation is SlimeHutch && 
                (id == "(O)909" ||
                id == "(O)848" ||
                id == "(O)881" ||
                id == "(O)330" ||
                id == "(O)390" ||
                id == "(O)382" ||
                id == "(O)378" ||
                id == "(O)380" ||
                id == "(O)382" ||
                id == "(O)384" ||
                id == "(O)386"))
            {
                number *= 2;
                //Game1.random.Next(6, 10) + (Game1.random.NextDouble() < (double)((float)Game1.player.LuckLevel / 100f) ? 1 : 0) + (Game1.random.NextDouble() < (double)((float)Game1.player.DailyLuck / 100f) ? 1 : 0);
            }
            ModEntry.SMonitor.Log($"{id} multidrop triggered, modified count to {number}", LogLevel.Trace);
        }
    }
}
