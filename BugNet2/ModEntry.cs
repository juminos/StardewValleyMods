using System;
using System.Collections.Generic;
using System.Linq;
using BugNet.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.Tools;

namespace BugNet
{
    /// <summary>The mod entry point.</summary>
    internal class Mod : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        private static readonly Dictionary<string, CritterData> CrittersData = new();
        private static readonly Dictionary<string, ObjectData> CritterCageData = new();

        /// <summary>The placeholder texture for custom critter cages.</summary>
        private TextureTarget PlaceholderSprite;


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
            Mod.Instance = this;
            SMonitor = this.Monitor;

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            var tilesheet = helper.ModContent.Load<Texture2D>("assets/critters.png");

            Rectangle GetTilesheetArea(int index)
            {
                return new Rectangle(index % 4 * 16, index / 4 * 16, 16, 16);
            }
            void Register(string name, int index, CritterBuilder critterBuilder)
            {
                this.RegisterCritter(
                    critterId: name,
                    texture: tilesheet,
                    textureArea: GetTilesheetArea(index),
                    translationKey: $"critter.{name}",
                    isThisCritter: critterBuilder.IsThisCritter,
                    makeCritter: critterBuilder.MakeCritter
                );
            }

            this.PlaceholderSprite = new TextureTarget(tilesheet, GetTilesheetArea(24)); // empty jar sprite

            Register("SummerButterflyBlue", 0, CritterBuilder.ForButterfly(128));
            Register("SummerButterflyGreen", 1, CritterBuilder.ForButterfly(148));
            Register("SummerButterflyRed", 2, CritterBuilder.ForButterfly(132));
            Register("SummerButterflyPink", 3, CritterBuilder.ForButterfly(152));
            Register("SummerButterflyYellow", 4, CritterBuilder.ForButterfly(136));
            Register("SummerButterflyOrange", 5, CritterBuilder.ForButterfly(156));
            Register("SpringButterflyPalePink", 6, CritterBuilder.ForButterfly(160));
            Register("SpringButterflyMagenta", 7, CritterBuilder.ForButterfly(180));
            Register("SpringButterflyWhite", 8, CritterBuilder.ForButterfly(163));
            Register("SpringButterflyYellow", 9, CritterBuilder.ForButterfly(183));
            Register("SpringButterflyPurple", 10, CritterBuilder.ForButterfly(166));
            Register("SpringButterflyPink", 11, CritterBuilder.ForButterfly(186));
            Register("BrownBird", 12, CritterBuilder.ForBird(Birdie.brownBird));
            Register("BlueBird", 13, CritterBuilder.ForBird(Birdie.blueBird));
            Register("GreenFrog", 14, CritterBuilder.ForFrog(olive: false));
            Register("OliveFrog", 15, CritterBuilder.ForFrog(olive: true));
            Register("Firefly", 16, CritterBuilder.ForFirefly());
            Register("Squirrel", 17, CritterBuilder.ForSquirrel());
            Register("GrayRabbit", 18, CritterBuilder.ForRabbit(white: false));
            Register("WhiteRabbit", 19, CritterBuilder.ForRabbit(white: true));
            Register("WoodPecker", 20, CritterBuilder.ForWoodpecker());
            Register("Seagull", 21, CritterBuilder.ForSeagull());
            Register("Owl", 22, CritterBuilder.ForOwl());
            Register("Crow", 23, CritterBuilder.ForCrow());
            Register("Cloud", 24, CritterBuilder.ForCloud());
            Register("BlueParrot", 25, CritterBuilder.ForParrot(green: false));
            Register("GreenParrot", 26, CritterBuilder.ForParrot(green: true));
            Register("Monkey", 27, CritterBuilder.ForMonkey());
            Register("OrangeIslandButterfly", 28, CritterBuilder.ForButterfly(364, island: true));
            Register("PinkIslandButterfly", 29, CritterBuilder.ForButterfly(368, island: true));
            Register("PurpleBird", 30, CritterBuilder.ForBirdAlt(390));
            Register("RedBird", 31, CritterBuilder.ForBirdAlt(430));
            Register("SunsetTropicalButterfly", 32, CritterBuilder.ForButterfly(372, island: true));
            Register("TropicalButterfly", 33, CritterBuilder.ForButterfly(376, island: true));
            Register("RedHeadBird", 34, CritterBuilder.ForBirdAlt(550));
            Register("DoveBird", 35, CritterBuilder.ForBirdAlt(570));
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return new BugNetApi(this.RegisterCritter, this.PlaceholderSprite, this.Monitor);
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Add a new critter which can be caught.</summary>
        /// <param name="critterId">The unique critter ID.</param>
        /// <param name="texture">The texture to show in the critter cage.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to show in the critter cage.</param>
        /// <param name="translationKey">The translation key for the critter name.</param>
        /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        private void RegisterCritter(string critterId, Texture2D texture, Rectangle textureArea, string translationKey, Func<int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter)
        {
            // get name translations
            this.GetTranslationsInAllLocales(
                translationKey,
                out string defaultCritterName,
                out Dictionary<string, string> critterNameTranslations
            );

            // register critter
            this.RegisterCritter(critterId, texture, textureArea, defaultCritterName, critterNameTranslations, makeCritter, isThisCritter);
        }

