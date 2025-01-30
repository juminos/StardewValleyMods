using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.TerrainFeatures;
using HarmonyLib;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;

namespace Agrivoltaics
{
    internal class HarmonyPatches
    {
        private static ModEntry mod;

        public static void PatchAll(ModEntry monsterHutch)
        {
            mod = monsterHutch;

            var harmony = new Harmony(mod.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.dayUpdate)),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DayUpdate_Pre)));

            harmony.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.dayUpdate)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DayUpdate_Post)));
        }
        public static void DayUpdate_Pre(HoeDirt __instance, out int __state)
        {
            __state = __instance.state.Value;
        }
        public static void DayUpdate_Post(HoeDirt __instance, int __state)
        {
            bool isShaded = CheckShadedTiles(__instance);
            if (__state != 0 && isShaded && Game1.random.NextDouble() < ModEntry.Config.RetentionIncrease)
            {
                __instance.state.Value = __state;
                ModEntry.SMonitor.Log($"patching water retention for {__instance.Tile.X.ToString()}, {__instance.Tile.Y.ToString()} with state: {__state.ToString()}", StardewModdingAPI.LogLevel.Trace);
            }

            //ModEntry.SMonitor.Log($"patching water retention for {__instance.Tile.X.ToString()}, {__instance.Tile.Y.ToString()} with retention chance {__result.ToString()}", StardewModdingAPI.LogLevel.Trace);
            //bool isShaded = CheckShadedTiles(__instance);
            //if (isShaded)
            //{
            //    var increasedChance = Math.Clamp(__result + ModEntry.Config.RetentionIncrease, 0f, 1f);
            //    __result = increasedChance;
            //    ModEntry.SMonitor.Log($"dirt at {__instance.Tile.X.ToString()}, {__instance.Tile.Y.ToString()} is shaded. updateing to {increasedChance.ToString()}", StardewModdingAPI.LogLevel.Trace);
            //    return;
            //}
            //else
            //{
            //    ModEntry.SMonitor.Log($"dirt at {__instance.Tile.X.ToString()}, {__instance.Tile.Y.ToString()} is not shaded", StardewModdingAPI.LogLevel.Trace);
            //    return;
            //}
        }
        public static bool CheckShadedTiles(HoeDirt dirt)
        {
            var shadedTiles = new List<Vector2>();
            bool isShaded = false;
            foreach(Building building in dirt.Location.buildings)
            {
                if (building.buildingType.Value != "juminos.Agrivoltaics.CP_SolarPanel")
                    continue;

                var footprint = building.GetBoundingBox();
                var shadeRect = new Rectangle(footprint.X - (footprint.Width / 3), footprint.Y - (footprint.Height * 3), footprint.Width * 5 / 3, footprint.Height * 4);
                var tileRect = new Rectangle((int)dirt.Tile.X * 64, (int)dirt.Tile.Y * 64, 64, 64);

                if (tileRect.Intersects(shadeRect))
                {
                    isShaded = true;
                    break;
                }    
            }
            return isShaded;
        }
    }
}
