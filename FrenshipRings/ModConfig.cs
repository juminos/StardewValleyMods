using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FrenshipRings
{
    public class ModConfig
    {
        /// <summary>
        /// Gets or sets a multiplicative factor which determines the number of critters to spawn.
        /// </summary>
        public int CritterSpawnMultiplier { get; set; } = 1;

        /// <summary>
        /// Gets or sets a multiplicative factor which determines the number of critters to spawn.
        /// </summary>
        public int JunimoSpawnMultiplier { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether or not audio effects should be played.
        /// </summary>
        public bool PlayAudioEffects { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not slime charmer ring should be modified.
        /// </summary>
        public bool FriendlySlimeRing { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating which button should be used for the frog ring's jump.
        /// </summary>
        public KeybindList FrogRingButton { get; set; } = new KeybindList(new(SButton.Space), new(SButton.RightStick));
        public int JumpChargeSpeed { get; set; } = 10;
        public int MaxFrogJumpDistance { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating whether or not the jump costs stamina.
        /// </summary>
        public bool JumpCostsStamina { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not the frogs should spawn in a very hot location
        /// such as the volcano or desert.
        /// </summary>
        public bool FrogsSpawnInHeat { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not frogs should spawn in a very cold location.
        /// </summary>
        public bool FrogsSpawnInCold { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not frogs can only spawn outdoors in rain.
        /// </summary>
        public bool FrogsSpawnOnlyInRain { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not frogs can spawn in areas that are saltwater.
        /// </summary>
        public bool SaltwaterFrogs { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not frogs can spawn indoors.
        /// </summary>
        public bool IndoorFrogs { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not butterflies should spawn if it's rainy out.
        /// </summary>
        public bool ButterfliesSpawnInRain { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not owls should spawn indoors.
        /// </summary>
        public bool OwlsSpawnIndoors { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether or not owls should spawn during the day.
        /// </summary>
        public bool OwlsSpawnDuringDay { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating how expensive the bunny ring's dash should be.
        /// </summary>
        public int BunnyRingStamina { get; set; } = 10;

        /// <summary>
        /// Gets or sets a value indicating how big of a speed boost the bunny ring's dash should be.
        /// </summary>
        public int BunnyRingBoost {  get; set; } = 3;

        /// <summary>
        /// Gets or sets a value indicating which button should be used for the bunny ring's stamina-sprint.
        /// </summary>
        public KeybindList BunnyRingButton { get; set; } = new KeybindList(new(SButton.LeftShift), new(SButton.LeftStick));
    }

}
