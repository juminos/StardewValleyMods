using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.TerrainFeatures;

namespace BugNet2.Framework
{
    /// <summary>Builds a vanilla critter instance.</summary>
    internal class CritterBuilder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>Create a critter instance at the given X and Y tile position.</summary>
        public Func<int, int, int, Critter> MakeCritter;

        /// <summary>Get whether a given critter instance matches this critter.</summary>
        public Func<Critter, bool> IsThisCritter;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
        public CritterBuilder(Func<int, int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter)
        {
            this.MakeCritter = makeCritter;
            this.IsThisCritter = isThisCritter;
        }

        /// <summary>Create a butterfly.</summary>
        /// <param name="baseFrame">The base frame in the critter tilesheet.</param>
        /// <param name="island">Whether to create an island butterfly.</param>
        /// <param name="summer">Whether the butterfly is a summer butterfly.</param>
        public static CritterBuilder ForButterfly(int baseFrame, bool island = false, bool summer = false)
        {
            return new(
                makeCritter: (x, y, variation) =>
                {
                    Butterfly butterfly = new Butterfly(Game1.currentLocation, new Vector2(x, y), islandButterfly: island)
                    {
                        baseFrame = baseFrame,
                        sprite = { CurrentFrame = baseFrame }
                    };

                    // Use reflection to set the private summerButterfly field
                    FieldInfo summerButterflyField = typeof(Butterfly).GetField("summerButterfly", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (summerButterflyField != null)
                    {
                        summerButterflyField.SetValue(butterfly, summer);
                    }

                    return butterfly;
                },
                isThisCritter: critter =>
                    critter is Butterfly butterfly
                    && butterfly.baseFrame == baseFrame
            );
        }

        /// <summary>Create a bird.</summary>
        /// <param name="baseFrame">The base frame in the critter tilesheet.</param>
        public static CritterBuilder ForBird(int baseFrame)
        {
            return new(
                makeCritter: (x, y, variation) => new Birdie(x, y, baseFrame),
                isThisCritter: critter => critter is Birdie birdie && birdie.baseFrame == baseFrame
            );
        }

        /// <summary>Create a cloud.</summary>
        public static CritterBuilder ForCloud()
        {
            return new(
                makeCritter: (x, y, variation) => new Cloud(new Vector2(x, y)),
                isThisCritter: critter => critter is Cloud
            );
        }

        /// <summary>Create a crow.</summary>
        public static CritterBuilder ForCrow()
        {
            return new(
                makeCritter: (x, y, variation) => new Crow(x, y),
                isThisCritter: critter => critter is Crow
            );
        }

        /// <summary>Create a firefly.</summary>
        public static CritterBuilder ForFirefly()
        {
            return new(
                makeCritter: (x, y, variation) => new Firefly(new Vector2(x, y)),
                isThisCritter: critter => critter is Firefly
            );
        }

        /// <summary>Create a frog.</summary>
        /// <param name="olive">Whether to create an olive frog.</param>
        public static CritterBuilder ForFrog(bool olive)
        {
            return new(
                makeCritter: (x, y, variation) => new Frog(new Vector2(x, y), waterLeaper: olive),
                isThisCritter: critter => critter is Frog frog && ModEntry.Instance.Helper.Reflection.GetField<bool>(frog, "waterLeaper").GetValue() == olive
            );
        }

        /// <summary>Create an owl.</summary>
        public static CritterBuilder ForOwl()
        {
            return new(
                makeCritter: (x, y, variation) => new Owl(new Vector2(x * Game1.tileSize, y * Game1.tileSize)),
                isThisCritter: critter => critter is Owl
            );
        }

        /// <summary>Create a rabbit.</summary>
        /// <param name="white">Whether to create a white rabbit.</param>
        public static CritterBuilder ForRabbit(bool white)
        {
            int baseFrame = white ? 74 : 54;

            return new(
                makeCritter: (x, y, variation) => new Rabbit(Game1.currentLocation, new Vector2(x, y), false)
                {
                    baseFrame = baseFrame
                },
                isThisCritter: critter => critter is Rabbit && critter.baseFrame == baseFrame
            );
        }

        /// <summary>Create a seagull.</summary>
        public static CritterBuilder ForSeagull()
        {
            return new(
                makeCritter: (x, y, variation) => new Seagull(new Vector2(x * Game1.tileSize, y * Game1.tileSize), Seagull.stopped),
                isThisCritter: critter => critter is Seagull
            );
        }

        /// <summary>Create a squirrel.</summary>
        public static CritterBuilder ForSquirrel()
        {
            return new(
                makeCritter: (x, y, variation) => new Squirrel(new Vector2(x, y), false),
                isThisCritter: critter => critter is Squirrel
            );
        }

        /// <summary>Create a woodpecker.</summary>
        public static CritterBuilder ForWoodpecker()
        {
            return new(
                makeCritter: (x, y, variation) => new Woodpecker(new Tree(), new Vector2(x, y)),
                isThisCritter: critter => critter is Woodpecker
            );
        }

        /// <summary>Create a monkey.</summary>
        public static CritterBuilder ForMonkey()
        {
            return new(
                makeCritter: (x, y, variation) => new CalderaMonkey(new Vector2(x, y)),
                isThisCritter: critter => critter is CalderaMonkey
            );
        }

        /// <summary>Create a parrot.</summary>
        /// <param name="color">Which type of parrot to create.</param>
        public static CritterBuilder ForParrot(string color)
        {
            int index = 0;
            if (color == "blue")
            {
                index = 2;
            }
            int minYOffset = index * 24;
            int maxYOffset = (index + 1) * 24;
            if (color == "joja")
            {
                minYOffset = 96;
                maxYOffset = minYOffset;
            }

            return new(
                makeCritter: (x, y, variation) => new OverheadParrot(new Vector2(x, y))
                {
                    sourceRect = new Rectangle(0, Game1.random.Next(minYOffset, maxYOffset + 1), 24, 24)
                },
                isThisCritter: critter =>
                    critter is OverheadParrot parrot
                    && parrot.sourceRect.Y >= minYOffset
                    && parrot.sourceRect.Y <= maxYOffset
            );
        }

        /// <summary>Create an opossum.</summary>
        public static CritterBuilder ForOpossum()
        {
            return new(
                makeCritter: (x, y, variation) => new Opossum(Game1.currentLocation, new Vector2(x, y), false),
                isThisCritter: critter => critter is Opossum
            );
        }

    }
}
