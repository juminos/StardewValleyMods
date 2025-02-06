using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace MonsterHutchFramework.HarmonyPatches
{
    internal class RingPatches
    {
        private static ModEntry mod;

        public static void PatchAll(ModEntry monsterHutch)
        {
            mod = monsterHutch;

            var harmony = new Harmony(mod.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Monster), nameof(Monster.isInvincible), null),
               postfix: new HarmonyMethod(typeof(RingPatches), nameof(InvinciblePatch_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Monster), nameof(Monster.OverlapsFarmerForDamage), null),
                postfix: new HarmonyMethod(typeof(RingPatches), nameof(OverlapFarmerDamage_Postfix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(ShadowShaman), nameof(ShadowShaman.behaviorAtGameTick), new[] { typeof(GameTime) }),
                prefix: new HarmonyMethod(typeof(RingPatches), nameof(ShadowShamanCastPatch_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Shooter), nameof(Shooter.behaviorAtGameTick), new[] { typeof(GameTime) }),
                prefix: new HarmonyMethod(typeof(RingPatches), nameof(ShadowShooterPatch_Prefix)));
        }
        internal static void InvinciblePatch_Postfix(Monster __instance, ref bool __result)
        {
            if (Game1.player.leftRing != null && Game1.player.rightRing != null && !ModEntry.Config.LethalRings)
            {
                string leftRingId = Game1.player.leftRing.Value.ItemId;
                string rightRingId = Game1.player.rightRing.Value.ItemId;

                ModEntry.SMonitor.Log($"player wearing a ring", LogLevel.Trace);

                if (AssetHandler.charmerRingData.ContainsKey(leftRingId) || AssetHandler.charmerRingData.ContainsKey(rightRingId))
                {
                    ModEntry.SMonitor.Log($"found {leftRingId} or {rightRingId}", LogLevel.Trace);

                    if (AssetHandler.charmerRingData[leftRingId].CharmedMonsters.Contains(__instance.Name) || AssetHandler.charmerRingData[rightRingId].CharmedMonsters.Contains(__instance.Name))
                    {
                        ModEntry.SMonitor.Log($"found {__instance.Name} in ring monster list", LogLevel.Trace);

                        __result = true;
                        return;
                    }
                }
            }
        }
        internal static void OverlapFarmerDamage_Postfix(Monster __instance, Farmer who, ref bool __result)
        {
            if (Game1.player.leftRing != null && Game1.player.rightRing != null)
            {
                string leftRingId = Game1.player.leftRing.Value.ItemId;
                string rightRingId = Game1.player.rightRing.Value.ItemId;

                ModEntry.SMonitor.Log($"player wearing a ring", LogLevel.Trace);

                if (AssetHandler.charmerRingData.ContainsKey(leftRingId) || AssetHandler.charmerRingData.ContainsKey(rightRingId))
                {
                    ModEntry.SMonitor.Log($"found {leftRingId} or {rightRingId}", LogLevel.Trace);

                    if (AssetHandler.charmerRingData[leftRingId].CharmedMonsters.Contains(__instance.Name) || AssetHandler.charmerRingData[rightRingId].CharmedMonsters.Contains(__instance.Name))
                    {
                        ModEntry.SMonitor.Log($"found {__instance.Name} in ring monster list", LogLevel.Trace);

                        __result = true;
                        return;
                    }
                }
            }
        }
        internal static void ShadowShamanCastPatch_Prefix(ShadowShaman __instance, GameTime time)
        {
            if (Game1.player.leftRing != null && Game1.player.rightRing != null)
            {
                string leftRingId = Game1.player.leftRing.Value.ItemId;
                string rightRingId = Game1.player.rightRing.Value.ItemId;

                ModEntry.SMonitor.Log($"player wearing a ring", LogLevel.Trace);

                if (AssetHandler.charmerRingData.ContainsKey(leftRingId) || AssetHandler.charmerRingData.ContainsKey(rightRingId))
                {
                    ModEntry.SMonitor.Log($"found {leftRingId} or {rightRingId}", LogLevel.Trace);

                    if (AssetHandler.charmerRingData[leftRingId].CharmedMonsters.Contains(__instance.Name) || AssetHandler.charmerRingData[rightRingId].CharmedMonsters.Contains(__instance.Name))
                    {
                        ModEntry.SMonitor.Log($"found {__instance.Name} in ring monster list", LogLevel.Trace);

                        __instance.coolDown = 1500;
                        return;
                    }
                }
            }
        }
        internal static void ShadowShooterPatch_Prefix(Shooter __instance, GameTime time)
        {
            if (Game1.player.leftRing != null && Game1.player.rightRing != null)
            {
                string leftRingId = Game1.player.leftRing.Value.ItemId;
                string rightRingId = Game1.player.rightRing.Value.ItemId;

                ModEntry.SMonitor.Log($"player wearing a ring", LogLevel.Trace);

                if (AssetHandler.charmerRingData.ContainsKey(leftRingId) || AssetHandler.charmerRingData.ContainsKey(rightRingId))
                {
                    ModEntry.SMonitor.Log($"found {leftRingId} or {rightRingId}", LogLevel.Trace);

                    if (AssetHandler.charmerRingData[leftRingId].CharmedMonsters.Contains(__instance.Name) || AssetHandler.charmerRingData[rightRingId].CharmedMonsters.Contains(__instance.Name))
                    {
                        ModEntry.SMonitor.Log($"found {__instance.Name} in ring monster list", LogLevel.Trace);

                        __instance.shooting.Value = false;
                        __instance.nextShot = 2f;
                        return;
                    }
                }
            }
        }
    }
}
