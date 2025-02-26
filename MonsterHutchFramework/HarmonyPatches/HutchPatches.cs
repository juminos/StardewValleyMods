using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Tools;

namespace MonsterHutchFramework.HarmonyPatches;

internal class HutchPatches
{
    private static ModEntry mod;

    public static void PatchAll(ModEntry monsterHutch)
    {
        mod = monsterHutch;

        var harmony = new Harmony(mod.ModManifest.UniqueID);

        harmony.Patch(
           original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
           prefix: new HarmonyMethod(typeof(HutchPatches), nameof(SlimeHutchDayUpdate_Pre)));

        harmony.Patch(
           original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
           postfix: new HarmonyMethod(typeof(HutchPatches), nameof(SlimeHutchDayUpdate_Post)));

        harmony.Patch(
           original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.UpdateWhenCurrentLocation)),
           prefix: new HarmonyMethod(typeof(HutchPatches), nameof(UpdateWhenCurrentLocation_Pre)));

        harmony.Patch(
           original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.TransferDataFromSavedLocation)),
           prefix: new HarmonyMethod(typeof(HutchPatches), nameof(TransferDataFromSavedLocation_Pre)));

        harmony.Patch(
           original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.performToolAction)),
           prefix: new HarmonyMethod(typeof(HutchPatches), nameof(PerformToolAction_Pre)));

        harmony.Patch(
           original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
           prefix: new HarmonyMethod(typeof(HutchPatches), nameof(DayUpdate_Pre)));

        harmony.Patch(
           original: AccessTools.Method(typeof(SlimeHutch), nameof(SlimeHutch.DayUpdate)),
           transpiler: new HarmonyMethod(typeof(HutchPatches), nameof(DayUpdate_Transpiler)));
    }
    public static void SlimeHutchDayUpdate_Pre(SlimeHutch __instance, ref List<Monster> __state)
    {
        // Collect and remove non slimes to avoid interference with slime behavior
        if (__instance.Name.Contains("MonsterHutchFramework") || __instance.Name.Contains("juminos.SeasonalBuildings_Winery"))
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
                    // ModEntry.SMonitor.Log($"trying to water {monster.Name}", LogLevel.Trace);

                    if (monster is not null && monster.modData.ContainsKey("{{ModId}}_Name") && monsterType.Value.Name == monster.modData["{{ModId}}_Name"] && waters > 0)
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

                if (monsterCount > 0 && monsterType.Value.ProduceData.Count > 0 && (string.IsNullOrEmpty(monsterType.Value.ProduceCondition) || GameStateQuery.CheckConditions(monsterType.Value.ProduceCondition)))
                {
                    for (int j = 0; j < (int)((float)monsterCount / monsterType.Value.NumberRequiredToProduce); j++)
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
                                        if (!string.IsNullOrEmpty(monsterType.Value.ProduceData[i].ItemId))
                                        {
                                            for (int k = 0; k < monsterType.Value.ProduceData[i].Weight; k++)
                                                weightedList.Add(i);
                                        }
                                    }
                                    var random = new Random();
                                    int index = random.Next(weightedList.Count);
                                    produceIndex = weightedList[index];
                                    produceId = monsterType.Value.ProduceData[produceIndex].ItemId;
                                }
                                if (monsterType.Value.DeluxeChance > 0 && monsterType.Value.DeluxeProduceData.Count > 0 && (string.IsNullOrEmpty(monsterType.Value.DeluxeCondition) || GameStateQuery.CheckConditions(monsterType.Value.DeluxeCondition)))
                                {
                                    var deluxeChance = Math.Clamp(((double)monsterType.Value.DeluxeChance / 100.0) + Game1.player.DailyLuck, 0, 1);
                                    if (Game1.random.NextDouble() < deluxeChance && monsterType.Value.DeluxeProduceData.Count > 0)
                                    {
                                        ModEntry.SMonitor.Log($"Deluxe chance {deluxeChance} check passed", LogLevel.Trace);

                                        var weightedList = new List<int>();
                                        for (int i = 0; i < monsterType.Value.DeluxeProduceData.Count; i++)
                                        {
                                            if (!string.IsNullOrEmpty(monsterType.Value.DeluxeProduceData[i].ItemId))
                                            {
                                                for (int k = 0; k < monsterType.Value.DeluxeProduceData[i].Weight; k++)
                                                    weightedList.Add(i);
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
                                ModEntry.SMonitor.Log($"{produce.Name} is type {produce.Type}, category {produce.Category}", LogLevel.Trace);
                                if (produce.Type != "Litter")
                                {
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
                                                    item.MinutesUntilReady = 1;
                                                }
                                                if (produce.ItemId == "395")
                                                    ModEntry.SMonitor.Log("Coffee cannot be picked up, change IsDropped to true", LogLevel.Info);
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
                                                    item.MinutesUntilReady = 1;
                                                }
                                                if (produce.ItemId == "395")
                                                    ModEntry.SMonitor.Log("Coffee cannot be picked up, change IsDropped to true", LogLevel.Info);
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
        if (__instance.Name.Contains("MonsterHutchFramework") || __instance.Name.Contains("juminos.SeasonalBuildings_Winery"))
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
    public static bool UpdateWhenCurrentLocation_Pre(SlimeHutch __instance, GameTime time)
    {
        var ptr = typeof(GameLocation).GetMethod("UpdateWhenCurrentLocation", BindingFlags.Public | BindingFlags.Instance).MethodHandle.GetFunctionPointer();
        var baseMethod = (Func<GameTime, GameLocation>)Activator.CreateInstance(typeof(Func<GameTime, GameLocation>), __instance, ptr);
        baseMethod(time);
        var size = __instance.Map.GetLayer("Buildings").LayerSize;
        for (int y = 0; y < size.Height; y++)
        {
            for (int x = 0; x < size.Width; x++)
            {
                Point p = new Point(x, y);
                int idx = y * size.Width + x;
                int tile = __instance.getTileIndexAt(p, "Buildings");
                if (tile == 2134 && __instance.waterSpots[idx])
                    __instance.setMapTile(x, y, 2135, "Buildings", "untitled tile sheet");
                else if (tile == 2135 && !__instance.waterSpots[idx])
                    __instance.setMapTile(x, y, 2134, "Buildings", "untitled tile sheet");
            }
        }
        return false;
    }
    public static void TransferDataFromSavedLocation_Pre(SlimeHutch __instance)
    {
        ModEntry.SMonitor.Log("building waterspots array");
        __instance.waterSpots.Clear();
        var length = __instance.Map.GetLayer("Buildings").LayerSize.Area;
        for (int i = 0; i < length; i++)
            __instance.waterSpots.Add(false);
    }
    public static bool PerformToolAction_Pre(SlimeHutch __instance, Tool t, int tileX, int tileY)
    {
        if (t is WateringCan)
            __instance.waterSpots[tileY * __instance.Map.GetLayer("Buildings").LayerWidth + tileX] = true;
        return false;
    }
    public static void DayUpdate_Pre(SlimeHutch __instance)
    {
        if (__instance.waterSpots.Count > 4)
            return;
        ModEntry.SMonitor.Log("building waterspots array");
        __instance.waterSpots.Clear();
        var length = __instance.Map.GetLayer("Buildings").LayerSize.Area;
        for (int i = 0; i < length; i++)
            __instance.waterSpots.Add(false);
    }
    public static IEnumerable<CodeInstruction> DayUpdate_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        ModEntry.SMonitor.Log($"Transpiling SlimeHutch.DayUpdate");

        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 1].operand == AccessTools.Field(typeof(Vector2), nameof(Vector2.X)) && codes[i + 2].opcode == OpCodes.Ldc_R4 && (float)codes[i + 2].operand == 16)
            {
                ModEntry.SMonitor.Log("Replacing watering logic");
                codes.RemoveRange(i + 1, 20);
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HutchPatches), nameof(HutchPatches.WaterSpot))));
                codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                break;
            }
        }

        return codes.AsEnumerable();
    }
    private static void WaterSpot(SlimeHutch hutch, Vector2 tile)
    {
        int idx = hutch.getTileIndexAt(Utility.Vector2ToPoint(tile), "Buildings");
        if (idx == 2135 || idx == 2134)
            hutch.waterSpots[hutch.Map.GetLayer("Buildings").LayerWidth * (int)tile.Y + (int)tile.X] = true;
    }
}
