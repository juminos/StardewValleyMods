using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            if (location is SlimeHutch hutch && location.canSlimeHatchHere() && location.characters.Count < ModEntry.Config.HutchMonsterCapacity)
            {
                ModEntry.SMonitor.Log($"map id: {hutch.map.Id}, path: {hutch.map.assetPath}, hutch name: {hutch.Name}", LogLevel.Trace);
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
            // Collect and remove non slimes to avoid interference with slime behavior
            if (__instance.map.Id.Contains("MonsterHutch"))
            {
                __state = new List<Monster>();
                for (int i = __instance.characters.Count - 1; i >= 0; i--)
                {
                    if (__instance.characters[i] is Monster monster &&
                        monster is not GreenSlime)
                    {
                        __state.Add(monster);
                        __instance.characters.RemoveAt(i);
                    }
                }
            }

            int startIndex = Game1.random.Next(__instance.waterSpots.Length);
            int waters = 0;
            for (int i = 0; i < __instance.waterSpots.Length; i++)
            {
                if (__instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length] && waters * 5 < __state.Count)
                {
                    waters++;
                    __instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length] = false;
                }
            }

            int usedWater = 0;
            foreach (var monsterType in AssetHandler.monsterHutchData)
            {
                if (waters < 1)
                    break;
                int monsterCount = 0;
                foreach (var monster in __state)
                {
                    ModEntry.SMonitor.Log($"tyring to water {monster.Name}", LogLevel.Trace);

                    if (monsterType.Value.Name == monster.Name && waters > 0)
                    {
                        monsterCount++;
                        usedWater++;
                        if (usedWater > 4)
                        {
                            waters--;
                            usedWater = 0;
                        }
                    }
                    else if (waters < 1)
                    {
                        ModEntry.SMonitor.Log($"no water left", LogLevel.Trace);
                        break;
                    }
                }

                if (monsterCount > 0 && monsterType.Value.ProduceData.Count > 0)
                {
                    for (int j = 0; j < (int)((float)monsterCount / monsterType.Value.NumberToProduce); j++)
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
                            if (Game1.random.NextDouble() < ((double)monsterType.Value.ProduceChance / 100.0))
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
                                    var deluxeChance = Math.Clamp(((double)monsterType.Value.DeluxeChance / 100.0) + Game1.player.DailyLuck, 0, 1);
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
                                ModEntry.SMonitor.Log($"{produce.Name} is type {produce.Type}, category {produce.Category}");
                                if (produce.ItemId == "395")
                                {
                                    produce.Type = "Cooking";
                                    produce.Category = -7;
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
        public static void SlimeHutchDayUpdate_Post(SlimeHutch __instance, ref List<Monster> __state)
        {
            if (__instance.map.Id.Contains("MonsterHutch"))
            {
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
        public static void MultipleObjectDebris_Pre(string id, int xTile, int yTile, ref int number, long who, GameLocation location)
        {

            if (location is SlimeHutch hutch && hutch.map.Id.Contains("MonsterHutch") &&
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
                var newNumber = number < 4 ? 3: number;
                Game1.createMultipleObjectDebris(id, xTile, yTile, newNumber, location);
            }
        }
    }
}
