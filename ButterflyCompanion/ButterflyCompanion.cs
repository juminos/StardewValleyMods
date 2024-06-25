using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Companions;

namespace ButterflyCompanion
{
    public class ButterflyCompanion : Companion
    {
        private float flitTimer;
        private Vector2 extraPosition;
        private Vector2 extraPositionMotion;
        private Vector2 extraPositionAcceleration;
        private bool floatup;
        private int currentSidewaysFlap;
        private int lightID = 301579;
        private int flapTimer;
        private int flapFrame;
        private int flapSpeed = 80;
        private int startingYForVariant = 144;
        private readonly NetInt butterflySpriteIndex = new NetInt(-1); // NetInt for network synchronization

        public ButterflyCompanion() : base()
        {
        }

        public ButterflyCompanion(int subvariant) : base()
        {
            this.butterflySpriteIndex.Value = subvariant;
            if (subvariant == -1)
            {
                Random random = Utility.CreateRandom(Game1.player.uniqueMultiplayerID.Value);
                butterflySpriteIndex.Value = random.Next(4); // Randomly selects one of the four variations
            }
            else
            {
                butterflySpriteIndex.Value = subvariant;
            }
        }

        public override void InitNetFields()
        {
            base.InitNetFields();
            base.NetFields.AddField(butterflySpriteIndex, "butterflySpriteIndex");
        }

        public override void Draw(SpriteBatch b)
        {
            if (base.Owner == null || base.Owner.currentLocation == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
            {
                return;
            }

            Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
            SpriteEffects effects = SpriteEffects.None;

            b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - height) * 4f) + extraPosition),
                new Rectangle((int)butterflySpriteIndex * 64 + flitTimer <= flapTimer ? 0 : (int)((flitTimer - flapTimer + flapSpeed) / flapSpeed) * 16, startingYForVariant, 16, 16),
                Color.White, 0f, new Vector2(8f, 8f), 4f, effects, _position.Y / 10000f);

            b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(extraPosition.X, 0f)),
                Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                3f * Utility.Lerp(1f, 0.8f, Math.Min(height, 1f)), SpriteEffects.None, (_position.Y - 8f) / 10000f - 2E-06f);
        }

        public override void Update(GameTime time, GameLocation location)
        {
            base.Update(time, location);
            height = 32f;

            flapTimer = 200 + Game1.random.Next(-5, 6);

            flitTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
            if (flitTimer > (float)(3 * flapSpeed + flapTimer))
            {
                flapFrame = 0;
                flitTimer = 0f;
                extraPositionMotion = new Vector2((Game1.random.NextDouble() < 0.5) ? 0.1f : (-0.1f), -2f);
                if (extraPositionMotion.X < 0f)
                {
                    currentSidewaysFlap--;
                }
                else
                {
                    currentSidewaysFlap++;
                }

                if (currentSidewaysFlap < -6 || currentSidewaysFlap > 6)
                {
                    extraPositionMotion.X *= -1f;
                }

                extraPositionAcceleration = new Vector2(0f, floatup ? 0.15f : 0.16f);
                if (extraPosition.Y > 8f)
                {
                    floatup = true;
                    //extraPosition.Y = 8f; // reset y position if too low
                }
                else if (extraPosition.Y < -8f)
                {
                    floatup = false;
                    //extraPosition.Y = -8f; // reset y position if too high
                }
            }
            extraPosition += extraPositionMotion;
            extraPositionMotion += extraPositionAcceleration;
            Utility.repositionLightSource(lightID, base.Position - new Vector2(0f, height * 4f) + extraPosition);
        }

        public override void InitializeCompanion(Farmer farmer)
        {
            base.InitializeCompanion(farmer);
            lightID = Game1.random.Next();
            Game1.currentLightSources.Add(new LightSource(1, base.Position, 2f, Color.Black, lightID, LightSource.LightContext.None, 0L));

            if ((int)butterflySpriteIndex == -1)
            {
                Random random = Utility.CreateRandom(Game1.player.uniqueMultiplayerID.Value);
                butterflySpriteIndex.Value = random.Next(4); // Randomly selects one of the four variations
            }
        }

        public override void CleanupCompanion()
        {
            base.CleanupCompanion();
            Utility.removeLightSource(lightID);
        }

        public override void OnOwnerWarp()
        {
            base.OnOwnerWarp();
            extraPosition = Vector2.Zero;
            extraPositionMotion = Vector2.Zero;
            extraPositionAcceleration = Vector2.Zero;
            lightID = Game1.random.Next();
            Game1.currentLightSources.Add(new LightSource(1, base.Position, 2f, Color.Black, lightID, LightSource.LightContext.None, 0L));
        }

        public override void Hop(float amount)
        {
        }

    }
}