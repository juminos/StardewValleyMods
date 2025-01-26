using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Linq;
using StardewObject = StardewValley.Object;

namespace MonsterHutch
{
    internal class HarmonyPatches
    {
        private static ModEntry mod;

        public static void PatchAll(ModEntry monsterHutch)
        {
            mod = monsterHutch;

            var harmony = new Harmony(mod.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.createObjectDebris), new Type[]
                { typeof(string), typeof(int), typeof(int), typeof(long), typeof(GameLocation) }),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CreateObjectDebris_Post)));

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewObject), nameof(StardewObject.DayUpdate)),
               postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(MonsterIncubatorUpdate_Post)));

            harmony.Patch(
               original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
               prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SlimeHutchDayUpdate_Pre)));

            //harmony.Patch(
            //   original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
            //   transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(SlimeHutchDayUpdate_Transpiler)));

            harmony.Patch(
               original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
               postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SlimeHutchDayUpdate_Post)));

            //harmony.Patch(
            //   original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.performToolAction)),
            //   prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SlimeHutchPerformToolAction_Pre)));

            //harmony.Patch(
            //   original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.UpdateWhenCurrentLocation)),
            //   prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SlimeHutchWhenCurrentLocation_Pre)));

            //harmony.Patch(
            //   original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.TransferDataFromSavedLocation)),
            //   prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SlimeHutchTransferDataFromSavedLocation_Pre)));

            //harmony.Patch(
            //   original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.UpdateWhenCurrentLocation)),
            //   postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SlimeHutchWhenCurrentLocation_Post)));

            //harmony.Patch(
            //   original: AccessTools.Method(typeof(DustSpirit), nameof(DustSpirit.behaviorAtGameTick)),
            //   prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(BehaviorAtGameTick_DustSprite)));
        }

        public static void CreateObjectDebris_Post(string id, int xTile, int yTile, long whichPlayer, GameLocation location)
        {
            ModEntry.SMonitor.Log("ore drop postfix triggered", StardewModdingAPI.LogLevel.Trace);
            Farmer forPlayer = Game1.GetPlayer(whichPlayer) ?? Game1.player;
            if (location is SlimeHutch &&
                (id == ModEntry.cinderShardId ||
                id == ModEntry.iridiumOreId ||
                id == ModEntry.goldOreId ||
                id == ModEntry.iridiumOreId ||
                id == ModEntry.copperOreId)
                )
            {
                for (int i = 0; i < 4; i++)
                {
                    location.debris.Add(new Debris(id, new Vector2(xTile * 64 + 32, yTile * 64 + 32), Game1.player.getStandingPosition()));
                }
            }
        }

        public static void MonsterIncubatorUpdate_Post(StardewObject __instance)
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

                switch (__instance.heldObject.Value.QualifiedItemId)
                {
                    case ModEntry.coalId:
                        monster = ModEntry.CreateDustSprite(v);
                        break;

                    case ModEntry.bugMeatId:
                        monster = ModEntry.CreateSpider(v);
                        break;

                    case ModEntry.mummifiedBatId:
                        monster = ModEntry.CreateRandomBat(v);
                        break;

                    case ModEntry.cinderShardId:
                        monster = ModEntry.CreateMagmaSprite(v);
                        break;

                    case ModEntry.iridiumOreId:
                        monster = ModEntry.CreateIridiumCrab(v);
                        break;

                    case ModEntry.goldOreId:
                        monster = ModEntry.CreateGoldCrab(v);
                        break;

                    case ModEntry.ironOreId:
                        monster = ModEntry.CreateIronCrab(v);
                        break;

                    case ModEntry.copperOreId:
                        monster = ModEntry.CreateCopperCrab(v);
                        break;

                    case ModEntry.truffleId:
                        monster = ModEntry.CreateTruffleCrab(v);
                        break;

                    case ModEntry.gingerId:
                        monster = ModEntry.CreateStickBug(v);
                        break;
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
            var dustSpriteCount = 0;
            var spiderCount = 0;
            var batCount = 0;
            var magmaSpriteCount = 0;
            var iridiumCrabCount = 0;
            var goldCrabCount = 0;
            var ironCrabCount = 0;
            var copperCrabCount = 0;
            var truffleCrabCount = 0;
            var stickBugCount = 0;

            // we collect and remove the non slimes, so we can run the base implementation for the slimes
            __state = new List<Monster>();

            for (int i = __instance.characters.Count - 1; i >= 0; i--)
            {
                if (__instance.characters[i] is DustSpirit dust && !dust.isHardModeMonster.Value)
                {
                    dustSpriteCount++;
                    __state.Add(dust);
                    __instance.characters.RemoveAt(i);
                }
                else if (__instance.characters[i] is Leaper spider)
                {
                    spiderCount++;
                    __state.Add(spider);
                    __instance.characters.RemoveAt(i);
                }
                else if (__instance.characters[i] is Bat bat && !bat.magmaSprite.Value && !bat.hauntedSkull.Value)
                {
                    batCount++;
                    __state.Add(bat);
                    __instance.characters.RemoveAt(i);
                }
                else if (__instance.characters[i] is Bat magmaSprite && magmaSprite.magmaSprite.Value)
                {
                    magmaSpriteCount++;
                    __state.Add(magmaSprite);
                    __instance.characters.RemoveAt(i);
                }
                else if (__instance.characters[i] is RockCrab iridiumCrab && iridiumCrab.Name == "Iridium Crab")
                {
                    iridiumCrabCount++;
                    __state.Add(iridiumCrab);
                    __instance.characters.RemoveAt(i);
                }
                else if (__instance.characters[i] is RockCrab goldCrab && goldCrab.Name == "Gold Crab")
                {
                    goldCrabCount++;
                    __state.Add(goldCrab);
                    __instance.characters.RemoveAt(i);
                }
                else if (__instance.characters[i] is RockCrab ironCrab && ironCrab.Name == "Iron Crab")
                {
                    ironCrabCount++;
                    __state.Add(ironCrab);
                    __instance.characters.RemoveAt(i);
                }
                else if (__instance.characters[i] is RockCrab copperCrab && copperCrab.Name == "Copper Crab")
                {
                    copperCrabCount++;
                    __state.Add(copperCrab);
                    __instance.characters.RemoveAt(i);
                }
                else if (__instance.characters[i] is RockCrab truffleCrab && truffleCrab.Name == "Truffle Crab")
                {
                    truffleCrabCount++;
                    __state.Add(truffleCrab);
                    __instance.characters.RemoveAt(i);
                }
                else if (__instance.characters[i] is RockCrab stickBug && stickBug.isStickBug.Value)
                {
                    stickBugCount++;
                    __state.Add(stickBug);
                    __instance.characters.RemoveAt(i);
                }
            }
            ModEntry.Mod.Monitor.Log($"collect counts are dust: {dustSpriteCount}, spider: {spiderCount}, bat: {batCount}, magma: {magmaSpriteCount}, iridium: {iridiumCrabCount}, gold: {goldCrabCount}, iron: {ironCrabCount}, copper: {copperCrabCount}, truffle: {truffleCrabCount}, stick: {stickBugCount}");

            // water troughs
            //if (__instance.waterSpots.Count !> 4)
            //{
            //    ModEntry.Mod.Monitor.Log("building waterspots array");
            //    __instance.waterSpots.Clear();  // second player accessing NetArray (waterSpots) seems to cause it to become fixed length which throws error when attempting to Clear
            //    var length = __instance.Map.GetLayer("Buildings").LayerSize.Area;
            //    for (int i = 0; i < length; i++)
            //        __instance.waterSpots.Add(false);
            //}            

            int dustWaters = 0;
            int spiderWaters = 0;
            int batWaters = 0;
            int magmaWaters = 0;
            int iridiumWaters = 0;
            int goldWaters = 0;
            int ironWaters = 0;
            int copperWaters = 0;
            int truffleWaters = 0;
            int stickWaters = 0;
            int startIndex = Game1.random.Next(__instance.waterSpots.Length);

            for (int i = 0; i < __instance.waterSpots.Length; i++)
            {
                //bool usedWater = false;
                int usedWater = 0;

                if (__instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length])
                {
                    if (dustWaters * 5 < dustSpriteCount)
                    {
                        dustWaters++;
                        usedWater++;
                    }
                    if (spiderWaters < spiderCount)
                    {
                        spiderWaters+=5;
                        usedWater++;
                    }
                    if (batWaters < batCount)
                    {
                        batWaters += 5;
                        usedWater++;
                    }
                    if (magmaWaters < magmaSpriteCount)
                    {
                        magmaWaters += 5;
                        usedWater++;
                    }
                    if (iridiumWaters < iridiumCrabCount)
                    {
                        iridiumWaters += 5;
                        usedWater++;
                    }
                    if (goldWaters < goldCrabCount)
                    {
                        goldWaters += 5;
                        usedWater++;
                    }
                    if (ironWaters < ironCrabCount)
                    {
                        ironWaters += 5;
                        usedWater++;
                    }
                    if (copperWaters < copperCrabCount)
                    {
                        copperWaters += 5;
                        usedWater++;
                    }
                    if (truffleWaters < truffleCrabCount)
                    {
                        truffleWaters += 5;
                        usedWater++;
                    }
                    if (stickWaters < stickBugCount)
                    {
                        stickWaters += 5;
                        usedWater++;
                    }
                    if (
                        (usedWater > 4) ||
                        (usedWater == 4 && Game1.random.NextDouble() < 0.8) ||
                        (usedWater == 3 && Game1.random.NextDouble() < 0.6) ||
                        (usedWater == 2 && Game1.random.NextDouble() < 0.4) ||
                        (usedWater == 1 && Game1.random.NextDouble() < 0.2)
                        )
                    {
                        __instance.waterSpots[(i + startIndex) % __instance.waterSpots.Length] = false;
                    }
                }
            }

            for (int numTripleShot = Math.Min((int)Math.Round(dustSpriteCount / 5f, MidpointRounding.AwayFromZero), dustWaters); numTripleShot > 0; numTripleShot--)
            {
                dustWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0)
                {
                    if (Game1.random.NextDouble() < (0.32 + Game1.player.DailyLuck))
                    {
                        var tripleShot = ItemRegistry.Create<StardewObject>(ModEntry.tripleShotId);
                        tripleShot.IsSpawnedObject = true;

                        __instance.Objects.Add(tile, tripleShot);
                    }
                    else
                    {
                        var coffee = ItemRegistry.Create<StardewObject>(ModEntry.coffeeId);
                        coffee.IsSpawnedObject = true;

                        __instance.Objects.Add(tile, coffee);
                    }
                }
            }

            for (int numBugMeat = Math.Min(spiderCount, spiderWaters); numBugMeat > 0; numBugMeat--)
            {
                spiderWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0)
                {
                    var meat = ItemRegistry.Create<StardewObject>(ModEntry.bugMeatId);
                    meat.IsSpawnedObject = true;

                    __instance.Objects.Add(tile, meat);
                }
            }

            for (int numBatWing = Math.Min(batCount, batWaters); numBatWing > 0; numBatWing--)
            {
                batWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0)
                {
                    var wing = ItemRegistry.Create<StardewObject>(ModEntry.batWingId);
                    //wing.IsSpawnedObject = true;

                    //__instance.Objects.Add(tile, wing);
                    __instance.debris.Add(new Debris(ModEntry.batWingId, new Vector2(tile.X * 64, tile.Y * 64), new Vector2(tile.X * 64, tile.Y * 64)));
                }
            }

            for (int numCinderShard = Math.Min(magmaSpriteCount, magmaWaters); numCinderShard > 0; numCinderShard--)
            {
                magmaWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0)
                {
                    var cinderNode = ItemRegistry.Create<StardewObject>(ModEntry.cinderStone1Id);
                    //shard.Fragility = 0;
                    cinderNode.MinutesUntilReady = 0;
                    __instance.objects.TryAdd(tile, cinderNode);
                }
            }
            for (int numIridiumNode = Math.Min(iridiumCrabCount, iridiumWaters); numIridiumNode > 0; numIridiumNode--)
            {
                iridiumWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0 && Game1.random.NextDouble() < 0.5)
                {
                    var iridiumNode = ItemRegistry.Create<StardewObject>(ModEntry.iridiumNodeId);
                    iridiumNode.MinutesUntilReady = 0;
                    __instance.Objects.Add(tile, iridiumNode);
                }
            }
            for (int numGoldNode = Math.Min(goldCrabCount, goldWaters); numGoldNode > 0; numGoldNode--)
            {
                goldWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0 && Game1.random.NextDouble() < 0.5)
                {
                    var goldNode = ItemRegistry.Create<StardewObject>(ModEntry.goldNodeId);
                    goldNode.MinutesUntilReady = 0;
                    __instance.Objects.Add(tile, goldNode);
                }
            }
            for (int numIronNode = Math.Min(ironCrabCount, ironWaters); numIronNode > 0; numIronNode--)
            {
                ironWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0 && Game1.random.NextDouble() < 0.5)
                {
                    var ironNode = ItemRegistry.Create<StardewObject>(ModEntry.ironNodeId);
                    ironNode.MinutesUntilReady = 0;
                    __instance.Objects.Add(tile, ironNode);
                }
            }
            for (int numCopperNode = Math.Min(copperCrabCount, copperWaters); numCopperNode > 0; numCopperNode--)
            {
                copperWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0 && Game1.random.NextDouble() < 0.5)
                {
                    var copperNode = ItemRegistry.Create<StardewObject>(ModEntry.copperNodeId);
                    copperNode.MinutesUntilReady = 0;
                    __instance.Objects.Add(tile, copperNode);
                }
            }
            for (int numTruffle = Math.Min(truffleCrabCount, truffleWaters); numTruffle > 0; numTruffle--)
            {
                truffleWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0 && Game1.random.NextDouble() < 0.5)
                {
                    var truffle = ItemRegistry.Create<StardewObject>(ModEntry.truffleId);
                    truffle.IsSpawnedObject = true;
                    __instance.Objects.Add(tile, truffle);
                }
            }
            for (int numGinger = Math.Min(stickBugCount, stickWaters); numGinger > 0; numGinger--)
            {
                stickWaters--;

                int tries = 50;
                Vector2 tile = __instance.getRandomTile();
                while ((!__instance.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || __instance.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NPCBarrier", "Back") != null || tile.Y >= 16f) && tries > 0)
                {
                    tile = __instance.getRandomTile();
                    tries--;
                }

                if (tries > 0)
                {
                    var ginger = ItemRegistry.Create<StardewObject>(ModEntry.gingerId);
                    ginger.IsSpawnedObject = true;
                    __instance.Objects.Add(tile, ginger);
                }
            }
        }

        //public static IEnumerable<CodeInstruction> SlimeHutchDayUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    ModEntry.Mod.Monitor.Log($"Transpiling SlimeHutch.DayUpdate");

        //    var codes = new List<CodeInstruction>(instructions);
        //    for (int i = 0; i < codes.Count; i++)
        //    {
        //        if (codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 1].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.X)) && codes[i + 2].opcode == OpCodes.Ldc_R4 && (float)codes[i + 2].operand == 16)
        //        {
        //            ModEntry.Mod.Monitor.Log("Replacing watering logic");
        //            codes.RemoveRange(i + 1, 20);
        //            codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatches), nameof(WaterSpot))));
        //            codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
        //            break;
        //        }
        //    }

        //    return codes.AsEnumerable();
        //}

        public static void SlimeHutchDayUpdate_Post(SlimeHutch __instance, ref List<Monster> __state)
        {
            // there is no AddRange for NetCollections
            foreach (var item in __state)
            {
                __instance.characters.Add(item);
            }
        }

        //public static bool SlimeHutchPerformToolAction_Pre(SlimeHutch __instance, Tool t, int tileX, int tileY)
        //{
        //    if (t is WateringCan)
        //    {
        //        __instance.waterSpots[tileY * __instance.Map.GetLayer("Buildings").LayerWidth + tileX] = true;
        //    }
        //    return false;
        //}

        //public static bool BehaviorAtGameTick_DustSprite(DustSpirit __instance, ref bool ___chargingFarmer, ref bool ___seenFarmer)
        //{
        //    if (___seenFarmer && __instance.currentLocation is SlimeHutch)
        //    {
        //        var farmer = Utility.isThereAFarmerWithinDistance(__instance.getStandingPosition() / 64f, 3, __instance.currentLocation);

        //        if (farmer == null)
        //        {
        //            ___chargingFarmer = false;
        //            __instance.controller = null;
        //        }
        //    }
        //    return true;
        //}

        //public static bool SlimeHutchWhenCurrentLocation_Pre(SlimeHutch __instance, GameTime time)
        //{
        //    var ptr = typeof(GameLocation).GetMethod("UpdateWhenCurrentLocation", BindingFlags.Public | BindingFlags.Instance).MethodHandle.GetFunctionPointer();
        //    var baseMethod = (Func<GameTime, GameLocation>)Activator.CreateInstance(typeof(Func<GameTime, GameLocation>), __instance, ptr);
        //    baseMethod(time);
        //    var size = __instance.Map.GetLayer("Buildings").LayerSize;
        //    for (int y = 0; y < size.Height; y++)
        //    {
        //        for (int x = 0; x < size.Width; x++)
        //        {
        //            Point p = new Point(x, y);
        //            int idx = y * size.Width + x;
        //            int tile = __instance.getTileIndexAt(p, "Buildings");
        //            if (tile == 2134 && __instance.waterSpots[idx])
        //            {
        //                __instance.setMapTile(x, y, 2135, "Buildings", "untitled tile sheet");
        //            }
        //            else if (tile == 2135 && !__instance.waterSpots[idx])
        //            {
        //                __instance.setMapTile(x, y, 2134, "Buildings", "untitled tile sheet");
        //            }
        //        }
        //    }
        //    return false;
        //}

        //public static void SlimeHutchTransferDataFromSavedLocation_Pre(SlimeHutch __instance)
        //{
        //    ModEntry.Mod.Monitor.Log("building waterspots array");
        //    __instance.waterSpots.Clear();
        //    var length = __instance.Map.GetLayer("Buildings").LayerSize.Area;
        //    for (int i = 0; i < length; i++)
        //        __instance.waterSpots.Add(false);
        //}

        //private static void WaterSpot(SlimeHutch hutch, Vector2 tile)
        //{
        //    int idx = hutch.getTileIndexAt(Utility.Vector2ToPoint(tile), "Buildings");
        //    if (idx == 2135 || idx == 2134)
        //        hutch.waterSpots[hutch.Map.GetLayer("Buildings").LayerWidth * (int)tile.Y + (int)tile.X] = true;
        //}
    }
}
