using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;
using static StardewValley.Monsters.Ghost;

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

            harmony.Patch(
                original: AccessTools.Method(typeof(BlueSquid), nameof(BlueSquid.behaviorAtGameTick), new[] { typeof(GameTime) }),
                prefix: new HarmonyMethod(typeof(RingPatches), nameof(BlueSquidPatch_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(Ghost), nameof(Ghost.behaviorAtGameTick), new[] { typeof(GameTime) }),
                prefix: new HarmonyMethod(typeof(RingPatches), nameof(GhostPatch_Prefix)));
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
            if (!__instance.GetBoundingBox().Intersects(who.GetBoundingBox()))
                return;
            if (MonsterIsCharmed(__instance, who, out string? matchedRingId, out int matchIndex))
            {
                __result = false;
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
        internal static void BlueSquidPatch_Prefix(BlueSquid __instance, GameTime time)
        {
            var who = Game1.player;
            if (MonsterIsCharmed(__instance, who, out string? matchedRingId, out int matchIndex))
            {
                __instance.nextFire = 3500.0f;
                return;
            }
        }
        internal static void GhostPatch_Prefix(Ghost __instance, GameTime time)
        {
            var who = Game1.player;
            if (MonsterIsCharmed(__instance, who, out string? matchedRingId, out int matchIndex) && __instance.variant.Value == GhostVariant.Putrid && __instance.currentState.Value == 3)
            {
                __instance.stateTimer = 1f;
                return;
            }
        }
        public static bool MonsterIsCharmed(Monster monster, Farmer who, out string? matchRingKey, out int matchMonsterIndex)
        {
            if (who.leftRing.Value == null && who.rightRing.Value == null)
            {
                matchRingKey = null;
                matchMonsterIndex = -1;
                return false;
            }
            bool isModded = monster.modData.ContainsKey($"{ModEntry.Mod.ModManifest.UniqueID}_Name");
            var ringData = AssetHandler.charmerRingData;
            var ringIds = new List<string>();

            if (who.leftRing.Value != null)
            {
                if (who.leftRing.Value is not CombinedRing)
                    ringIds.Add(who.leftRing.Value.ItemId);
                else
                    GetCombinedRingIds(who.leftRing.Value, ringIds);
            }
            if (who.rightRing.Value != null)
            {
                if (who.rightRing.Value is not CombinedRing)
                    ringIds.Add(who.rightRing.Value.ItemId);
                else
                    GetCombinedRingIds(who.rightRing.Value, ringIds);
            }
            foreach (var item in ringData)
            {
                var charmedData = ringData[item.Key].CharmedMonsters;
                foreach (var ringId in ringIds)
                {
                    if (item.Value.RingId == ringId)
                    {
                        for (int i = 0; i < ringData[item.Key].CharmedMonsters.Count; i++)
                        {
                            if (isModded && charmedData[i].MonsterName == monster.modData[$"{ModEntry.Mod.ModManifest.UniqueID}_Name"] ||
                                (!isModded && charmedData[i].MonsterName == monster.Name))
                            {
                                matchRingKey = item.Key;
                                matchMonsterIndex = i;
                                return true;
                            }
                        }
                    }
                }
            }
            matchRingKey = null;
            matchMonsterIndex = -1;
            return false;
        }
        public static void GetCombinedRingIds(Ring ring, List<string> combinedRings)
        {
            foreach (Ring r in (ring as CombinedRing).combinedRings)
            {
                if (r is not CombinedRing && !combinedRings.Contains(r.ItemId))
                    combinedRings.Add(r.ItemId);
                if (r is CombinedRing)
                    GetCombinedRingIds(r, combinedRings);
            }
        }
    }
}
