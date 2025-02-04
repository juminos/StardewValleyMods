using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Mods;
using StardewValley.Monsters;
using System;
using System.Collections;
using System.Threading;
using xTile;
using static StardewValley.Minigames.CraneGame;

namespace MonsterHutch
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;

        internal const string coalId = "(O)382";
        internal const string coffeeBeanId = "(O)433";
        internal const string coffeeId = "(O)395";
        internal const string tripleShotId = "(O)253";
        internal const string bugMeatId = "(O)684";
        internal const string bugSteakId = "(O)874";
        internal const string solarEssenceId = "(O)768";
        internal const string voidEssenceId = "(O)769";
        internal const string mummifiedBatId = "(O)827";
        internal const string batWingId = "(O)767";
        internal const string cinderShardId = "(O)848";
        internal const string cinderStone1Id = "(O)843";
        internal const string cinderStone2Id = "(O)844";
        internal const string truffleId = "(O)430";
        internal const string iridiumOreId = "(O)386";
        internal const string iridiumNodeId = "(O)765";
        internal const string goldOreId = "(O)384";
        internal const string goldNodeId = "(O)764";
        internal const string ironOreId = "(O)380";
        internal const string ironNodeId = "(O)850";
        internal const string copperOreId = "(O)378";
        internal const string copperNodeId = "(O)849";
        internal const string crabId = "(O)717";
        internal const string gingerId = "(O)829";
        internal const string obsidianId = "(O)575";

        internal static ModEntry Mod { get; private set; }
        internal static ModConfig Config { get; private set; }
        internal string MonsterIncubatorAssetPath { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Mod = this;
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            ModConfig.VerifyConfigValues(Config, this);

            Helper.Events.GameLoop.GameLaunched += delegate { ModConfig.SetUpModConfigMenu(Config, this); };
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;

            try
            {
                HarmonyPatches.PatchAll(this);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            MonsterIncubatorAssetPath = Helper.ModContent.GetInternalAssetName("assets/monsterIncubator.png").BaseName;
        }
        public static DustSpirit CreateDustSprite(Vector2 vector)
        {
            var monster = new DustSpirit(vector, false);
            monster.Speed /= 2;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.coalId);
            monster.objectsToDrop.Add(ModEntry.coffeeId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }

        public static Bat CreateRandomBat(Vector2 vector)
        {
            var monster = new Bat(vector);
            if (Game1.random.NextDouble() < 0.75)
            {
                monster.Name = "Frost Bat";
                if (Game1.random.NextDouble() < 0.67)
                {
                    monster.Name = "Lava Bat";
                    if (Game1.random.NextDouble() < 0.5)
                    {
                        monster.Name = "Iridium Bat";
                    }
                }
            }
            monster.reloadSprite();

            // need to implement method for saving dangerous variants for save loaded
            //if ((monster.Name == "Frost Bat" || monster.Name == "Bat") && Game1.random.NextDouble() < 0.5)
            //{
            //    string newAssetName = monster.Sprite.textureName.Value + "_dangerous";
            //    try
            //    {
            //        monster.Sprite.LoadTexture(newAssetName);
            //    }
            //    catch (Exception exception)
            //    {
            //        SMonitor.Log($"Failed loading '{newAssetName}' texture for dangerous {monster.Name}.", LogLevel.Error);
            //    }
            //}

            monster.Speed = 1;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.batWingId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static Bat CreateBat(Vector2 vector)
        {
            var monster = new Bat(vector);
            monster.reloadSprite();
            monster.Speed = 1;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.batWingId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static Bat CreateFrostBat(Vector2 vector)
        {
            var monster = new Bat(vector);
            monster.Name = "Frost Bat";
            monster.reloadSprite();
            monster.Speed = 1;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.batWingId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static Bat CreateLavaBat(Vector2 vector)
        {
            var monster = new Bat(vector);
            monster.Name = "Lava Bat";
            monster.reloadSprite();
            monster.Speed = 1;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.batWingId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static Bat CreateIridiumBat(Vector2 vector)
        {
            var monster = new Bat(vector);
            monster.Name = "Iridium Bat";
            monster.reloadSprite();
            monster.Speed = 1;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.batWingId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static Bat CreateDangerousBat(Vector2 vector)
        {
            var monster = new Bat(vector);
            monster.reloadSprite();
                string newAssetName = monster.Sprite.textureName.Value + "_dangerous";
                try
                {
                    monster.Sprite.LoadTexture(newAssetName);
                }
                catch (Exception exception)
                {
                    SMonitor.Log($"Failed loading '{newAssetName}' texture for dangerous {monster.Name}.", LogLevel.Error);
                }
            monster.Name = "Dangerous Bat";
            monster.Speed = 1;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.batWingId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static Bat CreateDangerousFrostBat(Vector2 vector)
        {
            var monster = new Bat(vector);
            monster.reloadSprite();
                string newAssetName = monster.Sprite.textureName.Value + "_dangerous";
                try
                {
                    monster.Sprite.LoadTexture(newAssetName);
                }
                catch (Exception exception)
                {
                    SMonitor.Log($"Failed loading '{newAssetName}' texture for dangerous {monster.Name}.", LogLevel.Error);
                }
            monster.Name = "Dangerous Frost Bat";
            monster.Speed = 1;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.batWingId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static Leaper CreateSpider(Vector2 vector)
        {
            var monster = new Leaper(vector);
            monster.Speed /= 2;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.bugMeatId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static Bat CreateMagmaSprite(Vector2 vector)
        {
            var monster = new Bat(vector, -555);
            monster.Name = "Magma Sprite";
            monster.magmaSprite.Value = true;
            monster.reloadSprite();
            monster.Speed = 1;
            monster.addedSpeed = 0;
            monster.lungeSpeed = 0;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.cinderShardId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static Bat CreateMagmaSparker(Vector2 vector)
        {
            var monster = new Bat(vector, -556);
            monster.Name = "Magma Sparker";
            monster.magmaSprite.Value = true;
            monster.reloadSprite();
            monster.Speed = 1;
            monster.addedSpeed = 0;
            monster.lungeSpeed = 0;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.cinderShardId);
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static RockCrab CreateRockCrab(Vector2 vector)
        {
            var monster = new RockCrab(vector);
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.crabId);
            monster.waiter = false;
            monster.farmerPassesThrough = true;
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static RockCrab CreateIridiumCrab(Vector2 vector)
        {
            var monster = new RockCrab(vector, "Iridium Crab");
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.crabId);
            monster.waiter = false;
            monster.farmerPassesThrough = true;
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static RockCrab CreateGoldCrab(Vector2 vector)
        {
            var monster = new RockCrab(vector, "Lava Crab");
                string newAssetName = monster.Sprite.textureName.Value + "_dangerous";
                try
                {
                    monster.Sprite.LoadTexture(newAssetName);
                }
                catch
                {
                    SMonitor.Log($"Failed loading '{newAssetName}' texture for gold {monster.Name}.", LogLevel.Error);
                }
            monster.Name = "Gold Crab";
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.crabId);
            monster.waiter = false;
            monster.farmerPassesThrough = true;
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static RockCrab CreateIronCrab(Vector2 vector)
        {
            var monster = new RockCrab(vector);
                string newAssetName = monster.Sprite.textureName.Value + "_iron";
                try
                {
                    monster.Sprite.LoadTexture(newAssetName);
                }
                catch
                {
                    SMonitor.Log($"Failed loading '{newAssetName}' texture for iron {monster.Name}.", LogLevel.Error);
                }
            monster.Name = "Iron Crab";
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.crabId);
            monster.waiter = false;
            monster.farmerPassesThrough = true;
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static RockCrab CreateCopperCrab(Vector2 vector)
        {
            var monster = new RockCrab(vector);
                string newAssetName = monster.Sprite.textureName.Value + "_copper";
                try
                {
                    monster.Sprite.LoadTexture(newAssetName);
                }
                catch
                {
                    SMonitor.Log($"Failed loading '{newAssetName}' texture for copper {monster.Name}.", LogLevel.Error);
                }
            monster.Name = "Copper Crab";
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.crabId);
            monster.waiter = false;
            monster.farmerPassesThrough = true;
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }
        public static RockCrab CreateTruffleCrab(Vector2 vector)
        {
            var monster = new RockCrab(vector, "Truffle Crab");
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.crabId);
            monster.farmerPassesThrough = true;

            return monster;
        }
        public static RockCrab CreateStickBug(Vector2 vector)
        {
            var monster = new RockCrab(vector);
            monster.makeStickBug();
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.gingerId);
            monster.waiter = false;
            monster.farmerPassesThrough = true;
            monster.moveTowardPlayerThreshold.Value = 2;

            return monster;
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;
            if (!Context.IsWorldReady)
                return;
            if (Game1.activeClickableMenu != null)
                return;
            if (!Game1.player.UsingTool &&
                !Game1.player.isEating &&
                Game1.currentLocation is SlimeHutch hutch &&
                e.Button.IsActionButton())
            {
                var playerTile = Game1.player.Tile;
                foreach (Monster monster in hutch.characters)
                {
                    if (Math.Abs(monster.Tile.X - playerTile.X) <= 1 && Math.Abs(monster.Tile.Y - playerTile.Y) <= 1)
                    {
                        if (
                            (monster is DustSpirit dust && !dust.isHardModeMonster.Value && Game1.player.isWearingRing("juminos.FrenshipRings.CP_Dust")) ||
                            (monster is Leaper && Game1.player.isWearingRing("juminos.FrenshipRings.CP_Spider")) ||
                            (monster is Bat bat && !bat.magmaSprite.Value && !bat.hauntedSkull.Value && Game1.player.isWearingRing("juminos.FrenshipRings.CP_Bat")) ||
                            (monster is GreenSlime && Game1.player.isWearingRing("520")) ||
                            (monster is Bat magmaSprite && magmaSprite.magmaSprite.Value && Game1.player.isWearingRing("juminos.FrenshipRings.CP_MagmaSprite")) ||
                            ((monster is RockCrab || monster.Name == "Truffle Crab") && Game1.player.isWearingRing("810"))
                            )
                        {
                            monster.showTextAboveHead("<", null, 2, 1500, 0);
                        }
                    }
                }
            }
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Utility.ForEachBuilding(delegate (Building building)
            {
                if (building?.indoors?.Value is SlimeHutch hutch)
                {
                    var monsterList = new List<Monster>();
                    for (int i = 0; i < hutch.characters.Count; i++)
                    {
                        var pos = hutch.characters[i].Position;
                        if (hutch.characters[i] is DustSpirit)
                        {
                            hutch.characters[i] = CreateDustSprite(pos);
                        }
                        else if (hutch.characters[i] is Bat bat)
                        {
                            if (bat.magmaSprite.Value)
                            {
                                if (bat.Name == "Magma Sparker")
                                {
                                    hutch.characters[i] = CreateMagmaSparker(pos);
                                }
                                else
                                {
                                    hutch.characters[i] = CreateMagmaSprite(pos);
                                }
                            }

                            else if (bat.Name == "Lava Bat")
                            {
                                hutch.characters[i] = CreateLavaBat(pos);
                            }
                            else if (bat.Name == "Iridium Bat")
                            {
                                hutch.characters[i] = CreateIridiumBat(pos);
                            }
                            else
                                hutch.characters[i] = CreateBat(pos);
                        }
                        else if (hutch.characters[i] is Leaper)
                        {
                            hutch.characters[i] = CreateSpider(pos);
                        }
                        else if (hutch.characters[i] is RockCrab crab)
                        {
                            if (crab.Name == "Iridium Crab")
                            {
                                hutch.characters[i] = CreateIridiumCrab(pos);
                            }
                            else if (crab.Name == "Gold Crab")
                            {
                                hutch.characters[i] = CreateGoldCrab(pos);
                            }
                            else if (crab.Name == "Iron Crab")
                            {
                                hutch.characters[i] = CreateIronCrab(pos);
                            }
                            else if (crab.Name == "Copper Crab")
                            {
                                hutch.characters[i] = CreateCopperCrab(pos);
                            }
                            else if (crab.Name == "Truffle Crab")
                            {
                                hutch.characters[i] = CreateTruffleCrab(pos);
                            }
                            else if (crab.isStickBug.Value || crab.Name == "Stick Bug")
                            {
                                hutch.characters[i] = CreateStickBug(pos);
                            }
                            else
                                hutch.characters[i] = CreateRockCrab(pos);
                        }
                    }
                }
                return true;
            });
        }

        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            if (!Config.RandomizeMonsterPositions)
            {
                return;
            }

            Utility.ForEachBuilding(delegate (Building building)
            {
                if (building?.indoors?.Value is SlimeHutch hutch)
                {
                    foreach (var monster in hutch.characters)
                    {
                        if (Config.RandomizeOnlyModMonsterPositions && monster is GreenSlime)
                        {
                            continue;
                        }

                        int tries = 50;
                        Vector2 tile = hutch.getRandomTile();
                        while ((!hutch.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || tile.Y >= 12f) && tries > 0)
                        {
                            tile = hutch.getRandomTile();
                            tries--;
                        }

                        tile *= 64;

                        if (tries > 0)
                        {
                            monster.Position = tile;
                        }
                    }
                }
                return true;
            });
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            //ApplyHutchInteriorChanges(e);
            MonsterIncubator.AddIncubatorAssetChanges(e);
        }

        // Hutch interior changes made with CP 
        //private void ApplyHutchInteriorChanges(AssetRequestedEventArgs e)
        //{
        //    if (e.NameWithoutLocale.IsEquivalentTo("Maps/SlimeHutch"))
        //    {
        //        e.Edit((asset) =>
        //        {
        //            var editor = asset.AsMap();

        //            var newVal = Helper.ModContent.Load<Map>("assets/MonsterHutch.tmx");

        //            if (newVal != null)
        //            {
        //                editor.ReplaceWith(newVal);
        //            }
        //        });
        //    }
        //}
    }

    /// <summary>
    /// Extension methods for IGameContentHelper.
    /// </summary>
    public static class GameContentHelperExtensions
    {
        /// <summary>
        /// Invalidates both an asset and the locale-specific version of an asset.
        /// </summary>
        /// <param name="helper">The game content helper.</param>
        /// <param name="assetName">The (string) asset to invalidate.</param>
        /// <returns>if something was invalidated.</returns>
        public static bool InvalidateCacheAndLocalized(this IGameContentHelper helper, string assetName)
            => helper.InvalidateCache(assetName)
                | (helper.CurrentLocaleConstant != LocalizedContentManager.LanguageCode.en && helper.InvalidateCache(assetName + "." + helper.CurrentLocale));
    }

}
