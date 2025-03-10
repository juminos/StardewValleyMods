﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using CustomCritters2.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;

namespace CustomCritters2
{
    /// <summary>The mod entry point.</summary>
    internal partial class ModEntry : StardewModdingAPI.Mod
    {
        /*********
        ** Accessors
        *********/
        public static Mod Instance;
        public static IMonitor SMonitor;

        /*********
        ** Public methods
        *********/
        /// <inheritdoc />
        public override void Entry(IModHelper helper)
        {
            ModEntry.Instance = this;
            SMonitor = this.Monitor;

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Player.Warped += this.OnWarped;

            // load content packs
            SMonitor.Log("Loading critter content packs...", LogLevel.Info);
            foreach (IContentPack contentPack in this.GetContentPacks())
            {
                CritterEntry data = contentPack.ReadJsonFile<CritterEntry>("critter.json");
                if (data == null)
                {
                    SMonitor.Log($"   {contentPack.Manifest.Name}: ignored (no critter.json file).", LogLevel.Warn);
                    continue;
                }
                if (!File.Exists(Path.Combine(contentPack.DirectoryPath, "critter.png")))
                {
                    SMonitor.Log($"   {contentPack.Manifest.Name}: ignored (no critter.png file).", LogLevel.Warn);
                    continue;
                }
                SMonitor.Log(contentPack.Manifest.Name == data.Id ? contentPack.Manifest.Name : $"   {contentPack.Manifest.Name} (id: {data.Id})", LogLevel.Info);
                CritterEntry.Register(data);
            }
        }


        /*********
        ** Private methods
        *********/
        /// <inheritdoc cref="IGameLoopEvents.GameLaunched"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            // register critters with BugNet
            var bugNet = this.Helper.ModRegistry.GetApi<IBugNetApi>("juminos.BugNet2");
            if (bugNet is not null)
            {
                foreach (CritterEntry critter in CritterEntry.Critters.Values)
                {
                    Texture2D texture = CustomCritter.LoadCritterTexture(critter.Id);

                    for (int variation = 0; variation < critter.SpriteData.Variations; variation++)
                    {
                        int index = variation * (texture.Width / critter.SpriteData.FrameWidth);
                        string? textureName = Helper.ModContent.GetInternalAssetName($"Critters/{critter.Id}/critter.png").ToString()?.Replace("/", "\\");
                        string critterId = $"{this.ModManifest.UniqueID}/{critter.Id.Replace(" ", string.Empty)}/Variation{variation + 1}";
                        string variationName = critter.VariationNames.TryGetValue(variation, out var name)
                            ? name
                            : $"{critter.Name} (Variation {variation + 1})";

                        bugNet.RegisterCritter(
                            manifest: this.ModManifest,
                            critterId: critterId,
                            textureName: textureName,
                            variation: variation,
                            index: index,
                            defaultCritterName: variationName,
                            translatedCritterNames: new Dictionary<string, string>(),
                            makeCritter: (x, y, variation) => critter.MakeCritter(new Vector2(x, y), variation),
                            isThisCritter: instance => (instance as CustomCritter)?.Data.Id == critter.Id
                            );

                        SMonitor.Log($"Registering {critterId} with BugNet2 using textureName: {textureName} and variation {variation}");
                    }
                }
            }
        }

        /// <inheritdoc cref="IPlayerEvents.Warped"/>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnWarped(object? sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer || Game1.CurrentEvent != null)
                return;

            foreach (var entry in CritterEntry.Critters)
            {
                for (int i = 0; i < entry.Value.SpawnAttempts; ++i)
                {
                    if (entry.Value.Check(e.NewLocation))
                    {
                        var spot = entry.Value.PickSpot(e.NewLocation);
                        if (spot == null)
                            continue;

                        e.NewLocation.addCritter(entry.Value.MakeCritter(spot.Value, Game1.random.Next(entry.Value.SpriteData.Variations)));
                    }
                }
            }
        }

        /// <summary>Load available content packs.</summary>
        private IEnumerable<IContentPack> GetContentPacks()
        {
            // SMAPI content packs
            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
                yield return contentPack;

            // legacy content packs
            string legacyRoot = Path.Combine(this.Helper.DirectoryPath, "Critters");
            Directory.CreateDirectory(legacyRoot);
            foreach (string folderPath in Directory.EnumerateDirectories(legacyRoot))
            {
                yield return this.Helper.ContentPacks.CreateTemporary(
                    directoryPath: folderPath,
                    id: Guid.NewGuid().ToString("N"),
                    name: new DirectoryInfo(folderPath).Name,
                    description: null,
                    author: null,
                    version: new SemanticVersion(1, 0, 0)
                );
            }
        }
    }
}