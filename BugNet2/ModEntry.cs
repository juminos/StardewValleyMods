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

        /// <summary>The placeholder texture for custom critters.</summary>
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

            var tilesheet = helper.GameContent.Load<Texture2D>("TileSheets\\critters");

            Rectangle GetTilesheetArea(int index)
            {
                return new Rectangle(index % 20 * 16, index / 20 * 16, 16, 16);
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

            this.PlaceholderSprite = new TextureTarget(tilesheet, GetTilesheetArea(500)); // empty sprite

            Register("SummerButterflyBlue", 128, CritterBuilder.ForButterfly(128));
            Register("SummerButterflyGreen", 148, CritterBuilder.ForButterfly(148));
            Register("SummerButterflyRed", 132, CritterBuilder.ForButterfly(132));
            Register("SummerButterflyPink", 152, CritterBuilder.ForButterfly(152));
            Register("SummerButterflyYellow", 136, CritterBuilder.ForButterfly(136));
            Register("SummerButterflyOrange", 156, CritterBuilder.ForButterfly(156));
            Register("SpringButterflyPalePink", 160, CritterBuilder.ForButterfly(160));
            Register("SpringButterflyMagenta", 180, CritterBuilder.ForButterfly(180));
            Register("SpringButterflyWhite", 163, CritterBuilder.ForButterfly(163));
            Register("SpringButterflyYellow", 183, CritterBuilder.ForButterfly(183));
            Register("SpringButterflyPurple", 166, CritterBuilder.ForButterfly(166));
            Register("SpringButterflyPink", 186, CritterBuilder.ForButterfly(186));
            Register("BrownBird", 110, CritterBuilder.ForBird(Birdie.brownBird));
            Register("BlueBird", 190, CritterBuilder.ForBird(Birdie.blueBird));
            Register("GreenFrog", 280, CritterBuilder.ForFrog(olive: false));
            Register("OliveFrog", 300, CritterBuilder.ForFrog(olive: true));
            Register("Firefly", 10, CritterBuilder.ForFirefly());
            Register("Squirrel", 260, CritterBuilder.ForSquirrel());
            Register("GrayRabbit", 276, CritterBuilder.ForRabbit(white: false));
            Register("WhiteRabbit", 278, CritterBuilder.ForRabbit(white: true));
            Register("WoodPecker", 320, CritterBuilder.ForWoodpecker());
            Register("Seagull", 0, CritterBuilder.ForSeagull());
            Register("Owl", 326, CritterBuilder.ForOwl());
            Register("Crow", 48, CritterBuilder.ForCrow());
            Register("Cloud", 110, CritterBuilder.ForCloud());
            Register("BlueParrot", 110, CritterBuilder.ForParrot(green: false));
            Register("GreenParrot", 110, CritterBuilder.ForParrot(green: true));
            Register("Monkey", 420, CritterBuilder.ForMonkey());
            Register("OrangeIslandButterfly", 364, CritterBuilder.ForButterfly(364, island: true));
            Register("PinkIslandButterfly", 368, CritterBuilder.ForButterfly(368, island: true));
            Register("PurpleBird", 510, CritterBuilder.ForBird(390));
            Register("RedBird", 550, CritterBuilder.ForBird(430));
            Register("SunsetTropicalButterfly", 372, CritterBuilder.ForButterfly(372, island: true));
            Register("TropicalButterfly", 376, CritterBuilder.ForButterfly(376, island: true));
            // Register("Opossum",600, CritterBuilder.ForOpossum()); // Not working
            Register("RedHeadBird", 670, CritterBuilder.ForBird(670));
            Register("DoveBird", 710, CritterBuilder.ForBird(690));
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
        /// <param name="texture">The texture to show in the inventory.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to show in the inventory.</param>
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
        /// <param name="texture">The texture to show in the inventory.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to show in the inventory.</param>
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

            // save critter data
            Mod.CrittersData.Add(critterId, new CritterData(
                defaultName: defaultCritterName,
                translatedName: () => TranslateCritterName(this.Helper.GameContent.CurrentLocale),
                texture: new TextureTarget(texture, textureArea),
                isThisCritter: isThisCritter,
                makeCritter: makeCritter
            ));

            // Get the current locale
            string currentLocale = this.Helper.GameContent.CurrentLocale;
        }
        private void OnUpdateTicked(object? sender, EventArgs e)
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
                        else
                        {
                            critters.Remove(critter);
                            SMonitor.Log($"Spawning a '{data.DefaultName}'", LogLevel.Info);
                            Game1.player.currentLocation.debris.Add(new Debris(new StardewValley.Object(critterId, 1), critter.position));
                        }
                    }
                    else
                    {
                        SMonitor.Log($"Critter bounding box does not intersect with the area of effect", LogLevel.Trace);
                    }
                }
            }
        }
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (e.Button.IsActionButton() && Game1.player.ActiveObject?.getDescription().Contains("wild critter") is true)
            {
                SMonitor.Log($"Active object name: {Game1.player.ActiveObject.Name}");
                // Get the critter ID
                CritterData activeCritter = null;
                foreach (var critterData in Mod.CrittersData)
                {
                    SMonitor.Log($"Checking : {critterData.Value.DefaultName}");
                    if (critterData.Value.DefaultName == Game1.player.ActiveObject.Name)
                    {
                        activeCritter = critterData.Value;
                        SMonitor.Log($"Active critter found: {activeCritter.DefaultName}");
                        break;
                    }
                }

                if (activeCritter != null)
                {
                    SMonitor.Log($"Attempting to spawn critter: {activeCritter.DefaultName}");

                    // Spawn the critter
                    int x = (int)e.Cursor.GrabTile.X + 1, y = (int)e.Cursor.GrabTile.Y + 1;
                    var critter = activeCritter.MakeCritter(x, y);

                    if (critter != null)
                    {
                        SMonitor.Log($"Critter spawn successful: {activeCritter.DefaultName}");
                        Game1.player.currentLocation.addCritter(critter);
                        Game1.player.reduceActiveItemByOne();
                        SMonitor.Log($"Active item count decremented by one.");
                    }
                    else
                    {
                        SMonitor.Log($"Failed to spawn critter: {activeCritter.DefaultName}. Critter object is null.");
                    }
                }
                else
                {
                    SMonitor.Log("No active critter found. Critter data is null.");
                }

                this.Helper.Input.Suppress(e.Button);
            }
        }
        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"Data/Objects"))
            {
                foreach (var critter in CrittersData)
                    e.Edit(
                        asset =>
                        {
                            var data = asset.AsDictionary<string, ObjectData>().Data;
                            // Check if texture exists
                            if (critter.Value == null || critter.Value.Texture == null)
                            {
                                this.Monitor.Log($"Critter or critter texture is null for '{critter.Key}'", LogLevel.Error);
                                return;
                            }
                            // Get translated name
                            string displayName = critter.Value.TranslatedName?.Invoke() ?? critter.Value.DefaultName;

                            int GetIndexFromRectangle(Rectangle rect)
                            {
                                int modPart = rect.X / 16;
                                int divPart = rect.Y / 16;

                                int index = divPart * 20 + modPart;
                                return index;
                            }
                            var objectData = new ObjectData
                            {
                                Name = critter.Value.DefaultName,
                                DisplayName = displayName,
                                Description = "A wild critter.",
                                Type = "Basic",
                                Category = StardewValley.Object.monsterLootCategory,
                                Price = critter.Value.DefaultName.Contains("Butterfly") ? 50 : 100,
                                Texture = critter.Value.Texture.Texture.Name,
                                SpriteIndex = GetIndexFromRectangle(critter.Value.Texture.SourceRect),
                                ContextTags = new List<string> { "critter" },
                                ExcludeFromShippingCollection = true
                            };
                            this.Monitor.Log($"Adding object data for '{critter.Key}': {objectData.Name}, {objectData.DisplayName}, {objectData.Description}, {objectData.Texture}, {objectData.SpriteIndex}", LogLevel.Trace);
                            data.Add(critter.Key, objectData);
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
    }
}
