using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Characters;
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

        internal static ModEntry Mod { get; private set; }
        internal static ModConfig Config { get; private set; }
        internal string MonsterIncubatorAssetPath { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Mod = this;
            Config = Helper.ReadConfig<ModConfig>();

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

            return monster;
        }

        public static Bat CreateBat(Vector2 vector)
        {
            var monster = new Bat(vector);
            if (Game1.random.NextDouble() < 0.65)
            {
                monster.Name = "Frost Bat";
            }
            if (Game1.random.NextDouble() < 0.5)
            {
                monster.Name = "Lava Bat";
            }
            if (Game1.random.NextDouble() < 0.3)
            {
                monster.Name = "Iridium Bat";
            }
            monster.reloadSprite();
            monster.Speed /= 4;
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
            monster.Speed /= 4;
            monster.farmerPassesThrough = true;
            monster.objectsToDrop.Clear();
            monster.objectsToDrop.Add(ModEntry.cinderShardId);
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
                if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Dust"))
                {
                    foreach (DustSpirit dust in hutch.characters)
                    {
                        if (!dust.isHardModeMonster.Value && Math.Abs(dust.Tile.X - playerTile.X) <= 1 && Math.Abs(dust.Tile.Y - playerTile.Y) <= 1)
                        {
                            dust.showTextAboveHead("<", null, 2, 2000, 0);
                        }
                    }
                }
                if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Spider"))
                {
                    foreach (Leaper leaper in hutch.characters)
                    {
                        if (Math.Abs(leaper.Tile.X - playerTile.X) <= 1 && Math.Abs(leaper.Tile.Y - playerTile.Y) <= 1)
                        {
                            leaper.showTextAboveHead("<", null, 2, 2000, 0);
                        }
                    }
                }
                if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_Bat"))
                {
                    foreach (Bat bat in hutch.characters)
                    {
                        if (!bat.magmaSprite.Value && !bat.hauntedSkull.Value && Math.Abs(bat.Tile.X - playerTile.X) <= 1 && Math.Abs(bat.Tile.Y - playerTile.Y) <= 1)
                        {
                            bat.showTextAboveHead("<", null, 2, 2000, 0);
                        }
                    }
                }
                if (Game1.player.isWearingRing("juminos.FrenshipRings.CP_MagmaSprite"))
                {
                    foreach (Bat magmaSprite in hutch.characters)
                    {
                        if (magmaSprite.magmaSprite.Value && Math.Abs(magmaSprite.Tile.X - playerTile.X) <= 1 && Math.Abs(magmaSprite.Tile.Y - playerTile.Y) <= 1)
                        {
                            magmaSprite.showTextAboveHead("<", null, 2, 2000, 0);
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
                    for (int i = 0; i < hutch.characters.Count; i++)
                    {
                        if (hutch.characters[i] is DustSpirit)
                        {
                            var pos = hutch.characters[i].Position;
                            hutch.characters[i] = CreateDustSprite(pos);
                        }
                        else if (hutch.characters[i] is Bat bat && !bat.magmaSprite.Value && !bat.hauntedSkull.Value)
                        {
                            var pos = hutch.characters[i].Position;
                            hutch.characters[i] = CreateBat(pos);
                        }
                        else if (hutch.characters[i] is Leaper)
                        {
                            var pos = hutch.characters[i].Position;
                            hutch.characters[i] = CreateSpider(pos);
                        }
                        else if (hutch.characters[i] is Bat magmaSprite && magmaSprite.magmaSprite.Value)
                        {
                            var pos = hutch.characters[i].Position;
                            hutch.characters[i] = CreateBat(pos);
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

        private void ApplyHutchInteriorChanges(AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/SlimeHutch"))
            {
                e.Edit((asset) =>
                {
                    var editor = asset.AsMap();

                    var newVal = Helper.ModContent.Load<Map>("assets/MonsterHutch.tmx");

                    if (newVal != null)
                    {
                        editor.ReplaceWith(newVal);
                    }
                });
            }
        }
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
