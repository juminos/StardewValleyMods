using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Threading;

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

            helper.Events.Content.AssetRequested += this.OnAssetRequested;
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/critters"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsImage();
                    IRawTextureData sourceImage = this.Helper.ModContent.Load<IRawTextureData>("assets/butterflyfix.png");
                    editor.PatchImage(sourceImage, targetArea: new Rectangle(144, 128, sourceImage.Width, sourceImage.Height));
                });
            }
        }
    }

    public static class CritterFixesPatch
    {
        public static void ApplyPatch(Harmony harmony)
        {
            harmony.PatchAll();
        }

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
    }
}