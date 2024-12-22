using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using Microsoft.Xna.Framework.Graphics;
using FrenshipRings.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using FrenshipRings.Framework.Managers;
using FrenshipRings.Utilities;
using FrenshipRings.MigrationManager;

namespace FrenshipRings
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;

        private static readonly PerScreen<BunnySpawnManager?> BunnyManagers = new(() => null);

        private static readonly PerScreen<JumpManager?> JumpManagers = new(() => null);

        private FrenshipRings.MigrationManager.MigrationManager? migrator;

        public static ModConfig Config;

        internal static bool shadowDisabled = false;
        internal static bool spiderDisabled = false;
        internal static bool dustDisabled = false;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            SMonitor = Monitor;
            SHelper = helper;

            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.Player.Warped += OnWarp;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;

        }

        private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsPlayerFree)
            {
                return;
            }
            if (!Game1.player.UsingTool &&
                !Game1.player.isEating &&
                Game1.player.yJumpOffset == 0 &&
                Config.MaxFrogJumpDistance > 0 &&
                Config.FrogRingButton.Keybinds.FirstOrDefault(k => k.GetState() == SButtonState.Pressed) is Keybind keybind &&
                Game1.player.isWearingRing("juminos.FrenshipRings.CP_Frog"))
            {
                if (JumpManagers.Value?.IsValid(out _) == true)
                {
                    Monitor.Log($"Jump already in progress for this player, skipping.");
                }

                else if (Game1.player.isRidingHorse())
                {
                    Game1.showRedMessage(I18n.FrogRing_Horse());
                }
                else if (Game1.player.exhausted.Value || (Game1.player.Stamina < Config.MaxFrogJumpDistance && Config.JumpCostsStamina))
                {
                    Game1.showRedMessage(I18n.BunnyBuff_Tired());
                }
                else
                {
                    JumpManagers.Value?.Dispose();
                    JumpManagers.Value = new(Game1.player, this.Helper.Events.GameLoop, this.Helper.Events.Display, keybind);
                }

            }

            if (Config.BunnyRingBoost > 0 && 
                Config.BunnyRingButton.JustPressed() &&
                Game1.player.isWearingRing("juminos.FrenshipRings.CP_Bunny") && 
                !Game1.player.hasBuff("juminos.FrenshipRings.CP_BunnyBuff"))
            {
                if (Game1.player.Stamina >= Config.BunnyRingStamina && !Game1.player.exhausted.Value)
                {
                    Buff buff = new Buff(
                        id: "juminos.FrenshipRings_BunnyBuff",
                        displayName: I18n.BunnyRing_Name(),
                        description: I18n.BunnyBuff_Description(Config.BunnyRingBoost),
                        iconTexture: this.Helper.ModContent.Load<Texture2D>("assets/bunnies_fast.png"),
                        iconSheetIndex: 1,
                        duration: 10000,
                        effects: new StardewValley.Buffs.BuffEffects()
                        {
                            Speed = { Config.BunnyRingBoost }
                        });

                    Game1.player.applyBuff(buff);
                    // Game1.buffsDisplay..addOtherBuff(buff); /// not needed?
                    Game1.player.Stamina -= Config.BunnyRingStamina;
                }
                else
                {
                    Game1.showRedMessage(I18n.BunnyBuff_Tired());
                }
            }
        }

        /// <inheritdoc cref="IPlayerEvents.Warped"/>
        [EventPriority(EventPriority.Low)]
        private void OnWarp(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer)
            {
                SMonitor.Log("failed local player check", LogLevel.Trace);
                return;
            }

            // forcibly end the jump if the player was in one.
            JumpManagers.Value?.Dispose();
            JumpManagers.Value = null;

            if (Config.CritterSpawnMultiplier == 0)
            {
                SMonitor.Log("critter spawn multiplier is zero", LogLevel.Trace);
                return;
            }

            e.NewLocation?.instantiateCrittersList();
            if (e.NewLocation?.critters is not List<Critter> critters)
            {
                SMonitor.Log("critters list not loaded", LogLevel.Trace);
                return;
            }
            if (Game1.isDarkOut(Game1.currentLocation))
            {
                if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Firefly"))
                {
                    CRUtils.SpawnFirefly(critters, 5);
                }
            }
            else if (e.NewLocation.ShouldSpawnButterflies() && 
                Game1.player.isWearingRing("juminos.FrenshipRings.CP_Butterfly"))
            {
                SMonitor.Log("butterfly spawning check passed", LogLevel.Trace);
                CRUtils.SpawnButterfly(critters, 3);
            }
            if (e.NewLocation is not Caldera && Game1.player.isWearingRing("juminos.FrenshipRings.CP_Bunny"))
            {
                if (BunnyManagers.Value?.IsValid() == false)
                {
                    BunnyManagers.Value.Dispose();
                    BunnyManagers.Value = null;
                }
                BunnyManagers.Value ??= new(this.Monitor, Game1.player, this.Helper.Events.Player);
                CRUtils.AddBunnies(critters, 5, BunnyManagers.Value.GetTrackedBushes());
            }
            if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Frog") && e.NewLocation.ShouldSpawnFrogs())
            {
                CRUtils.SpawnFrogs(e.NewLocation, critters, 5);
            }

            if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Owl") && e.NewLocation.ShouldSpawnOwls())
            {
                CRUtils.SpawnOwls(e.NewLocation, critters, 1);
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.TimeChanged"/>
        private void OnTimeChanged(object? sender, TimeChangedEventArgs e)
        {
            if (Config.CritterSpawnMultiplier == 0)
            {
                return;
            }
            Game1.currentLocation?.instantiateCrittersList();
            if (Game1.currentLocation?.critters is not List<Critter> critters)
            {
                return;
            }
            if (Game1.isDarkOut(Game1.currentLocation))
            {
                if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Firefly"))
                {
                    CRUtils.SpawnFirefly(critters, Game1.player.GetEffectsOfRingMultiplier("juminos.FrenshipRings.CP_Firefly"));
                }
            }
            else if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Butterfly") && Game1.currentLocation.ShouldSpawnButterflies())
            {
                CRUtils.SpawnButterfly(critters, Game1.player.GetEffectsOfRingMultiplier("juminos.FrenshipRings.CP_Butterfly"));
            }
            if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Frog") && Game1.currentLocation.ShouldSpawnFrogs())
            {
                CRUtils.SpawnFrogs(Game1.currentLocation, critters, Game1.player.GetEffectsOfRingMultiplier("juminos.FrenshipRings.CP_Frog"));
            }

            if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Owl") && Game1.currentLocation.ShouldSpawnOwls())
            {
                CRUtils.SpawnOwls(Game1.currentLocation, critters, Game1.player.GetEffectsOfRingMultiplier("juminos.FrenshipRings.CP_Owl"));
            }

            if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Bunny") && Game1.currentLocation is not Caldera)
            {
                if (BunnyManagers.Value?.IsValid() == false)
                {
                    BunnyManagers.Value.Dispose();
                    BunnyManagers.Value = null;
                }
                BunnyManagers.Value ??= new(this.Monitor, Game1.player, this.Helper.Events.Player);
                CRUtils.AddBunnies(critters, Game1.player.GetEffectsOfRingMultiplier("juminos.FrenshipRings.CP_Bunny"), BunnyManagers.Value.GetTrackedBushes());
            }
        }

        /// <inheritdoc cref="IGameLoopEvents.SaveLoaded"/>
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            MultiplayerHelpers.AssertMultiplayerVersions(this.Helper.Multiplayer, this.ModManifest, this.Monitor, this.Helper.Translation);

            this.migrator = new(this.ModManifest, this.Helper, this.Monitor);
            if (!this.migrator.CheckVersionInfo())
            {
                this.Helper.Events.GameLoop.Saved += this.WriteMigrationData;
            }
            else
            {
                this.migrator = null;
            }

            if (Context.IsMainPlayer)
            {
                // hook event to save Ids so future migrations are possible.
                // this.Helper.Events.GameLoop.Saving -= this.OnSaving; /// not needed?
                // this.Helper.Events.GameLoop.Saving += this.OnSaving; /// not needed?
            }
        }

        /// <summary>
        /// Resets the managers when returning to the title.
        /// </summary>
        /// <param name="sender">SMAPI.</param>
        /// <param name="e">Event args.</param>
        [EventPriority(EventPriority.High)]
        private void OnReturnedToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            // reset and yeet managers.
            foreach ((_, JumpManager? value) in JumpManagers.GetActiveValues())
            {
                value?.Dispose();
            }
            JumpManagers.ResetAllScreens();
            foreach ((_, BunnySpawnManager? value) in BunnyManagers.GetActiveValues())
            {
                value?.Dispose();
            }
            BunnyManagers.ResetAllScreens();
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(20) || !Context.IsWorldReady || Game1.player == null)
                return;

            var currentLocation = Game1.player.currentLocation;

            // Check for shadow people in current location
            var anyShadowPeople = currentLocation.characters.Any(c => c is ShadowShaman || c is ShadowBrute || c is ShadowGuy || c is ShadowGirl);

            if (anyShadowPeople)
            {
                // Check for players wearing shadow ring
                var anyShadowRing = currentLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.FrenshipRings.CP_Shadow"));

                if (anyShadowRing && !shadowDisabled)
                {
                    Monitor.Log($"Attempting to disable shadow aggression at {currentLocation}.");
                    Shadow.DisableShadow(currentLocation);
                    shadowDisabled = true;
                }
                else if (!anyShadowRing && shadowDisabled)
                {
                    Monitor.Log($"Attempting to enable shadow aggression at {currentLocation}.");
                    Shadow.EnableShadow(currentLocation);
                    shadowDisabled = false;
                }
            }

            // Check for spiders in current location
            var anySpiders = currentLocation.characters.Any(c => c is Leaper);

            if (anySpiders)
            {
                // Check for players wearing spider ring
                var anySpiderRing = currentLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.FrenshipRings.CP_Spider"));

                if (anySpiderRing && !spiderDisabled)
                {
                    Monitor.Log($"Attempting to disable spider aggression at {currentLocation}.");
                    Spider.DisableSpider(currentLocation);
                    spiderDisabled = true;
                }
                else if (!anySpiderRing && spiderDisabled)
                {
                    Monitor.Log($"Attempting to enable spider aggression at {currentLocation}.");
                    Spider.EnableSpider(currentLocation);
                    spiderDisabled = false;
                }
            }

            // Check for dust spirit in current location
            var anyDust = currentLocation.characters.Any(c => c is DustSpirit);

            if (anyDust)
            {
                // Check for players wearing dust ring
                var anyDustRing = currentLocation.farmers.Any(farmer => farmer.isWearingRing("juminos.FrenshipRings.CP_Dust"));

                if (anyDustRing && !dustDisabled)
                {
                    Monitor.Log($"Attempting to disable dust spirit aggression at {currentLocation}.");
                    Dust.DisableDust(currentLocation);
                    dustDisabled = true;
                }
                else if (!anyDustRing && dustDisabled)
                {
                    Monitor.Log($"Attempting to enable dust spirit aggression at {currentLocation}.");
                    Dust.EnableDust(currentLocation);
                    dustDisabled = false;
                }
            }
        }

        private void OnGameLaunched(object? sender, EventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => this.Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Play Audio Effects",
                tooltip: () => "Whether or not audio effects should be played",
                getValue: () => Config.PlayAudioEffects,
                setValue: value => Config.PlayAudioEffects = value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Critter Spawn Multiplier",
                tooltip: () => "Multiplicative factor which determines the number of critters to spawn",
                getValue: () => Config.CritterSpawnMultiplier,
                setValue: value => Config.CritterSpawnMultiplier = (int)value,
                min: 0,
                max: 5,
                interval: 1
                );

            // Frog ring category

            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => "Frog Ring Button",
                tooltip: () => "Which button should be used for the frog ring's jump",
                getValue: () => Config.FrogRingButton,
                setValue: value => Config.FrogRingButton = value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Max Frog Jump Distance",
                tooltip: () => "Maximum distance you can jump with the Frog Ring",
                getValue: () => Config.MaxFrogJumpDistance,
                setValue: value => Config.MaxFrogJumpDistance = (int)value,
                min: 0,
                max: 20,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Jump Charge Speed",
                tooltip: () => "How fast jumping distance charges with the Frog Ring",
                getValue: () => Config.JumpChargeSpeed,
                setValue: value => Config.JumpChargeSpeed = (int)value,
                min: 1,
                max: 40,
                interval: 1
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Jump Costs Stamina",
                tooltip: () => "Whether or not the jump costs stamina",
                getValue: () => Config.JumpCostsStamina,
                setValue: value => Config.JumpCostsStamina = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Frogs Spawn In Heat",
                tooltip: () => "Whether or not the frogs should spawn in a very hot location \nsuch as the volcano or desert",
                getValue: () => Config.FrogsSpawnInHeat,
                setValue: value => Config.FrogsSpawnInHeat = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Frogs Spawn In Cold",
                tooltip: () => "Whether or not frogs should spawn in a very cold location",
                getValue: () => Config.FrogsSpawnInCold,
                setValue: value => Config.FrogsSpawnInCold = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Frogs Spawn Only In Rain",
                tooltip: () => "Whether or not frogs can only spawn outdoors in rain",
                getValue: () => Config.FrogsSpawnOnlyInRain,
                setValue: value => Config.FrogsSpawnOnlyInRain = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Saltwater Frogs",
                tooltip: () => "Whether or not frogs can spawn in areas that are saltwater",
                getValue: () => Config.SaltwaterFrogs,
                setValue: value => Config.SaltwaterFrogs = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Indoor Frogs",
                tooltip: () => "Whether or not frogs can spawn indoors",
                getValue: () => Config.IndoorFrogs,
                setValue: value => Config.IndoorFrogs = value
                );

            // Butterfly ring category

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Butterflies Spawn In Rain",
                tooltip: () => "Whether or not butterflies should spawn if it's rainy out",
                getValue: () => Config.ButterfliesSpawnInRain,
                setValue: value => Config.ButterfliesSpawnInRain = value
                );

            // Owl ring category

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Owls Spawn Indoors",
                tooltip: () => "Whether or not owls should spawn indoors",
                getValue: () => Config.OwlsSpawnIndoors,
                setValue: value => Config.OwlsSpawnIndoors = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => "Owls Spawn During Day",
                tooltip: () => "Whether or not owls should spawn during the day",
                getValue: () => Config.OwlsSpawnDuringDay,
                setValue: value => Config.OwlsSpawnDuringDay = value
                );

            // Bunny ring category

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Bunny Ring Stamina",
                tooltip: () => "How expensive the bunny ring's dash should be",
                getValue: () => Config.BunnyRingStamina,
                setValue: value => Config.BunnyRingStamina = (int)value,
                min: 0,
                max: 50,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => "Bunny Ring Boost",
                tooltip: () => "How big of a speed boost the bunny ring's dash should be",
                getValue: () => Config.BunnyRingBoost,
                setValue: value => Config.BunnyRingBoost = (int)value,
                min: 0,
                max: 10,
                interval: 1
                );

            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: () => "Bunny Ring Button",
                tooltip: () => "Which button should be used for the bunny ring's stamina-sprint",
                getValue: () => Config.BunnyRingButton,
                setValue: value => Config.BunnyRingButton = value
                );
        }

        #region migration

        /// <inheritdoc cref="IGameLoopEvents.Saved"/>
        /// <remarks>
        /// Writes migration data then detaches the migrator.
        /// </remarks>
        private void WriteMigrationData(object? sender, SavedEventArgs e)
        {
            if (this.migrator is not null)
            {
                this.migrator.SaveVersionInfo();
                this.migrator = null;
            }

            this.Helper.Events.GameLoop.Saved -= this.WriteMigrationData;
        }
        #endregion

    }
}