        /// <summary>Add a new critter which can be caught.</summary>
        /// <param name="critterId">The unique critter ID.</param>
        /// <param name="texture">The texture to show in the critter cage.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to show in the critter cage.</param>
        /// <param name="defaultCritterName">The default English critter name.</param>
        /// <param name="translatedCritterNames">The translated critter names in each available locale.</param>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
        private void RegisterCritter(string critterId, Texture2D texture, Rectangle textureArea, string defaultCritterName, Dictionary<string, string> translatedCritterNames, Func<int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter)
        {
            // get translations
            string TranslateCritterName(string locale)
            {
                return translatedCritterNames.GetValueOrDefault(locale) ?? defaultCritterName;
            }
            this.GetTranslationsInAllLocales("cage.name", out string defaultCageName, out var cageNameTranslations, format: (locale, translation) => translation.Tokens(new { critterName = TranslateCritterName(locale) }).ToString());
            this.GetTranslationsInAllLocales("cage.description", out string defaultCageDescription, out var cageDescriptionTranslations);

            // save critter data
            Mod.CrittersData.Add(critterId, new CritterData(
                defaultName: defaultCritterName,
                translatedName: () => TranslateCritterName(this.Helper.GameContent.CurrentLocale),
                texture: new TextureTarget(texture, textureArea),
                isThisCritter: isThisCritter,
                makeCritter: makeCritter
            ));

            // Clone texture area
            Texture2D critterTexture = this.CloneTextureArea(texture, textureArea);
            SMonitor.Log($"Critter texture cloned. Size: {critterTexture.Width}x{critterTexture.Height}", LogLevel.Trace);

            // Save the texture as a temporary asset
            string assetName = $"CritterCage_{critterId}";
            this.SaveTextureAsTemporaryAsset(assetName, critterTexture);
            SMonitor.Log($"Critter texture saved as temporary asset with name: {assetName}", LogLevel.Trace);

            // Get the current locale
            string currentLocale = this.Helper.GameContent.CurrentLocale;

            // Get the localized names and descriptions for the current locale
            string localizedCageName = cageNameTranslations.GetValueOrDefault(currentLocale) ?? defaultCageName;
            string localizedCageDescription = cageDescriptionTranslations.GetValueOrDefault(currentLocale) ?? defaultCageDescription;

            // save cage data
            var cageData = new ObjectData
            {
                Name = defaultCageName,
                DisplayName = localizedCageName,
                Description = localizedCageDescription,
                Type = "Basic",
                Category = StardewValley.Object.monsterLootCategory,
                Price = critterId.Contains("Butterfly") ? 50 : 100,
                Texture = $"Mods/{this.ModManifest.UniqueID}/assets/{assetName}.png",
                ContextTags = new List<string>(new[] { "critter" }),
                ExcludeFromShippingCollection = true
            };
            SMonitor.Log($"Texture path set to: {$"Mods/{this.ModManifest.UniqueID}/assets/{assetName}.png"}", LogLevel.Trace);

            Mod.CritterCageData.Add($"CritterCage_{critterId}", cageData);
        }
        private void OnUpdateTicked(object sender, EventArgs e)
        {
            if (Game1.player.UsingTool && Game1.player.CurrentTool is MeleeWeapon weapon && weapon.Name == "Bug Net")
            {
                SMonitor.Log($"Player using bug net.", LogLevel.Debug);
                Vector2 toolLoc = Game1.player.GetToolLocation(true);
                Vector2 a = Vector2.Zero, b = Vector2.Zero;
                Rectangle area = weapon.getAreaOfEffect((int)toolLoc.X, (int)toolLoc.Y, Game1.player.FacingDirection, ref a, ref b, Game1.player.GetBoundingBox(), Game1.player.FarmerSprite.currentAnimationIndex);
                SMonitor.Log($"Calculated area of effect: {area}", LogLevel.Trace);

                var critters = Game1.player.currentLocation.critters;
                if (critters == null)
                {
                    SMonitor.Log($"No critters found in the current location", LogLevel.Warn);
                    return;
                }

                foreach (Critter critter in critters.ToList())
                {
                    Rectangle critterBoundingBox = critter.getBoundingBox(0, 0);
                    SMonitor.Log($"Critter bounding box: {critterBoundingBox}", LogLevel.Trace);

                    if (critterBoundingBox.Intersects(area))
                    {
                        SMonitor.Log($"Critter found within area of effect", LogLevel.Debug);
                        if (!Mod.TryGetCritter(critter, out CritterData data, out string critterId))
                        {
                            SMonitor.Log($"Critter not supported by BugNet mod", LogLevel.Debug);
                            continue; // not a supported critter
                        }

                        critters.Remove(critter);
                        string objId = $"CritterCage_{critterId}";
                        if (Mod.CritterCageData.TryGetValue(objId, out ObjectData cageData))
                        {
                            SMonitor.Log($"Spawning a '{data.DefaultName}' critter cage with item ID {objId}", LogLevel.Info);
                            Game1.player.currentLocation.debris.Add(new Debris(new StardewValley.Object(objId, 1), critter.position));
                        }
                        else
                        {
                            SMonitor.Log($"Failed to find critter cage data for ID: {objId}", LogLevel.Error);
                        }
                    }
                    else
                    {
                        SMonitor.Log($"Critter bounding box does not intersect with the area of effect", LogLevel.Trace);
                    }
                }
            }
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton() && Game1.player.ActiveObject?.Name.StartsWith("Critter Cage: ") is true)
            {
                SMonitor.Log($"Active object name: {Game1.player.ActiveObject.Name}");
                // Get the critter ID
                CritterData activeCritter = null;
                foreach (var critterData in Mod.CrittersData)
                {
                    string check = $"Critter Cage: {critterData.Value.DefaultName}";
                    SMonitor.Log($"Checking : {critterData.Value.DefaultName}");
                    if (check == Game1.player.ActiveObject.Name)
                    {
                        activeCritter = critterData.Value;
                        break;
                    }
                }

                // Spawn the critter
                int x = (int)e.Cursor.GrabTile.X + 1, y = (int)e.Cursor.GrabTile.Y + 1;
                var critter = activeCritter.MakeCritter(x, y);
                Game1.player.currentLocation.addCritter(critter);

                Game1.player.reduceActiveItemByOne();

                this.Helper.Input.Suppress(e.Button);
            }
        }
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"Data/Objects"))
            {
                foreach (var critterCage in CritterCageData)
                    e.Edit(
                        asset =>
                        {
                            var data = asset.AsDictionary<string, ObjectData>().Data;
                            var objectData = new ObjectData
                            {
                                Name = critterCage.Value.Name,
                                DisplayName = critterCage.Value.DisplayName,
                                Description = critterCage.Value.Description,
                                Type = critterCage.Value.Type,
                                Category = critterCage.Value.Category,
                                Price = critterCage.Value.Price,
                                Texture = critterCage.Value.Texture,
                                ContextTags = critterCage.Value.ContextTags,
                                ExcludeFromShippingCollection = critterCage.Value.ExcludeFromShippingCollection,
                            };
                            data.Add(critterCage.Key, objectData);
                        });
                return;
            }
        }

        /// <summary>Get the data for a given critter, if it's supported by BugNet.</summary>
        /// <param name="critter">The critter to match.</param>
        /// <param name="data">The critter data.</param>
        /// <returns>Returns whether the critter data was found.</returns>
        internal static bool TryGetCritter(Critter critter, out CritterData data, out string critterId)
        {
            foreach (var pair in Mod.CrittersData)
            {
                if (pair.Value.IsThisCritter(critter))
                {
                    critterId = pair.Key;
                    data = pair.Value;
                    return true;
                }
            }
            critterId = null;
            data = null;
            return false;
        }

        /// <summary>Get the translations in all available locales for a given translation key.</summary>
        /// <param name="key">The translation key.</param>
        /// <param name="defaultText">The default text.</param>
        /// <param name="translations">The translation text in each locale.</param>
        /// <param name="format">Format a translation.</param>
        private void GetTranslationsInAllLocales(string key, out string defaultText, out Dictionary<string, string> translations, Func<string, Translation, string> format = null)
        {
            translations = this.Helper.Translation
                .GetInAllLocales(key, withFallback: true)
                .ToDictionary(
                    localeSet => localeSet.Key,
                    localeSet => format?.Invoke(localeSet.Key, localeSet.Value) ?? localeSet.Value.ToString()
                );

            defaultText = translations.GetValueOrDefault("default");
            translations.Remove("default");
        }

        /// <summary>Copy an area in a texture into a new texture.</summary>
        /// <param name="texture">The texture to copy.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to copy.</param>
        private Texture2D CloneTextureArea(Texture2D texture, Rectangle textureArea)
        {
            // 256 is kinda borderline for array rental.
            Color[] data = GC.AllocateUninitializedArray<Color>(textureArea.Width * textureArea.Height);
            texture.GetData(0, textureArea, data, 0, data.Length);
            Texture2D newTexture = new Texture2D(Game1.graphics.GraphicsDevice, textureArea.Width, textureArea.Height);
            newTexture.SetData(data);

            return newTexture;
        }
        private void SaveTextureAsTemporaryAsset(string assetName, Texture2D texture)
        {
            string path = Path.Combine(this.Helper.DirectoryPath, "assets", assetName + ".png");
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    texture.SaveAsPng(stream, texture.Width, texture.Height);
                }

                this.Helper.ModContent.Load<Texture2D>($"assets/{assetName}.png");
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Failed to save texture as temporary asset: {ex}", LogLevel.Error);
            }
        }
    }
}
