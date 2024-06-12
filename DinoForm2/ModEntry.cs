using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.Network;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;

namespace DinoForm
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string DinoFormKey = "juminos.DinoForm2";

        public static PerScreen<ICue> dinoSound = new();
        public static PerScreen<int> dinoFrame = new();
        public static PerScreen<int> frameOffset = new();
        public static PerScreen<bool> breathingFire = new();
        public static PerScreen<int> breathingTicks = new();

        public enum DinoForm
        {
            Inactive,
            Temporary,
            Active
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            helper.Events.Player.Warped += Player_Warped;

            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Buffs"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, BuffData>().Data;
                    int buffDuration = Config.Duration * 1000;
                    var dinoBuffData = new BuffData
                    {
                        DisplayName = "Pepper Rex Form",
                        Description = "You've harnessed the power of a Pepper Rex!",
                        Duration = buffDuration,
                        IconTexture = "Mods/juminos.DinoForm2.CP/DinoFormIcon",
                        Effects = new BuffAttributesData
                        {
                            Defense = Config.Defense,
                            Speed = Config.MoveSpeed
                        }
                    };
                    data.Add("juminos.DinoForm2_DinoForm", dinoBuffData);
                    SMonitor.Log("Dino Form buff created");
                });
            }
        }

        private void GameLoop_DayEnding(object? sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            ResetForm();
        }

        private void Player_Warped(object? sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (!Config.ModEnabled || e.Player != Game1.player || DinoFormStatus(e.Player) == DinoForm.Inactive)
                return;
            if (Game1.CurrentEvent != null)
            {
                ResetForm();
            }
        }

        private void GameLoop_ReturnedToTitle(object? sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            ResetForm();
        }

        private void GameLoop_DayStarted(object? sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ResetForm();
        }

        private void GameLoop_UpdateTicking(object? sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            if (!Config.ModEnabled || !Context.CanPlayerMove)
                return;
            if (Game1.player?.currentLocation != null && breathingFire.Value && DinoFormStatus(Game1.player) != DinoForm.Inactive)
            {
                if (breathingTicks.Value % 120 == 0 && Config.FireSound != "")
                {
                    Game1.player.currentLocation.playSound(Config.FireSound, null, null, StardewValley.Audio.SoundContext.Default);
                }
                if (breathingTicks.Value++ % 3 != 0)
                {
                    return;
                }
                float fire_angle = 0f;
                float mouthOffset = 56;
                Vector2 shot_origin = new Vector2((float)Game1.player.GetBoundingBox().Center.X - 32f, (float)Game1.player.GetBoundingBox().Center.Y - 32f);
                switch (Game1.player.facingDirection.Value)
                {
                    case 0:
                        shot_origin.Y -= mouthOffset + 24;
                        fire_angle = 90f;
                        break;
                    case 1:
                        shot_origin.X += mouthOffset;
                        fire_angle = 0f;
                        break;
                    case 2:
                        fire_angle = 270f;
                        shot_origin.Y += mouthOffset - 24;
                        break;
                    case 3:
                        shot_origin.X -= mouthOffset;
                        fire_angle = 180f;
                        break;
                }
                fire_angle += (float)Math.Sin((double)((float)breathingTicks.Value * 16 / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;
                Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * 3.1415926535897931 / 180.0), -(float)Math.Sin((double)fire_angle * 3.1415926535897931 / 180.0));
                shot_velocity *= 10f;
                BasicProjectile projectile = new BasicProjectile(Config.FireDamage, 10, 0, 1, 0.196349546f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", "", false, true, Game1.player.currentLocation, Game1.player, null, null);
                projectile.ignoreTravelGracePeriod.Value = true;
                projectile.maxTravelDistance.Value = Config.FireDistance;
                Game1.player.currentLocation.projectiles.Add(projectile);
            }
            else
            {
                breathingTicks.Value = 0;
            }
        }

        private void GameLoop_OneSecondUpdateTicked(object? sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || DinoFormStatus(Game1.player) != DinoForm.Active || Config.StaminaUse <= 0)
                return;
            if (Game1.player.Stamina <= Config.StaminaUse)
            {
                Transform();
                return;
            }
            Game1.player.Stamina -= Config.StaminaUse;
        }

        private void GameLoop_UpdateTicked(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.killScreen || Game1.player is null || Game1.player.health <= 0 || Game1.timeOfDay >= 2600 || Game1.eventUp || Game1.CurrentEvent != null || !Game1.player.hasBuff("juminos.DinoForm2_DinoForm"))
            {
                ResetForm();
                return;
            }

            if (DinoFormStatus(Game1.player) != DinoForm.Active && Game1.player.hasBuff("juminos.DinoForm2_DinoForm"))
            {
                SMonitor.Log("Player triggered transformation.", LogLevel.Trace);
                Transform();
            }
        }

        private void Input_ButtonsChanged(object? sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.CanPlayerMove)
                return;
            if (Config.TransformKey.JustPressed())
            {
                Transform();
            }
            else if (Config.FireKey.JustPressed() && DinoFormStatus(Game1.player) != DinoForm.Inactive)
            {
                breathingFire.Value = true;
            }
            else if (breathingFire.Value && !Config.FireKey.IsDown() && DinoFormStatus(Game1.player) != DinoForm.Inactive)
            {
                breathingFire.Value = false;
            }
        }

        private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {

                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddTextOption(
                   mod: ModManifest,
                   name: () => Helper.Translation.Get("GMCM_Option_BuffedFoods_Name"),
                   getValue: () => Config.BuffedFoods,
                   setValue: value => Config.BuffedFoods = value
               );

                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_TransformKey_Name"),
                    getValue: () => Config.TransformKey,
                    setValue: value => Config.TransformKey = value
                );


                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_FireKey_Name"),
                    getValue: () => Config.FireKey,
                    setValue: value => Config.FireKey = value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_MoveSpeed_Name"),
                    getValue: () => Config.MoveSpeed,
                    setValue: value => Config.MoveSpeed = (int)value
                );
                
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_Defense_Name"),
                    getValue: () => Config.Defense,
                    setValue: value => Config.Defense = (int)value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_StaminaUse_Name"),
                    getValue: () => Config.StaminaUse,
                    setValue: value => Config.StaminaUse = (int)value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_FireDamage_Name"),
                    getValue: () => Config.FireDamage,
                    setValue: value => Config.FireDamage = (int)value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_FireDistance_Name"),
                    getValue: () => Config.FireDistance,
                    setValue: value => Config.FireDistance = (int)value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_Duration_Name"),
                    getValue: () => Config.Duration,
                    setValue: value => Config.Duration = (int)value
                );

                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_TransformSound_Name"),
                    getValue: () => Config.TransformSound,
                    setValue: value => Config.TransformSound = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_FireSound_Name"),
                    getValue: () => Config.FireSound,
                    setValue: value => Config.FireSound = value
                );
            }

        }
    }
}