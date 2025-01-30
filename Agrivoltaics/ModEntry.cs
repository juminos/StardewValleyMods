using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
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

            try
            {
                HarmonyPatches.PatchAll(this);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            foreach (GameLocation location in Game1.locations)
            {
                if (!location.HasMinBuildings("juminos.Agrivoltaics.CP_SolarPanel", 1))
                {
                    //SMonitor.Log($"skipping location: {location.Name}", LogLevel.Debug);
                    continue;
                }
                foreach (Building building in location.buildings)
                {
                    if (building.buildingType.Value != "juminos.Agrivoltaics.CP_SolarPanel")
                    {
                        //SMonitor.Log($"skipping building type: {building.buildingType.Value}", LogLevel.Debug);
                        continue;
                    }

                    //define shaded area
                    var footprint = building.GetBoundingBox();
                    var shadeRect = new Microsoft.Xna.Framework.Rectangle(footprint.X - (footprint.Width / 3), footprint.Y - (footprint.Height * 3), footprint.Width * 5 / 3, footprint.Height * 4);
                    var startingTileX = footprint.X / 64 - 1;
                    var startingTileY = footprint.Y / 64 - 3;

                    //spread existing grass
                    if (Config.SpreadGrass)
                    {
                        var newGrass = new Dictionary<Vector2, Grass>();
                        foreach (TerrainFeature feature in location.terrainFeatures.Values)
                        {
                            if (!(feature is Grass grass) || !(Game1.random.NextDouble() < Config.SpreadRate) || !(feature.getBoundingBox().Intersects(shadeRect)))
                                continue;
                            if (grass.numberOfWeeds.Value < 4)
                            {
                                grass.numberOfWeeds.Value = Math.Max(0, Math.Min(4, grass.numberOfWeeds.Value + Game1.random.Next(3)));
                            }
                            else
                            {
                                if (grass.numberOfWeeds.Value < 4)
                                    continue;
                                Vector2[] adjacentTileLocationsArray = Utility.getAdjacentTileLocationsArray(grass.Tile);
                                for (int k = 0; k < adjacentTileLocationsArray.Length; k++)
                                {
                                    Vector2 tile = adjacentTileLocationsArray[k];
                                    if (location.isTileOnMap((int)grass.Tile.X, (int)grass.Tile.Y) && 
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
                        foreach (KeyValuePair<Vector2, Grass> pair in newGrass)
                        {
                            location.terrainFeatures.Add(pair.Key, pair.Value);
                        }
                    }

                    //add new grass under panels
                    if (Config.GrowGrass)
                    {
                        for (int i = startingTileX; i < startingTileX + 5; i++)
                        {
                            for (int j = startingTileY; j < startingTileY + 4; j++)
                            {
                                var grassTile = new Vector2(i, j);

                                location.objects.TryGetValue(grassTile, out var o);
                                if (o == null && location.doesTileHaveProperty(i, j, "Diggable", "Back") != null && !location.IsNoSpawnTile(grassTile) && location.isTileLocationOpen(new Location(i, j)) && !location.IsTileOccupiedBy(grassTile) && !location.isWaterTile(i, j) && location.GetSeason() != Season.Winter && Game1.random.NextDouble() < Config.GrowRate)
                                {
                                    location.terrainFeatures.Add(grassTile, new Grass((Game1.random.NextDouble() < 0.5) ? 1 : 7, Game1.random.Next(1, 3)));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            if (Config.MinBatteryCount > 0)
            {
                foreach (GameLocation location in Game1.locations)
                {
                    if (!location.HasMinBuildings("juminos.Agrivoltaics.CP_SolarPanel", 1))
                    {
                        //SMonitor.Log($"skipping location: {location.Name}", LogLevel.Debug);
                        continue;
                    }
                    foreach (Building building in location.buildings)
                    {
                        if (building.buildingType.Value != "juminos.Agrivoltaics.CP_SolarPanel")
                        {
                            //SMonitor.Log($"skipping building type: {building.buildingType.Value}", LogLevel.Debug);
                            continue;
                        }

                        //add batteries to building output
                        if (!location.IsRainingHere() && !location.IsSnowingHere() && !location.IsLightningHere() && !location.IsGreenRainingHere())
                        {
                            var battery = ItemRegistry.Create<StardewValley.Object>("(O)787");
                            int min = Config.MinBatteryCount;
                            int max = Config.MaxBatteryCount + 1;
                            if (Config.MinBatteryCount > Config.MaxBatteryCount)
                                min = Config.MaxBatteryCount;
                            battery.Stack = Game1.random.Next(min, max);

                            var chest = building.GetBuildingChest("Output");
                            chest.addItem(battery);
                        }
                    }
                }
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
                name: I18n.MaxBatteryCount_Title,
                tooltip: I18n.MaxBatteryCount_Description,
                getValue: () => Config.MaxBatteryCount,
                setValue: value => Config.MaxBatteryCount = (int)value
                );

            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: I18n.MinBatteryCount_Title,
                tooltip: I18n.MinBatteryCount_Description,
                getValue: () => Config.MinBatteryCount,
                setValue: value => Config.MinBatteryCount = (int)value
                );
            //configMenu.AddNumberOption(
            //    mod: this.ModManifest,
            //    name: I18n.BatteryRate_Title,
            //    tooltip: I18n.BatteryRate_Description,
            //    getValue: () => Config.BatteryRate,
            //    setValue: value => Config.BatteryRate = (int)value
            //    );

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
        }
    }
}
