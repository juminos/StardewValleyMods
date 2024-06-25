using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using StardewModdingAPI.Events;
using StardewValley.GameData;
using StardewValley.BellsAndWhistles;
using Microsoft.Xna.Framework.Graphics;
using xTile.Tiles;
using StardewValley.Objects;


namespace ButterflyCompanion
{
    internal class ModEntry : Mod
    {
        public static Mod Instance;
        public static IMonitor SMonitor;

        public override void Entry(IModHelper helper)
        {
            ModEntry.Instance = this;
            SMonitor = this.Monitor;

            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"Data/Trinkets"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, TrinketData>().Data;
                    var butterflyData = new TrinketData
                    {
                        ID = $"{this.ModManifest.UniqueID}_ButterflyCompanion",
                        DisplayName = "Lucky Butterfly",
                        Description = "A magic butterfly companion.",
                        Texture = Helper.GameContent.Load<Texture2D>("TileSheets\\Objects_2").Name,
                        SheetIndex = 75,
                        TrinketEffectClass = "ButterflyCompanion.ButterflyTrinketEffect, ButterflyCompanion",
                        DropsNaturally = true,
                        CanBeReforged = true,
                        TrinketMetadata = null
                    };
                    data.Add($"{this.ModManifest.UniqueID}_ButterflyCompanion", butterflyData);

                    // Log trinket data addition
                    SMonitor.Log($"Butterfly trinket data added: ID={butterflyData.ID}, Class={butterflyData.TrinketEffectClass}", LogLevel.Debug);

                    // Check trinket effect class instantiation
                    if (butterflyData.TrinketEffectClass != null)
                    {
                        Type type = System.Type.GetType(butterflyData.TrinketEffectClass);
                        if (type != null)
                        {
                            SMonitor.Log($"Trinket effect {butterflyData.TrinketEffectClass} instantiated with assembly qualified name: {type.AssemblyQualifiedName}.", LogLevel.Debug);
                        }
                        else
                        {
                            SMonitor.Log($"Failed to find type for trinket effect {butterflyData.TrinketEffectClass}.", LogLevel.Error);
                        }
                    }
                });
            }
        }
    }
}
