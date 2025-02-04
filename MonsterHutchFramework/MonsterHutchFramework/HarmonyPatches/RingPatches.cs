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

namespace MonsterHutchFramework.MonsterHutchFramework.HarmonyPatches
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
            if (!ModEntry.Config.LethalRings)
            {
                if ((__instance is GreenSlime || __instance is BigSlime) && Game1.player.isWearingRing("520"))
                {
                    __result = true;
                    return;
                }
                foreach (var monsterData in AssetHandler.data)
                {
                    if (monsterData.Value.Name == __instance.Name && Game1.player.isWearingRing(monsterData.Value.CharmerRingId))
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
        internal static void OverlapFarmerDamage_Postfix(Monster __instance, Farmer who, ref bool __result)
        {
            foreach (var monsterData in AssetHandler.data)
            {
                if (monsterData.Value.Name == __instance.Name && Game1.player.isWearingRing(monsterData.Value.CharmerRingId))
                {
                    __result = true;
                    return;
                }
            }
        }
        internal static void ShadowShamanCastPatch_Prefix(ShadowShaman __instance, GameTime time)
        {
            foreach (var monsterData in AssetHandler.data)
            {
                if (monsterData.Value.Name == __instance.Name && 
                    __instance.coolDown <= 1500 && 
                    Game1.player.isWearingRing(monsterData.Value.CharmerRingId))
                {
                    __instance.coolDown = 1500;
                    return;
                }
            }
        }
        internal static void ShadowShooterPatch_Prefix(Shooter __instance, GameTime time)
        {
            foreach (var monsterData in AssetHandler.data)
            {
                if (monsterData.Value.Name == __instance.Name && 
                    (__instance.shooting.Value == true || (__instance.shooting.Value == false && __instance.nextShot <= 0f)) && 
                    Game1.player.isWearingRing(monsterData.Value.CharmerRingId))
                {
                    __instance.shooting.Value = false;
                    __instance.nextShot = 2f;
                    return;
                }
            }
        }
    }
}
