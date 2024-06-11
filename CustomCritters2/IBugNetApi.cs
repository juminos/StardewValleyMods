using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.BellsAndWhistles;

namespace CustomCritters2
{
        /// <summary>The BugNet API which other mods can access.</summary>
        public interface IBugNetApi
        {
            /*********
            ** Methods
            *********/
            /// <summary>Add a new critter which can be caught.</summary>
            /// <param name="manifest">The manifest of the mod registering the critter.</param>
            /// <param name="critterId">The unique critter ID.</param>
            /// <param name="textureName">The texture to add to objects.</param>
            /// <param name="index">The index of the sprite on the spritesheet.</param>
            /// <param name="defaultCritterName">The default English critter name.</param>
            /// <param name="translatedCritterNames">The translated critter names in each available locale.</param>
            /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
            /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
            void RegisterCritter(IManifest manifest, string critterId, string textureName, int index, string defaultCritterName, Dictionary<string, string> translatedCritterNames, Func<int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter);
        }
}