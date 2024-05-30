using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley;
using StardewModdingAPI;

namespace YourModNamespace
{
    public class YourModClass : Mod
    {
        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);
            YourPatchClass.ApplyPatch(harmony);
        }
    }
    public static class YourPatchClass
    {
        public static void ApplyPatch(Harmony harmony)
        {
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Rabbit), nameof(Rabbit.update))]
        public static class Rabbit_Update_Patch
        {
            public static bool Prefix(Rabbit __instance, GameTime time, GameLocation environment)
            {
                // Your logic here if needed before calling the original method
                // Return true to execute the original method, false to skip it
                return true;
            }

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
    }
}