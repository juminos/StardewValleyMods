using System;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace BugNet.Framework
{
    /// <summary>Metadata for a critter supported by BugNet.</summary>
    internal class CritterData
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The texture to show in the critter cage.</summary>
        public Texture2D Texture { get; }

        /// <summary>The sprite index within the texture to show in the critter cage.</summary>
        public int SpriteIndex { get; }

        /// <summary>The default English critter name.</summary>
        public string DefaultName { get; }

        /// <summary>The critter name translated into the current locale.</summary>
        public Func<string> TranslatedName { get; }

        /// <summary>Get whether a given critter instance matches this critter.</summary>
        public Func<Critter, bool> IsThisCritter { get; }

        /// <summary>Create a critter instance at the given X and Y tile position.</summary>
        public Func<int, int, Critter> MakeCritter { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="defaultName">The critter name translated into the current locale.</param>
        /// <param name="translatedName">The default English critter name.</param>
        /// <param name="texture">The texture to show in the critter cage.</param>
        /// <param name="spriteIndex">The sprite index within the texture to show in the critter cage.</param>
        /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        public CritterData(string defaultName, Func<string> translatedName, Texture2D texture, int spriteIndex, Func<Critter, bool> isThisCritter, Func<int, int, Critter> makeCritter)
        {
            this.Texture = texture;
            this.SpriteIndex = spriteIndex;
            this.DefaultName = defaultName;
            this.TranslatedName = translatedName;
            this.IsThisCritter = isThisCritter;
            this.MakeCritter = makeCritter;
        }
    }
}
