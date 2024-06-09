using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BugNet2.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.Tools;

namespace BugNet2
{
    /// <summary>The mod entry point.</summary>
    internal class ModEntry : StardewModdingAPI.Mod
    {
        /*********
        ** Fields
        *********/
        private static readonly Dictionary<string, CritterData> CrittersData = new();
        private static readonly Dictionary<string, ObjectData> CritterCageData = new();
        private static readonly Dictionary<string, string> ElleCritterNameReplacements = new Dictionary<string, string>
        {
            { "SummerButterflyBlue", "Morpho achilles Butterfly" },
            { "SummerButterflyGreen", "Goliath Butterfly" },
            { "SummerButterflyRed", "Ulysses Butterfly" },
            { "SummerButterflyPink", "Janetta Butterfly" },
            { "SummerButterflyYellow", "Periander Butterfly" },
            { "SummerButterflyOrange", "Monarch Butterfly" },    
            //{ "SummerButterflyRareRed", "ElleRareButterflyRed" },
            //{ "SummerButterflyRarePink", "ElleRareButterflyPink" },
            //{ "SummerButterflyRareGreen", "ElleRareButterflyGreen" },
            //{ "SummerButterflyRarePurple", "ElleRareButterflyPurple" },
            { "SpringButterflyPalePink", "Azure Butterfly" },
            { "SpringButterflyMagenta", "Zoe Butterfly" },
            { "SpringButterflyWhite", "Cabbage Butterfly" },
            { "SpringButterflyYellow", "Cloudless Butterfly" },
            { "SpringButterflyPurple", "Common Butterfly" },
            { "SpringButterflyPink", "Migrant Butterfly" },
            { "WinterButterfly", "Morpho anaxibia Butterfly" },
            { "BlueBird", "Black Bird" },
            { "Squirrel", "ElleSquirrel" },
            { "WhiteRabbit", "White Lop Rabbit" },
            { "WoodPecker", "Crested Woodpecker" },
            { "Owl", "Barn Owl" },
    //{ "BlueParrot", "ElleBlueParrot" },
    //{ "GreenParrot", "ElleGreenParrot" },
            { "OrangeIslandButterfly", "Angel Butterfly" },
            { "PinkIslandButterfly", "Hairstreak Butterfly" },
            { "PurpleBird", "Jay" },
            { "RedBird", "Sparrow" },
            { "SunsetTropicalButterfly", "Crowned Butterfly" },
            { "TropicalButterfly", "Swallowtail Butterfly" },
            //{ "RedHeadBird", "ElleRedHeadBird" },
        };


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

            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Content.AssetRequested += this.OnAssetRequested;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            var tilesheet = helper.GameContent.Load<Texture2D>("TileSheets\\critters");
            var tilesheetBirds = helper.GameContent.Load<Texture2D>("LooseSprites\\birds");
            var tilesheetParrots = helper.GameContent.Load<Texture2D>("LooseSprites\\parrots");

            Rectangle GetTilesheetArea(Texture2D texture, int index)
            {
                if (texture.Name == "LooseSprites/birds")
                {
                    return new Rectangle(index % 10 * 16, index / 10 * 16, 16, 16);
                }
                else if(texture.Name == "LooseSprites/parrots")
                {
                    return new Rectangle(index % 11 * 24, index / 11 * 24, 24, 24);
                }
                else
                    return new Rectangle(index % 20 * 16, index / 20 * 16, 16, 16);
            }

            void Register(string name, Texture2D texture, int index, CritterBuilder critterBuilder)
            {
                string customTexturePath = Path.Combine(this.Helper.DirectoryPath, "assets", "items", name + ".png");
                string textureName = texture.Name;
                if (File.Exists(customTexturePath))
                {
                    texture = helper.ModContent.Load<Texture2D>($"assets\\items\\{name}");
                    textureName = Helper.ModContent.GetInternalAssetName($"assets/items/{name}").ToString()?.Replace("/", "\\");
                }
                // get custom critter texture name?
                //if (textureName.StartsWith("Critters"))
                //{
                //    textureName = Helper.ModContent.GetInternalAssetName(textureName.Substring(0, textureName.Length - 4)).ToString()?.Replace("/", "\\");
                //}

                this.RegisterCritter(
                    critterId: name,
                    texture: texture,
                    textureName: textureName,
                    textureArea: GetTilesheetArea(texture, index),
                    translationKey: $"critter.{name}",
                    isThisCritter: critterBuilder.IsThisCritter,
                    makeCritter: critterBuilder.MakeCritter
                );
            }

