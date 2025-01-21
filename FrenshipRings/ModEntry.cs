using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using FrenshipRings.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using FrenshipRings.Framework.Managers;
using FrenshipRings.Utilities;
using HarmonyLib;
using FrenshipRings.HarmonyPatches.OwlRing;
using FrenshipRings.HarmonyPatches;
using FrenshipRings.HarmonyPatches.ShadowRing;

namespace FrenshipRings
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;

        private static readonly PerScreen<BunnySpawnManager?> BunnyManagers = new(() => null);

        private static readonly PerScreen<JumpManager?> JumpManagers = new(() => null);

        /// <summary>
        /// Gets a reference to the current jumpManager, if applicable.
        /// </summary>
        internal static JumpManager? CurrentJumper => JumpManagers.Value;

        private FrenshipRings.MigrationManager.MigrationManager? migrator;

        public static ModConfig Config;

        internal static int junimoCount = 0;

        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            SMonitor = Monitor;
            SHelper = helper;

            try
            {
                var harmony = new Harmony(this.ModManifest.UniqueID);

                new BaseSightPatch(SMonitor, SHelper).ApplyPatch(harmony);
                new IsInvinciblePatch(SMonitor, SHelper).ApplyPatch(harmony);
                new OverlapFarmerDamagePatch(SMonitor, SHelper).ApplyPatch(harmony);
                new ShooterBehaviorPatch(SMonitor, SHelper).ApplyPatch(harmony);
                new ShamanBehaviorPatch(SMonitor, SHelper).ApplyPatch(harmony);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            Config = helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.Player.Warped += OnWarp;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            //helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += OnReturnedToTitle;
            helper.Events.Input.ButtonsChanged += OnButtonsChanged;
        }

        //private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        //{
        //    if (!Context.IsPlayerFree)
        //    {
        //        return;
        //    }
        //    if (Game1.player.isMoving() &&
        //        Game1.player.isWearingRing("juminos.FrenshipRings.CP_Junimo"))
        //    {
                
        //    }
        //    //warp junimo if distance from player greater than x
        //    //increase junimo speed +1 if distance from player greater than x
        //    //reset junimo speed to match player (+1?) if distance from player less than x
        //}
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
                    Game1.player.applyBuff("juminos.FrenshipRings.CP_BunnyBuff");
                    Game1.player.Stamina -= Config.BunnyRingStamina;
                }
                else
                {
                    Game1.showRedMessage(I18n.BunnyBuff_Tired());
                }
            }
            //if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Junimo"))
            //{
            //    CRUtils.SpawnJunimo(Game1.currentLocation, Config.JunimoSpawnMultiplier);
            //}
            //if (!Game1.player.isWearingRing("juminos.FrenshipRings.CP_Junimo") && junimoCount > 0)
            //{
            //    CRUtils.RemoveJunimos(Game1.currentLocation);
            //}
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
            //if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Junimo"))
            //{
            //    CRUtils.WarpJunimos(e.OldLocation, e.NewLocation);
            //}
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
                name: I18n.PlayAudioEffects_Title,
                tooltip: I18n.PlayAudioEffects_Description,
                getValue: () => Config.PlayAudioEffects,
                setValue: value => Config.PlayAudioEffects = value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.CritterSpawnMultiplier_Title,
                tooltip: I18n.CritterSpawnMultiplier_Description,
                getValue: () => Config.CritterSpawnMultiplier,
                setValue: value => Config.CritterSpawnMultiplier = (int)value,
                min: 0,
                max: 5,
                interval: 1
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.FriendlySlimeRing_Title,
                tooltip: I18n.FriendlySlimeRing_Description,
                getValue: () => Config.FriendlySlimeRing,
                setValue: value => Config.FriendlySlimeRing = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.LethalRings_Title,
                tooltip: I18n.LethalRings_Description,
                getValue: () => Config.LethalRings,
                setValue: value => Config.LethalRings = value
                );

            // Frog ring category

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: I18n.FrogRing_Title
                );

            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: I18n.FrogRingButton_Title,
                tooltip: I18n.FrogRingButton_Description,
                getValue: () => Config.FrogRingButton,
                setValue: value => Config.FrogRingButton = value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.MaxFrogJumpDistance_Title,
                tooltip: I18n.MaxFrogJumpDistance_Description,
                getValue: () => Config.MaxFrogJumpDistance,
                setValue: value => Config.MaxFrogJumpDistance = (int)value,
                min: 0,
                max: 20,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.JumpChargeSpeed_Title,
                tooltip: I18n.JumpChargeSpeed_Description,
                getValue: () => Config.JumpChargeSpeed,
                setValue: value => Config.JumpChargeSpeed = (int)value,
                min: 1,
                max: 40,
                interval: 1
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.JumpCostsStamina_Title,
                tooltip: I18n.JumpCostsStamina_Description,
                getValue: () => Config.JumpCostsStamina,
                setValue: value => Config.JumpCostsStamina = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.FrogsSpawnInHeat_Title,
                tooltip: I18n.FrogsSpawnInHeat_Description,
                getValue: () => Config.FrogsSpawnInHeat,
                setValue: value => Config.FrogsSpawnInHeat = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.FrogsSpawnInCold_Title,
                tooltip: I18n.FrogsSpawnInCold_Description,
                getValue: () => Config.FrogsSpawnInCold,
                setValue: value => Config.FrogsSpawnInCold = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.FrogsSpawnOnlyInRain_Title,
                tooltip: I18n.FrogsSpawnOnlyInRain_Description,
                getValue: () => Config.FrogsSpawnOnlyInRain,
                setValue: value => Config.FrogsSpawnOnlyInRain = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.SaltwaterFrogs_Title,
                tooltip: I18n.SaltwaterFrogs_Description,
                getValue: () => Config.SaltwaterFrogs,
                setValue: value => Config.SaltwaterFrogs = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.IndoorFrogs_Title,
                tooltip: I18n.IndoorFrogs_Description,
                getValue: () => Config.IndoorFrogs,
                setValue: value => Config.IndoorFrogs = value
                );

            // Butterfly ring category

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: I18n.ButterflyRing_Title
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.ButterfliesSpawnInRain_Title,
                tooltip: I18n.ButterfliesSpawnInRain_Description,
                getValue: () => Config.ButterfliesSpawnInRain,
                setValue: value => Config.ButterfliesSpawnInRain = value
                );

            // Owl ring category

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: I18n.OwlRing_Title
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.OwlsSpawnIndoors_Title,
                tooltip: I18n.OwlsSpawnIndoors_Description,
                getValue: () => Config.OwlsSpawnIndoors,
                setValue: value => Config.OwlsSpawnIndoors = value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.OwlsSpawnDuringDay_Title,
                tooltip: I18n.OwlsSpawnDuringDay_Description,
                getValue: () => Config.OwlsSpawnDuringDay,
                setValue: value => Config.OwlsSpawnDuringDay = value
                );

            // Bunny ring category

            configMenu.AddSectionTitle(
                mod: this.ModManifest,
                text: I18n.BunnyRing_Title
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.BunnyRingStamina_Title,
                tooltip: I18n.BunnyRingStamina_Description,
                getValue: () => Config.BunnyRingStamina,
                setValue: value => Config.BunnyRingStamina = (int)value,
                min: 0,
                max: 50,
                interval: 1
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.BunnyRingBoost_Title,
                tooltip: I18n.BunnyRingBoost_Description,
                getValue: () => Config.BunnyRingBoost,
                setValue: value => Config.BunnyRingBoost = (int)value,
                min: 0,
                max: 10,
                interval: 1
                );

            configMenu.AddKeybindList(
                mod: this.ModManifest,
                name: I18n.BunnyRingButton_Title,
                tooltip: I18n.BunnyRingButton_Description,
                getValue: () => Config.BunnyRingButton,
                setValue: value => Config.BunnyRingButton = value
                );

            // Junimo ring category

            //configMenu.AddSectionTitle(
            //    mod: this.ModManifest,
            //    text: I18n.JunimoRing_Title
            //    );


            //configMenu.AddNumberOption(
            //    mod: this.ModManifest,
            //    name: I18n.JunimoSpawnMultiplier_Title,
            //    tooltip: I18n.JunimoSpawnMultiplier_Description,
            //    getValue: () => Config.JunimoSpawnMultiplier,
            //    setValue: value => Config.JunimoSpawnMultiplier = (int)value,
            //    min: 1,
            //    max: 5,
            //    interval: 1
            //    );
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
