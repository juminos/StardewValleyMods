using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Threading;
using System.Reflection;

namespace CritterFixes
{ 
    public class ModEntry : Mod
    {
        public static Mod Instance;
        public static IMonitor SMonitor;

        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);
            CritterFixesPatch.ApplyPatch(harmony);

            ModEntry.Instance = this;
            SMonitor = this.Monitor;
        }
    }

    public static class CritterFixesPatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            harmony.PatchAll();
        }

        // Fix rabbit critter running animation

        [HarmonyPatch(typeof(Rabbit), nameof(Rabbit.update))]
        public static class Rabbit_Update_Patch
        {
            public static void Postfix(Rabbit __instance, GameTime time, GameLocation environment)
            {
                // Ensure the sprite is not null
                if (__instance.sprite != null && __instance.sprite.CurrentAnimation != null)
                {
                    // Find and replace the desired line
                    List<FarmerSprite.AnimationFrame> animationFrames = __instance.sprite.CurrentAnimation;
                    for (int i = 0; i < animationFrames.Count; i++)
                    {
                        if (animationFrames[i].frame == __instance.baseFrame + 5 && animationFrames[i].milliseconds == 70)
                        {
                            animationFrames[i] = new FarmerSprite.AnimationFrame(__instance.baseFrame + 4, 70);
                            break;
                        }
                    }
                }
            }
        }

        // Fix butterflies

        [HarmonyPatch(typeof(Butterfly))]
        [HarmonyPatch(MethodType.Constructor, typeof(GameLocation), typeof(Vector2), typeof(bool), typeof(bool), typeof(int), typeof(bool))]
        public static class Butterfly_Constructor_Patch
        {
            public static void Postfix(Butterfly __instance, GameLocation location, Vector2 position, bool islandButterfly, bool forceSummerButterfly, int baseFrameOverride, bool prismatic)
            {

                // Fix butterfly sprite reference

                if (__instance.baseFrame == 169)
                {
                    __instance.baseFrame = 428;
                    __instance.sprite = new AnimatedSprite(Critter.critterTexture, 428, 16, 16);
                    __instance.sprite.loop = false;
                }
                if (__instance.baseFrame == 173)
                {
                    __instance.baseFrame = 432;
                    __instance.sprite = new AnimatedSprite(Critter.critterTexture, 432, 16, 16);
                    __instance.sprite.loop = false;
                }

                // Fix butterfly animation **officially fixed in update 1.6.15

                //if (baseFrameOverride == 160 || baseFrameOverride == 180 || baseFrameOverride == 163 || baseFrameOverride == 183 || baseFrameOverride == 166 || baseFrameOverride == 186 || baseFrameOverride == 397)
                //{
                //    FieldInfo summerButterflyField = typeof(Butterfly).GetField("summerButterfly", BindingFlags.NonPublic | BindingFlags.Instance);
                //    summerButterflyField.SetValue(__instance, false);
                //}
            }
        }
    }
}