using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using xTile.Tiles;

namespace Agrivoltaics
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;

        internal static ModEntry Mod { get; private set; }
        internal static ModConfig Config { get; private set; }
        internal string SolarPanelAssetPath { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Mod = this;
            Config = Helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.GameLoop.DayEnding += OnDayEnding;
            Helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            Helper.Events.Content.AssetRequested += OnAssetRequested;

            try
            {
                HarmonyPatches.PatchAll(this);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            SHelper.ModContent.Load<Texture2D>("assets/SolarPanel.png");
            SolarPanelAssetPath = SHelper.ModContent.GetInternalAssetName("assets/SolarPanel.png").BaseName;
        }
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            SolarPanel.ConfigOptions(e);
        }
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            foreach (GameLocation location in Game1.locations)
            {
                if (!location.HasMinBuildings("juminos.Agrivoltaics_SolarPanel", 1))
                {
                    //SMonitor.Log($"skipping location: {location.Name}", LogLevel.Debug);
                    continue;
                }
                var shadeTiles = new List<Vector2>();
                var newGrass = new Dictionary<Vector2, Grass>();

                foreach (Building building in location.buildings)
                {
                    if (building.buildingType.Value != "juminos.Agrivoltaics_SolarPanel")
                    {
                        //SMonitor.Log($"skipping building type: {building.buildingType.Value}", LogLevel.Debug);
                        continue;
                    }
                    //define shaded area
                    var footprint = building.GetBoundingBox();
                    //var shadeRect = new Microsoft.Xna.Framework.Rectangle(footprint.X - (footprint.Width / 3), footprint.Y - (footprint.Height * 3), footprint.Width * 5 / 3, footprint.Height * 4);
                    var startingTileX = footprint.X / 64 - 1;
                    var startingTileY = footprint.Y / 64 - 4;
                    for (int i = 0; i < 5; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            var tile = new Vector2(startingTileX + i, startingTileY + j);
                            if (!shadeTiles.Contains(tile) && location.isTilePassable(tile))
                            {
                                shadeTiles.Add(tile);
                            }
                        }
                    }
                }

                //spread existing grass
                if (Config.SpreadGrass)
                {
                    foreach (TerrainFeature feature in location.terrainFeatures.Values)
                    {
                        if (!(feature is Grass grass) || !(Game1.random.NextDouble() < Config.SpreadRate))
                            continue;
                        var grassTile = new Vector2((int)grass.Tile.X, (int)grass.Tile.Y);
                        if (!shadeTiles.Contains(grassTile))
                            continue;
                        //add more clumps to existing grass
                        if (grass.numberOfWeeds.Value < 4)
                        {
                            grass.numberOfWeeds.Value = Math.Max(0, Math.Min(4, grass.numberOfWeeds.Value + Game1.random.Next(3)));
                        }
                        else
                        {
                            if (grass.numberOfWeeds.Value < 4)
                                continue;
                            //expand to adjacent tiles
                            Vector2[] adjacentTileLocationsArray = Utility.getAdjacentTileLocationsArray(grass.Tile);
                            for (int k = 0; k < adjacentTileLocationsArray.Length; k++)
                            {
                                Vector2 tile = adjacentTileLocationsArray[k];
                                if (location.isTileOnMap((int)tile.X, (int)tile.Y) && 
                                    !location.IsTileBlockedBy(tile) && 
                                    location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null && 
                                    !location.IsNoSpawnTile(tile) && 
                                    Game1.random.NextDouble() < 0.25 &&
                                    !newGrass.ContainsKey(tile))
                                {
                                    newGrass.Add(tile, new Grass(grass.grassType.Value, Game1.random.Next(1, 3)));
                                }
                            }
                        }
                    }
                }

                //grow new grass under panels
                if (Config.GrowGrass)
                {
                    foreach(Vector2 tile in shadeTiles)
                    {
                        location.objects.TryGetValue(tile, out var o);
                        if (o == null &&
                            location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null &&
                            !location.IsNoSpawnTile(tile) &&
                            location.isTileLocationOpen(new Location((int)tile.X, (int)tile.Y)) &&
                            !location.IsTileOccupiedBy(tile) &&
                            !location.isWaterTile((int)tile.X, (int)tile.Y) &&
                            location.GetSeason() != Season.Winter &&
                            Game1.random.NextDouble() < Config.GrowRate &&
                            !newGrass.ContainsKey(tile))
                        {
                            newGrass.Add(tile, new Grass((Game1.random.NextDouble() < 0.5) ? 1 : 7, Game1.random.Next(1, 3)));
                        }
                    }
                }
                foreach (KeyValuePair<Vector2, Grass> pair in newGrass)
                {
                    location.terrainFeatures.Add(pair.Key, pair.Value);
                }
            }
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            foreach (GameLocation location in Game1.locations)
                {
                if (!location.HasMinBuildings("juminos.Agrivoltaics_SolarPanel", 1))
                {
                    //SMonitor.Log($"skipping location: {location.Name}", LogLevel.Debug);
                    continue;
                }
                if ((Config.SpringBatteryCount <= 0 && location.GetSeason() == Season.Spring) ||
                (Config.SummerBatteryCount <= 0 && location.GetSeason() == Season.Summer) ||
                (Config.FallBatteryCount <= 0 && location.GetSeason() == Season.Fall) ||
                (Config.WinterBatteryCount <= 0 && location.GetSeason() == Season.Winter))
                {
                    continue;
                }
                foreach (Building building in location.buildings)
                {
                    if (building.buildingType.Value != "juminos.Agrivoltaics_SolarPanel")
                    {
                        //SMonitor.Log($"skipping building type: {building.buildingType.Value}", LogLevel.Debug);
                        continue;
                    }

                    //add batteries to building output
                    if (!location.IsRainingHere() && !location.IsSnowingHere() && !location.IsLightningHere() && !location.IsGreenRainingHere())
                    {
                        if (!building.modData.ContainsKey("batteryCounter"))
                        {
                            var batteryCounter = GetBatteryCounterBySeason(location);
                            building.modData.Add("batteryCounter", batteryCounter.ToString());
                        }
                        else
                        {
                            var batteryCounter = Convert.ToInt32(building.modData["batteryCounter"]);
                            if (batteryCounter <= 1)
                            {
                                var battery = ItemRegistry.Create<StardewValley.Object>("(O)787");
                                int count;
                                switch (location.GetSeasonIndex())
                                {
                                    case 0:
                                        count = Config.SpringBatteryCount;
                                        break;
                                    case 1:
                                        count = Config.SummerBatteryCount;
                                        break;
                                    case 2:
                                        count = Config.FallBatteryCount;
                                        break;
                                    case 3:
                                        count = Config.WinterBatteryCount;
                                        break;
                                    default:
                                        count = 1;
                                        break;
                                }

                                battery.Stack = count;
                                var chest = building.GetBuildingChest("Output");
                                chest.addItem(battery);

                                var resetCounter = GetBatteryCounterBySeason(location);
                                building.modData["batteryCounter"] = resetCounter.ToString();
                            }
                            else
                            {
                                batteryCounter--;
                                building.modData["batteryCounter"] = batteryCounter.ToString();
                            }
                        }
                    }
                }
            }
        }

        public static int GetBatteryCounterBySeason(GameLocation location)
        {
            var seasonIndex = location.GetSeasonIndex();
            switch (seasonIndex)
            {
                case 0:
                    return Config.SpringBatteryCounter;
                case 1:
                    return Config.SummerBatteryCounter;
                case 2:
                    return Config.FallBatteryCounter;
                case 3:
                    return Config.WinterBatteryCounter;
                default: return 1;
            }
        }

        private void OnGameLaunched (object? sender, GameLaunchedEventArgs e)
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

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.SolarPanelCost_Title,
                tooltip: I18n.SolarPanelCost_Description,
                getValue: () => Config.SolarPanelCost,
                setValue: value => Config.SolarPanelCost = (int)value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.RefinedQuartzAmount_Title,
                tooltip: I18n.RefinedQuartzAmount_Description,
                getValue: () => Config.RefinedQuartzAmount,
                setValue: value => Config.RefinedQuartzAmount = (int)value
                );


            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.IronBarAmount_Title,
                tooltip: I18n.IronBarAmount_Description,
                getValue: () => Config.IronBarAmount,
                setValue: value => Config.IronBarAmount = (int)value
                );


            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.GoldBarAmount_Title,
                tooltip: I18n.GoldBarAmount_Description,
                getValue: () => Config.GoldBarAmount,
                setValue: value => Config.GoldBarAmount = (int)value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.ConditionalBuild_Title,
                tooltip: I18n.ConditionalBuild_Description,
                getValue: () => Config.ConditionalBuild,
                setValue: value => Config.ConditionalBuild = value
                );

            configMenu.AddTextOption(
                mod: this.ModManifest,
                name: I18n.BuildCondition_Title,
                tooltip: I18n.BuildCondition_Description,
                getValue: () => Config.BuildCondition,
                setValue: value => Config.BuildCondition = value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.SpringBatteryCount_Title,
                tooltip: I18n.SpringBatteryCount_Description,
                getValue: () => Config.SpringBatteryCount,
                setValue: value => Config.SpringBatteryCount = (int)value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.SummerBatteryCount_Title,
                tooltip: I18n.SummerBatteryCount_Description,
                getValue: () => Config.SummerBatteryCount,
                setValue: value => Config.SummerBatteryCount = (int)value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.FallBatteryCount_Title,
                tooltip: I18n.FallBatteryCount_Description,
                getValue: () => Config.FallBatteryCount,
                setValue: value => Config.FallBatteryCount = (int)value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.WinterBatteryCount_Title,
                tooltip: I18n.WinterBatteryCount_Description,
                getValue: () => Config.WinterBatteryCount,
                setValue: value => Config.WinterBatteryCount = (int)value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.SpringBatteryCounter_Title,
                tooltip: I18n.SpringBatteryCounter_Description,
                getValue: () => Config.SpringBatteryCounter,
                setValue: value => Config.SpringBatteryCounter = (int)value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.SummerBatteryCounter_Title,
                tooltip: I18n.SummerBatteryCounter_Description,
                getValue: () => Config.SummerBatteryCounter,
                setValue: value => Config.SummerBatteryCounter = (int)value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.FallBatteryCounter_Title,
                tooltip: I18n.FallBatteryCounter_Description,
                getValue: () => Config.FallBatteryCounter,
                setValue: value => Config.FallBatteryCounter = (int)value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.WinterBatteryCounter_Title,
                tooltip: I18n.WinterBatteryCounter_Description,
                getValue: () => Config.WinterBatteryCounter,
                setValue: value => Config.WinterBatteryCounter = (int)value
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.GrowGrass_Title,
                tooltip: I18n.GrowGrass_Description,
                getValue: () => Config.GrowGrass,
                setValue: value => Config.GrowGrass = value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.GrassRate_Title,
                tooltip: I18n.GrassRate_Description,
                getValue: () => Config.GrowRate,
                setValue: value => Config.GrowRate = value,
                min: 0.0f,
                max: 1.0f,
                interval: 0.01f
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.SpreadGrass_Title,
                tooltip: I18n.SpreadGrass_Description,
                getValue: () => Config.SpreadGrass,
                setValue: value => Config.SpreadGrass = value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.SpreadRate_Title,
                tooltip: I18n.SpreadRate_Description,
                getValue: () => Config.SpreadRate,
                setValue: value => Config.SpreadRate = value,
                min: 0.0f,
                max: 1.0f,
                interval: 0.01f
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.RetentionIncrease_Title,
                tooltip: I18n.RetentionIncrease_Description,
                getValue: () => Config.RetentionIncrease,
                setValue: value => Config.RetentionIncrease = value,
                min: 0.0f,
                max: 1.0f,
                interval: 0.01f
                );

            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: I18n.SeasonReset_Title,
                tooltip: I18n.SeasonReset_Description,
                getValue: () => Config.SeasonReset,
                setValue: value => Config.SeasonReset = value
                );
        }
    }
}
