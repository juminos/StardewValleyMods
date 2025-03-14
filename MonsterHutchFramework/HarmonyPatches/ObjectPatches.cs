﻿using HarmonyLib;
using StardewValley;

namespace MonsterHutchFramework.HarmonyPatches;

internal class ObjectPatches
{
    private static ModEntry mod;

    public static void PatchAll(ModEntry monsterHutch)
    {
        mod = monsterHutch;

        var harmony = new Harmony(mod.ModManifest.UniqueID);

        harmony.Patch(
           original: AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), [typeof(string), typeof(int), typeof(int), typeof(int)]),
           prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(MultipleObjectDebris_Pre1)));

        harmony.Patch(
           original: AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), [typeof(string), typeof(int), typeof(int), typeof(int), typeof(GameLocation)]),
           prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(MultipleObjectDebris_Pre2)));

        // This overload only used by slime balls
        //harmony.Patch(
        //   original: AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), [typeof(string), typeof(int), typeof(int), typeof(int), typeof(float)]),
        //   prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(MultipleObjectDebris_Pre3)));

        harmony.Patch(
           original: AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), [typeof(string), typeof(int), typeof(int), typeof(int), typeof(long)]),
           prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(MultipleObjectDebris_Pre4)));

        harmony.Patch(
           original: AccessTools.Method(typeof(Game1), nameof(Game1.createMultipleObjectDebris), [typeof(string), typeof(int), typeof(int), typeof(int), typeof(long), typeof(GameLocation)]),
           prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(MultipleObjectDebris_Pre5)));
    }
    public static bool MultipleObjectDebris_Pre1(string id, int xTile, int yTile, ref int number)
    {

        if (Game1.player.currentLocation is SlimeHutch hutch && 
            (hutch.Name.Contains("MonsterHutchFramework") || hutch.Name.Contains("juminos.SeasonalBuildings_Winery")) && 
            ModEntry.Config.DoubleNodeDrops)
        {
            for (int i = 0; i < number; i++)
                Game1.createObjectDebris(id, xTile, yTile);
        }
        return true;
    }
    public static bool MultipleObjectDebris_Pre2(string id, int xTile, int yTile, ref int number, GameLocation location)
    {

        if (location is SlimeHutch hutch && 
            (hutch.Name.Contains("MonsterHutchFramework") || hutch.Name.Contains("juminos.SeasonalBuildings_Winery")) && 
            ModEntry.Config.DoubleNodeDrops)
        {
            for (int i = 0; i < number; i++)
                Game1.createObjectDebris(id, xTile, yTile);
        }
        return true;
    }
    public static bool MultipleObjectDebris_Pre3(string id, int xTile, int yTile, ref int number, float velocityMultiplier)
    {

        if (Game1.player.currentLocation is SlimeHutch hutch && 
            (hutch.Name.Contains("MonsterHutchFramework") || hutch.Name.Contains("juminos.SeasonalBuildings_Winery")) && 
            ModEntry.Config.DoubleNodeDrops)
        {
            for (int i = 0; i < number; i++)
                Game1.createObjectDebris(id, xTile, yTile);
        }
        return true;
    }
    public static bool MultipleObjectDebris_Pre4(string id, int xTile, int yTile, ref int number, long who)
    {

        if (Game1.player.currentLocation is SlimeHutch hutch && 
            (hutch.Name.Contains("MonsterHutchFramework") || hutch.Name.Contains("juminos.SeasonalBuildings_Winery")) && 
            ModEntry.Config.DoubleNodeDrops)
        {
            for (int i = 0; i < number; i++)
                Game1.createObjectDebris(id, xTile, yTile);
        }
        return true;
    }
    public static bool MultipleObjectDebris_Pre5(string id, int xTile, int yTile, ref int number, long who, GameLocation location)
    {

        if (location is SlimeHutch hutch && 
            (hutch.Name.Contains("MonsterHutchFramework") || 
            hutch.Name.Contains("juminos.SeasonalBuildings_Winery")) && ModEntry.Config.DoubleNodeDrops)
        {
            for (int i = 0; i < number; i++)
                Game1.createObjectDebris(id, xTile, yTile);
        }
        return true;
    }
}