            Register("SummerButterflyBlue", tilesheet, 128, CritterBuilder.ForButterfly(128, summer: true));
            Register("SummerButterflyGreen", tilesheet, 148, CritterBuilder.ForButterfly(148, summer: true));
            Register("SummerButterflyRed", tilesheet, 132, CritterBuilder.ForButterfly(132, summer: true));
            Register("SummerButterflyPink", tilesheet, 152, CritterBuilder.ForButterfly(152, summer: true));
            Register("SummerButterflyYellow", tilesheet, 136, CritterBuilder.ForButterfly(136, summer: true));
            Register("SummerButterflyOrange", tilesheet, 156, CritterBuilder.ForButterfly(156, summer: true));
            Register("SummerButterflyRareRed", tilesheet, 480, CritterBuilder.ForButterfly(480, summer: true));
            Register("SummerButterflyRarePink", tilesheet, 484, CritterBuilder.ForButterfly(484, summer: true));
            Register("SummerButterflyRareGreen", tilesheet, 169, CritterBuilder.ForButterfly(169, summer: true));
            Register("SummerButterflyRarePurple", tilesheet, 173, CritterBuilder.ForButterfly(173, summer: true));
            Register("SpringButterflyPalePink", tilesheet, 160, CritterBuilder.ForButterfly(160, summer: false));
            Register("SpringButterflyMagenta", tilesheet, 180, CritterBuilder.ForButterfly(180, summer: false));
            Register("SpringButterflyWhite", tilesheet, 163, CritterBuilder.ForButterfly(163, summer: false));
            Register("SpringButterflyYellow", tilesheet, 183, CritterBuilder.ForButterfly(183, summer: false));
            Register("SpringButterflyPurple", tilesheet, 166, CritterBuilder.ForButterfly(166, summer: false));
            Register("SpringButterflyPink", tilesheet, 186, CritterBuilder.ForButterfly(186, summer: false));
            Register("WinterButterfly", tilesheet, 397, CritterBuilder.ForButterfly(397, summer: false));
            Register("BrownBird", tilesheetBirds, 0, CritterBuilder.ForBird(Birdie.brownBird));
            Register("BlueBird", tilesheetBirds, 10, CritterBuilder.ForBird(Birdie.blueBird));
            Register("GreenFrog", tilesheet, 280, CritterBuilder.ForFrog(olive: false));
            Register("OliveFrog", tilesheet, 300, CritterBuilder.ForFrog(olive: true));
            Register("Firefly", tilesheet, 10, CritterBuilder.ForFirefly());
            Register("Squirrel", tilesheet, 260, CritterBuilder.ForSquirrel());
            Register("GrayRabbit", tilesheet, 276, CritterBuilder.ForRabbit(white: false));
            Register("WhiteRabbit", tilesheet, 278, CritterBuilder.ForRabbit(white: true));
            Register("WoodPecker", tilesheet, 320, CritterBuilder.ForWoodpecker());
            Register("Seagull", tilesheet, 0, CritterBuilder.ForSeagull());
            Register("Owl", tilesheetBirds, 110, CritterBuilder.ForOwl());
            Register("Crow", tilesheetBirds, 100, CritterBuilder.ForCrow());
            Register("Cloud", tilesheet, 110, CritterBuilder.ForCloud());
            Register("BlueParrot", tilesheetParrots, 22, CritterBuilder.ForParrot(color: "blue"));
            Register("GreenParrot", tilesheetParrots, 0, CritterBuilder.ForParrot(color: "green"));
            Register("JojaParrot", tilesheetParrots, 44, CritterBuilder.ForParrot(color: "joja"));
            Register("Monkey", tilesheet, 420, CritterBuilder.ForMonkey());
            Register("OrangeIslandButterfly", tilesheet, 364, CritterBuilder.ForButterfly(364, island: true, summer: true));
            Register("PinkIslandButterfly", tilesheet, 368, CritterBuilder.ForButterfly(368, island: true, summer: true));
            Register("PurpleBird", tilesheet, 510, CritterBuilder.ForBird(125));
            Register("RedBird", tilesheet, 550, CritterBuilder.ForBird(135));
            Register("SunsetTropicalButterfly", tilesheet, 372, CritterBuilder.ForButterfly(372, island: true, summer: true));
            Register("TropicalButterfly", tilesheet, 376, CritterBuilder.ForButterfly(376, island: true, summer: true));
            Register("Opossum", tilesheet, 600, CritterBuilder.ForOpossum());
            Register("RedHeadBird", tilesheet, 670, CritterBuilder.ForBird(165));
            Register("Dove", tilesheet, 710, CritterBuilder.ForBird(175));
        }

