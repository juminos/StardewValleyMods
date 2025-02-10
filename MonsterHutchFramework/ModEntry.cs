using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Monsters;
using StardewValley;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;
using System.Threading;
using StardewValley.GameData.Machines;
using xTile;
using static StardewValley.Minigames.CraneGame;
using StardewValley.Extensions;
using System.Drawing;
using MonsterHutchFramework.HarmonyPatches;

namespace MonsterHutchFramework
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;
        internal static ModEntry Mod { get; private set; }
        internal static ModConfig Config { get; private set; }
        internal string MonsterIncubatorAssetPath { get; private set; }
        public override void Entry(IModHelper helper)
        {
            Mod = this;
            Config = Helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            SMonitor = Monitor;
            SHelper = helper;

            ModConfig.VerifyConfigValues(Config, this);

            Helper.Events.GameLoop.GameLaunched += delegate { ModConfig.SetUpModConfigMenu(Config, this); };
            Helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            Helper.Events.GameLoop.DayStarted += OnDayStarted;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;

            try
            {
                HarmonyPatches.HutchPatches.PatchAll(this);
                HarmonyPatches.RingPatches.PatchAll(this);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            SHelper.ModContent.Load<Texture2D>("assets/monsterIncubator.png");
            MonsterIncubatorAssetPath = SHelper.ModContent.GetInternalAssetName("assets/monsterIncubator.png").BaseName; 
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
                        if (hutch.characters[i] is Monster monster && monster is not GreenSlime)
                        {
                            monsterList.Add(monster);
                        }
                    }
                    foreach (Monster monster in monsterList)
                    {
                        var pos = monster.Position;
                        var name = monster.Name;
                        bool foundMonster = false;
                        foreach (var monsterData in AssetHandler.monsterHutchData)
                        {
                            if (monsterData.Value.Name == name)
                            {
                                Monster newMonster = MonsterBuilder.CreateMonster(pos, monsterData.Value);
                                hutch.characters.Remove(monster);
                                hutch.characters.Add(newMonster);
                                foundMonster = true;
                                break;
                            }
                        }
                        if (!foundMonster)
                        {
                            SMonitor.Log($"monster name {name} not found in monster data", LogLevel.Error);
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
                        monster.modData.Remove($"{this.ModManifest.UniqueID}_monsterPetted");
                        if (Config.SkipRandomizeSlimePositions && monster is GreenSlime)
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
            MonsterHutch.ExpandMonsterHutchInterior(e);
            MonsterIncubator.AddIncubatorAssetChanges(e);
            AssetHandler.OnAssetRequested(sender, e);
        }
        private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            AssetHandler.OnAssetsInvalidated(sender, e);
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
                    var monsterPos = new Vector2(monster.Tile.X, monster.Tile.Y);
                    if (Math.Abs(monsterPos.X - playerTile.X) <= 1 && Math.Abs(monsterPos.Y - playerTile.Y) <= 1)
                    {
                        if (RingPatches.MonsterIsCharmed(monster, Game1.player) && !monster.modData.ContainsKey($"{this.ModManifest.UniqueID}_monsterPetted"))
                        {
                            DelayedAction.textAboveHeadAfterDelay("<", monster, Game1.random.Next(600));
                            monster.modData.Add($"{this.ModManifest.UniqueID}_monsterPetted", "true");
                            //monster.showTextAboveHead("<", null, 2, 1500, Game1.random.Next(500));
                            foreach(var monsterData in AssetHandler.monsterHutchData)
                            {
                                if (monsterData.Value.Name == monster.Name)
                                {
                                    DelayedAction.playSoundAfterDelay(monsterData.Value.Sound, Game1.random.Next(600), hutch);
                                    //monster.playNearbySoundAll(monsterData.Value.Sound);
                                }
                            }    
                        }
                    }
                }
            }
        }
    }
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
