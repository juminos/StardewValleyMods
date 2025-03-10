﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;

namespace HorseReskin
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;
        internal static IManifest SModManifest;

        internal static bool IsEnabled = true; // Whether the mod is enabled for the current farmhand

        private static Framework.HorseReskinModConfig config;

        private static Dictionary<Guid, int> horseSkinMap = new Dictionary<Guid, int>();
        private static Dictionary<Guid, Horse> horseIdMap = new Dictionary<Guid, Horse>();
        private static Dictionary<int, Texture2D> skinTextureMap = new Dictionary<int, Texture2D>();

        //constants
        internal static readonly string ReskinHorseMessageId = "HorseReskin"; // A request from a farmhand to reskin a horse
        internal static readonly string ReloadHorseSpritesMessageId = "HorseSpriteReload"; // Inform farmhands to update horse sprites
        private readonly uint TextureUpdateRateWithSinglePlayer = 30;
        private readonly uint TextureUpdateRateWithMultiplePlayers = 3;

        // The minimum version the host must have for the mod to be enable on a farmhand.
        private readonly string MinHostVersion = "1.0.0";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mads.</param>
        public override void Entry(IModHelper helper)
        {
            // Static variables
            SMonitor = Monitor;
            SHelper = helper;
            SModManifest = ModManifest;

            // Events
            IModEvents events = helper.Events;
            events.GameLoop.GameLaunched += this.OnGameLaunched;
            events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            events.GameLoop.DayStarted += this.OnDayStarted;
            events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            events.Input.ButtonPressed += this.OnButtonPressed;
            events.Multiplayer.ModMessageReceived += this.OnModMessageReceived;
            events.Multiplayer.PeerConnected += this.OnPeerConnected;

            // SMAPI Commands
            SHelper.ConsoleCommands.Add("list_horses", "Lists the names of all horses on your farm.", Framework.CommandHandler.OnCommandReceived);
            SHelper.ConsoleCommands.Add("reskin_horse", "Specify [horse name] and the [skin id] (1-31) you want to assign to it. Try list_horses to see available horses.", Framework.CommandHandler.OnCommandReceived);
            SHelper.ConsoleCommands.Add("reskin_horse_id", "Specify [horse id] and the [skin id] (1-31) you want to assign to it. Try list_horses to see available horses.", Framework.CommandHandler.OnCommandReceived);
        }

        /*********
        ** Event Listeners
        *********/
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // Read config
            config = SHelper.ReadConfig<Framework.HorseReskinModConfig>();

            // Add Generic Mod Config Menu integration
            var api = SHelper.ModRegistry.GetApi<Common.GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (api != null)
            {
                api.RegisterModConfig(ModManifest, () => config = new Framework.HorseReskinModConfig(), () => SHelper.WriteConfig(config));
                api.RegisterSimpleOption(ModManifest, "Total horse skin assets", "The number of .png files in the /assets folder for this mod", () => config.AmountOfHorseSkins, (int val) => config.AmountOfHorseSkins = val);
            }
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            // check if mod should be enable for the current player
            IsEnabled = Context.IsMainPlayer;
            if (!IsEnabled)
            {
                ISemanticVersion playerVersion = this.ModManifest.Version;
                ISemanticVersion hostVersion = SHelper.Multiplayer.GetConnectedPlayer(Game1.MasterPlayer.UniqueMultiplayerID)?.GetMod(this.ModManifest.UniqueID)?.Version;

                if (hostVersion == null)
                {
                    IsEnabled = false;
                    SMonitor.Log("This mod is disabled because the host player doesn't have it installed.", LogLevel.Warn);
                    return;
                }
                else if (hostVersion.IsOlderThan(this.MinHostVersion))
                {
                    IsEnabled = false;
                    SMonitor.Log($"This mod is disabled because the host player has {this.ModManifest.Name} {hostVersion}, but the minimum compatible version is {this.MinHostVersion}.", LogLevel.Warn);
                    return;
                }
                else if (!this.ModManifest.Version.Equals(hostVersion))
                {
                    IsEnabled = false;
                    SMonitor.Log($"This mod is disabled because the host player has {this.ModManifest.Name} {hostVersion}, but you are using version {playerVersion}. Make sure you both have the same version", LogLevel.Warn);
                    return;
                }
                else
                    IsEnabled = true;
            }

            // initialize maps and sprites
            if (Context.IsMainPlayer)
            {
                horseIdMap.Clear();
                horseIdMap = GetHorsesDict();
                foreach (var d in horseIdMap)
                    GenerateHorseSkinMap(d.Value);
                LoadAllSprites();
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!IsEnabled)
                return;
            horseIdMap.Clear();
            horseIdMap = GetHorsesDict();

            foreach (var d in horseIdMap)
                ReLoadHorseSprites(d.Value);
        }

        private void OnPeerConnected(object sender, PeerConnectedEventArgs e)
        {
            if (!Context.IsMainPlayer)
                return;

            foreach (var d in horseSkinMap)
            {
                var horseId = d.Key;
                var skinId = d.Value;
                SendMultiplayerReloadSkinMessage(horseId, skinId);
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!IsEnabled)
                return;

            // multiplayer: override textures in the current location
            if (Context.IsWorldReady && Game1.currentLocation != null)
            {
                uint updateRate = Game1.currentLocation.farmers.Count > 1 ? TextureUpdateRateWithMultiplePlayers : TextureUpdateRateWithSinglePlayer;
                if (e.IsMultipleOf(updateRate))
                {
                    foreach (Horse horse in GetHorsesIn(Game1.currentLocation))
                        if (horseSkinMap.ContainsKey(horse.HorseId) && skinTextureMap.ContainsKey(horseSkinMap[horse.HorseId]))
                            horse.Sprite.spriteTexture = skinTextureMap[horseSkinMap[horse.HorseId]];
                }
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            if (!IsEnabled)
                return;

            if (!Game1.player.currentLocation.IsFarm)
                return;

            bool IsPlayerInStable(Building stable)
            {
                var stableRect = new Rectangle(stable.tileX.Value, stable.tileY.Value, stable.tilesWide.Value, stable.tilesHigh.Value);
                if (stableRect.Contains(Game1.player.TilePoint)) // player standing in stable
                    return true;
                return false;
            }

            if (e.Button.IsActionButton())
            {
                foreach (Stable stable in GetHorseStables())
                {
                    if (IsPlayerInStable(stable))
                    {
                        if (Game1.activeClickableMenu == null)
                        {
                            Game1.activeClickableMenu = new Framework.HorseReskinMenu(stable.HorseId, skinTextureMap);
                        }
                        break;
                    }
                }
            }
        }

        private void OnModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.Type == ReskinHorseMessageId && Context.IsMainPlayer && e.FromModID == SModManifest.UniqueID)
            {
                Framework.HorseReskinMessage message = e.ReadAs<Framework.HorseReskinMessage>();
                SaveHorseReskin(message.horseId, message.skinId);
                return;
            }
            if (e.Type == ReloadHorseSpritesMessageId && !Context.IsMainPlayer && e.FromModID == SModManifest.UniqueID)
            {
                if (horseIdMap.Count <= 0)
                {
                    horseIdMap = GetHorsesDict();
                    LoadAllSprites();
                }

                Framework.HorseReskinMessage message = e.ReadAs<Framework.HorseReskinMessage>();
                UpdateHorseSkinMap(message.horseId, message.skinId);
                ReLoadHorseSprites(GetHorseById(message.horseId));
            }
        }

        /*********
        ** Public methods
        *********/

        /// <summary> Gets all horses in game </summary>
        /// <returns>Dictionary of horses</returns>
        public static Dictionary<Guid, Horse> GetHorsesDict()
        {
            Dictionary<Guid, Horse> horses = new Dictionary<Guid, Horse>();

            // Mounted Horses
            foreach (Farmer player in Game1.getAllFarmers())
            {
                if (player.mount != null)
                {
                    Horse mountedHorse = player.mount;
                    if (!horses.ContainsKey(mountedHorse.HorseId))
                        horses.Add(mountedHorse.HorseId, mountedHorse);
                }
            }

            // Loop for Farmhands
            if (!Context.IsMainPlayer)
            {
                foreach (GameLocation location in SHelper.Multiplayer.GetActiveLocations())
                {
                    foreach (NPC npc in location.characters)
                    {
                        if (npc is Horse && IsNotATractor(npc as Horse))
                        {
                            Horse horse = npc as Horse;
                            if (!horses.ContainsKey(horse.HorseId))
                                horses.Add(horse.HorseId, horse);
                        }
                    }
                }
                return horses;
            }

            // Loop for Host
            foreach (NPC npc in Utility.getAllCharacters())
                if (npc is Horse && IsNotATractor(npc as Horse))
                {
                    Horse horse = npc as Horse;
                    if (!horses.ContainsKey(horse.HorseId))
                        horses.Add(horse.HorseId, horse);
                }

            return horses;
        }

        /// <summary> Gets Horse by id </summary>
        /// <param name="horseId">id of horse you wish to get</param>
        /// <returns>Horse object</returns>
        public static Horse GetHorseById(Guid horseId) { return horseIdMap[horseId]; }


        /// <summary> Gets all stables that are fully constructed and contain a horse (i.e. not a tractor) </summary>
        /// <returns>List of valid stables</returns>
        public static List<Stable> GetHorseStables()
        {
            List<Stable> stables = new List<Stable>();
            foreach (Building building in Game1.getFarm().buildings)
            {
                if (building is Stable)
                {
                    Stable stable = building as Stable;
                    Horse horse = Utility.findHorse(stable.HorseId);
                    if (horse != null && IsNotATractor(horse))
                        stables.Add(stable);
                }
            }
            return stables;
        }

        /// <summary> Checks if given horse is not a tractor </summary>
        /// <param name="horse">Horse object</param>
        /// <returns>true if not a tractor</returns>
        public static bool IsNotATractor(Horse horse) { return horse.Name == null ? true : !horse.Name.StartsWith("tractor/"); }

        public void GenerateHorseSkinMap(Horse horse)
        {
            if (!Context.IsMainPlayer)
                return;

            if (horse.Manners > 0)
            {
                if (horse.Manners <= config.AmountOfHorseSkins)
                {
                    if (File.Exists(Path.Combine(SHelper.DirectoryPath, $"assets/horse_{horse.Manners}.png")))
                    {
                        // horse.Sprite.spriteTexture = SHelper.Content.Load<Texture2D>($"assets/horse_{horse.Manners}.png");
                        UpdateHorseSkinMap(horse.HorseId, horse.Manners);
                    }
                }
            }
        }
        public static void ReLoadHorseSprites(Horse horse)
        {
            if (horseSkinMap.ContainsKey(horse.HorseId) && horseSkinMap[horse.HorseId] > 0)
            {
                int skinId = horseSkinMap[horse.HorseId];

                if (skinId <= config.AmountOfHorseSkins)
                {
                    if (File.Exists(Path.Combine(SHelper.DirectoryPath, $"assets/horse_{skinId}.png")))
                    {
                        horse.Sprite.spriteTexture = SHelper.ModContent.Load<Texture2D>($"assets/horse_{skinId}.png");
                    }
                }
            }
        }

        public static void SendMultiplayerReloadSkinMessage(Guid horseId, int skinId)
        {
            if (Context.IsMainPlayer)
            {
                SHelper.Multiplayer.SendMessage(
                    message: new Framework.HorseReskinMessage(horseId, skinId),
                    messageType: ReloadHorseSpritesMessageId,
                    modIDs: new[] { SModManifest.UniqueID }
                );
            }
        }

        public static void UpdateHorseSkinMap(Guid horseId, int skinId)
        {
            horseSkinMap[horseId] = skinId;
        }

        private static void LoadAllSprites()
        {
            for (var i = 1; i <= config.AmountOfHorseSkins; i++)
                skinTextureMap[i] = SHelper.ModContent.Load<Texture2D>($"assets/horse_{i}.png");
        }

        public static void SaveHorseReskin(Guid horseId, int skinId)
        {
            if (!Context.IsMainPlayer)
                return;

            if (skinId > config.AmountOfHorseSkins)
            {
                SMonitor.Log($"Tried to save skin {skinId}, but config file states there are only {config.AmountOfHorseSkins} skins in /assets", LogLevel.Warn);
                return;
            }

            var horse = GetHorseById(horseId);
            if (horse != null)
            {
                horse.Manners = skinId;
                SMonitor.Log($"Saving skin {skinId} to horse {horse.displayName}", LogLevel.Info);
                UpdateHorseSkinMap(horseId, skinId);
                ReLoadHorseSprites(horse);
                SendMultiplayerReloadSkinMessage(horseId, skinId);
            }
        }

        public static Guid? GetHorseIdFromName(string horseName)
        {
            foreach (var d in horseIdMap)
                if (d.Value.displayName == horseName)
                    return d.Key;

            SMonitor.Log($"No horse named {horseName} was found", LogLevel.Error);
            return null;
        }

        /// <summary>Get all horses in the given location.</summary>
        /// <param name="location">The location to scan.</param>
        private IEnumerable<Horse> GetHorsesIn(GameLocation location)
        {
            // single-player
            if (!Context.IsMultiplayer)
                return location.characters.OfType<Horse>().Where(h => IsNotATractor(h));

            // multiplayer
            return
                location.characters.OfType<Horse>().Where(h => IsNotATractor(h))
                    .Concat(
                        from player in location.farmers
                        where (player.mount != null && IsNotATractor(player.mount))
                        select player.mount
                    )
                    .Distinct();
        }
    }
}