        /// <inheritdoc />
        public override object GetApi()
        {
            return new BugNetApi(this.RegisterCritter, this.Monitor);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Add a new critter which can be caught.</summary>
        /// <param name="critterId">The unique critter ID.</param>
        /// <param name="texture">The texture to show in the inventory.</param>
        /// <param name="textureName">The texture to add to objects.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to show in the inventory.</param>
        /// <param name="translationKey">The translation key for the critter name.</param>
        /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        private void RegisterCritter(string critterId, Texture2D texture, string textureName, Rectangle textureArea, string translationKey, Func<int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter)
        {
            // get name translations
            this.GetTranslationsInAllLocales(
                translationKey,
                out string defaultCritterName,
                out Dictionary<string, string> critterNameTranslations
            );

            // register critter
            this.RegisterCritter(critterId, texture, textureName, textureArea, defaultCritterName, critterNameTranslations, makeCritter, isThisCritter);
        }

        /// <summary>Add a new critter which can be caught.</summary>
        /// <param name="critterId">The unique critter ID.</param>
        /// <param name="texture">The texture to show in the inventory.</param>
        /// <param name="textureArea">The pixel area within the <paramref name="texture"/> to show in the inventory.</param>
        /// <param name="defaultCritterName">The default English critter name.</param>
        /// <param name="translatedCritterNames">The translated critter names in each available locale.</param>
        /// <param name="makeCritter">Create a critter instance at the given X and Y tile position.</param>
        /// <param name="isThisCritter">Get whether a given critter instance matches this critter.</param>
        private void RegisterCritter(string critterId, Texture2D texture, string textureName, Rectangle textureArea, string defaultCritterName, Dictionary<string, string> translatedCritterNames, Func<int, int, Critter> makeCritter, Func<Critter, bool> isThisCritter)
        {
            // get translations
            string TranslateCritterName(string locale)
            {
                return translatedCritterNames.GetValueOrDefault(locale) ?? defaultCritterName;
            }

            // save critter data
            ModEntry.CrittersData.Add(critterId, new CritterData(
                defaultName: defaultCritterName,
                translatedName: () => TranslateCritterName(this.Helper.GameContent.CurrentLocale),
                texture: new TextureTarget(texture, textureArea),
                textureName: textureName,
                isThisCritter: isThisCritter,
                makeCritter: makeCritter
            ));
            SMonitor.Log($"Registering {critterId}: defaultName {defaultCritterName}, translatedName {() => TranslateCritterName(this.Helper.GameContent.CurrentLocale)}, texture {texture.Name}");

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
                        if (!ModEntry.TryGetCritter(critter, out CritterData data, out string critterId))
                        {
                            SMonitor.Log($"Critter not supported by BugNet2 mod", LogLevel.Debug);
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
                foreach (var critterData in ModEntry.CrittersData)
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

                            // Update displayName if Elle's Town Animals is installed
                            if (Helper.ModRegistry.IsLoaded("Elle.TownAnimals") && ElleCritterNameReplacements.TryGetValue(critter.Key, out string newName))
                            {
                                displayName = newName;
                            }

                            int GetIndexFromRectangle(Rectangle rect, int widthMultiplier)
                            {
                                int modPart = rect.X / 16;
                                int divPart = rect.Y / 16;

                                int index = divPart * widthMultiplier + modPart;
                                return index;
                            }

                            int spriteIndex = 0;
                            if (critter.Value.Texture.Texture.Name == "TileSheets/critters" && critter.Value.Texture.SourceRect.Width == 16 && critter.Value.Texture.SourceRect.Height == 16)
                            {
                                spriteIndex = GetIndexFromRectangle(critter.Value.Texture.SourceRect, 20);
                            }
                            if (critter.Value.Texture.Texture.Name == "LooseSprites/birds")
                            {
                                spriteIndex = GetIndexFromRectangle(critter.Value.Texture.SourceRect, 10);
                            }

                            var objectData = new ObjectData
                            {
                                Name = critter.Value.DefaultName,
                                DisplayName = displayName,
                                Description = "A wild critter.",
                                Type = "Basic",
                                Category = StardewValley.Object.monsterLootCategory,
                                Price = critter.Value.DefaultName.Contains("Butterfly") ? 50 : 100,
                                Texture = critter.Value.TextureName,
                                SpriteIndex = spriteIndex,
                                ContextTags = new List<string> { "critter" },
                                ExcludeFromShippingCollection = true
                            };
                            this.Monitor.Log($"Adding object data for '{critter.Key}': {objectData.Name}, {objectData.DisplayName}, {objectData.Description}, {objectData.Texture}, {objectData.SpriteIndex}", LogLevel.Trace);
                            data.Add(critter.Key, objectData);
                        });
                return;
            }
        }

        /// <summary>Get the data for a given critter, if it's supported by BugNet2.</summary>
        /// <param name="critter">The critter to match.</param>
        /// <param name="data">The critter data.</param>
        /// <returns>Returns whether the critter data was found.</returns>
        internal static bool TryGetCritter(Critter critter, out CritterData data, out string critterId)
        {
            foreach (var pair in ModEntry.CrittersData)
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
