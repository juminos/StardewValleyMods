using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Monsters;
using StardewValley.Objects;

namespace ButterflyCompanion
{
    public class ButterflyTrinketEffect : TrinketEffect
    {
        private float effectTimer;
        private float effectDelay = 5000f; // 5 seconds default delay
        private float boostPower = 0.1f; // default boost power
        private int energyBoost;
        private ButterflyCompanion _companion;

        public ButterflyTrinketEffect(Trinket trinket)
            : base(trinket)
        {
        }

        public override void GenerateRandomStats(Trinket trinket)
        {
            Random random = Utility.CreateRandom(trinket.generationSeed.Value);
            int num = 1;
            if (random.NextDouble() < 0.45)
            {
                num = 2;
            }
            else if (random.NextDouble() < 0.25)
            {
                num = 3;
            }
            else if (random.NextDouble() < 0.125)
            {
                num = 4;
            }

            effectDelay = 6000 - num * 500; // decreasing delay with better trinket
            boostPower = 0.05f + (float)num * 0.05f; // increasing boost power
            energyBoost = num; // additional energy boost per interval

            trinket.descriptionSubstitutionTemplates.Add(num.ToString() ?? "");
        }

        public override void Apply(Farmer farmer)
        {
            effectTimer = 0f;
            _companion = new ButterflyCompanion();
            if (Game1.gameMode == 3)
            {
                farmer.AddCompanion(_companion);
            }

            base.Apply(farmer);
        }

        public override void Unapply(Farmer farmer)
        {
            if (_companion != null)
            {
                farmer.RemoveCompanion(_companion);
            }
            base.Unapply(farmer);
        }

        public override void Update(Farmer farmer, GameTime time, GameLocation location)
        {
            effectTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
            if (effectTimer >= effectDelay)
            {
                // Apply energy boost
                farmer.Stamina = Math.Min(farmer.MaxStamina, farmer.Stamina + energyBoost);
                farmer.addedSpeed = (int)Math.Min(farmer.addedSpeed + boostPower, 5); // Adjust speed boost
                farmer.Speed += energyBoost;

                effectTimer = 0f;
            }

            base.Update(farmer, time, location);
        }
    }
}
