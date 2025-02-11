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
            var who = Game1.player;
            if (__instance is not GreenSlime && __instance is not BigSlime && MonsterIsCharmed(__instance, who) && !ModEntry.Config.LethalRings)
            {
                __result = true;
                return;
            }
            if ((__instance is GreenSlime || __instance is BigSlime) && who.isWearingRing("520") && !ModEntry.Config.VanillaSlimeRing)
            {
                __result = true;
                return;
            }
        }
        internal static void OverlapFarmerDamage_Postfix(Monster __instance, Farmer who, ref bool __result)
        {
            if (MonsterIsCharmed(__instance, who))
            {
                __result = true;
                return;
            }
        }
        internal static void ShadowShamanCastPatch_Prefix(ShadowShaman __instance, GameTime time)
        {
            var who = Game1.player;
            if (MonsterIsCharmed(__instance, who))
            {
                __instance.coolDown = 1500;
                return;
            }
        }
        internal static void ShadowShooterPatch_Prefix(Shooter __instance, GameTime time)
        {
            var who = Game1.player;
            if (MonsterIsCharmed(__instance, who))
            {
                __instance.shooting.Value = false;
                __instance.nextShot = 2f;
                return;
            }
        }
        public static bool MonsterIsCharmed(Monster monster, Farmer who)
        {
            if (who.rightRing.Value == null && who.leftRing.Value == null)
            {
                //ModEntry.SMonitor.Log($"No rings found on {who.Name}", LogLevel.Trace);
                return false;
            }
            else if (who.leftRing.Value != null && AssetHandler.charmerRingData.ContainsKey(who.leftRing.Value.ItemId) && AssetHandler.charmerRingData[who.leftRing.Value.ItemId].CharmedMonsters.Contains(monster.Name))
            {
                //ModEntry.SMonitor.Log($"{who.Name} wearing {who.leftRing.Value.ItemId} ring for {monster.Name}", LogLevel.Trace);

                return true;
            }
            else if (who.rightRing.Value != null && AssetHandler.charmerRingData.ContainsKey(who.rightRing.Value.ItemId) && AssetHandler.charmerRingData[who.rightRing.Value.ItemId].CharmedMonsters.Contains(monster.Name))
            {
                //ModEntry.SMonitor.Log($"{who.Name} wearing {who.rightRing.Value.ItemId} ring for {monster.Name}", LogLevel.Trace);

                return true;
            }
            else
            {
                //ModEntry.SMonitor.Log($"{who.Name} not wearing matching ring for {monster.Name}", LogLevel.Trace);
                return false;
            }
        }
    }
}
