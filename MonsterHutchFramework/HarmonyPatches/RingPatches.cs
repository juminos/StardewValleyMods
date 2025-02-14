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
            if (__instance is not GreenSlime && __instance is not BigSlime && MonsterIsCharmed(__instance, who, out string? matchedRingId, out int matchIndex) && !ModEntry.Config.LethalRings)
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
            if (MonsterIsCharmed(__instance, who, out string? matchedRingId, out int matchIndex))
            {
                __result = true;
                return;
            }
        }
        internal static void ShadowShamanCastPatch_Prefix(ShadowShaman __instance, GameTime time)
        {
            var who = Game1.player;
            if (MonsterIsCharmed(__instance, who, out string? matchedRingId, out int matchIndex))
            {
                __instance.coolDown = 1500;
                return;
            }
        }
        internal static void ShadowShooterPatch_Prefix(Shooter __instance, GameTime time)
        {
            var who = Game1.player;
            if (MonsterIsCharmed(__instance, who, out string? matchedRingId, out int matchIndex))
            {
                __instance.shooting.Value = false;
                __instance.nextShot = 2f;
                return;
            }
        }
        public static bool MonsterIsCharmed(Monster monster, Farmer who, out string? matchRingKey, out int matchMonsterIndex)
        {
            var ringData = AssetHandler.charmerRingData;
            if (who.rightRing.Value == null && who.leftRing.Value == null)
            {
                matchRingKey = null;
                matchMonsterIndex = -1;
                return false;
            }
            foreach(var item in ringData)
            {
                if (who.leftRing.Value != null && item.Value.RingId == who.leftRing.Value.ItemId)
                {
                    for (int i = 0; i < ringData[item.Key].CharmedMonsters.Count; i++)
                    {
                        if (ringData[item.Key].CharmedMonsters[i].MonsterName == monster.Name)
                        {
                            matchRingKey = item.Key;
                            matchMonsterIndex = i;
                            return true;
                        }
                    }
                }
                if (who.rightRing.Value != null && item.Value.RingId == who.rightRing.Value.ItemId)
                {
                    for (int i = 0; i < ringData[item.Key].CharmedMonsters.Count; i++)
                    {
                        if (ringData[item.Key].CharmedMonsters[i].MonsterName == monster.Name)
                        {
                            matchRingKey = item.Key;
                            matchMonsterIndex = i;
                            return true;
                        }
                    }
                }
            }
            matchRingKey = null;
            matchMonsterIndex = -1;
            return false;
        }
    }
}
