using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Monsters;
using StardewValley;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonsterHutchFramework.HarmonyPatches;
using StardewValley.BellsAndWhistles;
using StardewModdingAPI.Utilities;
using StardewValley.Objects;
using System.Net.NetworkInformation;
using System.Threading;

namespace MonsterHutchFramework
{
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;
        internal static ModEntry Mod { get; private set; }
        internal static ModConfig Config { get; private set; }
        internal string MonsterIncubatorAssetPath { get; private set; }
        internal string MonsterHutchExteriorPath { get; private set; }
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
            Helper.Events.GameLoop.DayEnding += OnDayEnding;
            Helper.Events.Player.Warped += OnPlayerWarped;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;

            try
            {
                HutchPatches.PatchAll(this);
                RingPatches.PatchAll(this);
                ObjectPatches.PatchAll(this);
                IncubatorPatches.PatchAll(this);
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
                return;
            }

            SHelper.ModContent.Load<Texture2D>("assets/monsterIncubator.png");
            MonsterIncubatorAssetPath = SHelper.ModContent.GetInternalAssetName("assets/monsterIncubator.png").BaseName;
            SHelper.ModContent.Load<Texture2D>("assets/MonsterHutch.png");
            MonsterHutchExteriorPath = SHelper.ModContent.GetInternalAssetName("assets/MonsterHutch.png").BaseName;
        }
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            Utility.ForEachBuilding(delegate (Building building)
            {
                if (building?.indoors?.Value is SlimeHutch hutch &&
                hutch.characters.Count > 0)
                {
                    var monsterList = new List<Monster>();
                    for (int i = 0; i < hutch.characters.Count; i++)
                    {
                        if (hutch.characters[i] is Monster monster && monster is not null && monster.modData.ContainsKey($"{ModEntry.Mod.ModManifest.UniqueID}_Name"))
                            monsterList.Add(monster);
                    }
                    if (monsterList.Count > 0)
                    {
                        foreach (Monster monster in monsterList)
                        {
                            var pos = monster.Position;
                            if (monster.modData.ContainsKey($"{this.ModManifest.UniqueID}_Position"))
                            {
                                var posString = monster.modData[$"{this.ModManifest.UniqueID}_Position"];
                                string[] values = posString.Split(',');
                                var posXstr = values[0];
                                var posYstr = values[1];
                                int posX = Convert.ToInt32(posXstr);
                                int posY = Convert.ToInt32(posYstr);
                                pos = new Vector2(posX, posY);
                            }
                            var name = monster.modData[$"{ModEntry.Mod.ModManifest.UniqueID}_Name"];
                            if (!monster.modData.ContainsKey($"{ModEntry.Mod.ModManifest.UniqueID}_Scale"))
                            {
                                var getScale = monster.Scale;
                                monster.modData.Add($"{ModEntry.Mod.ModManifest.UniqueID}_Scale", getScale.ToString());
                            }
                            float scale = float.Parse(monster.modData[$"{ModEntry.Mod.ModManifest.UniqueID}_Scale"]);
                            bool foundMonster = false;
                            foreach (var monsterData in AssetHandler.monsterHutchData)
                            {
                                if (monsterData.Value.Name == name)
                                {
                                    Monster newMonster = MonsterBuilder.CreateMonster(pos, monsterData.Value, scale);
                                    if (newMonster != null)
                                    {
                                        hutch.characters.Remove(monster);
                                        hutch.characters.Add(newMonster);
                                    }
                                    else
                                        SMonitor.Log($"Unable to create monster {monsterData.Value.Name}", LogLevel.Error);
                                    foundMonster = true;
                                    break;
                                }
                            }
                            if (!foundMonster)
                            {
                                SMonitor.Log($"monster name {name} not found in monster data, removing monster", LogLevel.Error);
                                hutch.characters.Remove(monster);
                            }
                        }
                    }
                }
                return true;
            });
        }
        private void OnDayStarted(object? sender, DayStartedEventArgs e)
        {
            Utility.ForEachBuilding(delegate (Building building)
            {
                if (building?.indoors?.Value is SlimeHutch hutch &&
                hutch.characters.Count > 0)
                {
                    foreach (var monster in hutch.characters)
                    {
                        if (monster is null)
                            continue;
                        if (monster is Monster && monster.modData.ContainsKey($"{this.ModManifest.UniqueID}_monsterPetted"))
                            monster.modData.Remove($"{this.ModManifest.UniqueID}_monsterPetted");
                        if (!Config.RandomizeMonsterPositions)
                            break;
                        if (Config.SkipRandomizeSlimePositions && monster is GreenSlime)
                            continue;
                        int tries = 50;
                        Vector2 tile = hutch.getRandomTile();
                        while ((!hutch.CanItemBePlacedHere(tile, false, CollisionMask.All, ~CollisionMask.Objects, false, false) || tile.Y >= 17f) && tries > 0)
                        {
                            tile = hutch.getRandomTile();
                            tries--;
                        }

                        tile *= 64;

                        if (tries > 0)
                            monster.Position = tile;
                    }
                }
                return true;
            });
        }
        private void OnDayEnding(object? sender, DayEndingEventArgs e)
        {
            if (Config.RandomizeMonsterPositions)
                return;
            Utility.ForEachBuilding(delegate (Building building)
            {
                if (building?.indoors?.Value is SlimeHutch hutch &&
                hutch.characters.Count > 0)
                {
                    foreach (var monster in hutch.characters)
                    {
                        if (monster is null)
                            continue;
                        var posX = (int)monster.Position.X;
                        var posY = (int)monster.Position.Y;
                        string posString = posX.ToString() + "," + posY.ToString();
                        if (monster is Monster && !monster.modData.ContainsKey($"{this.ModManifest.UniqueID}_Position"))
                            monster.modData.Add($"{this.ModManifest.UniqueID}_Position", posString);
                        else if (monster is Monster && monster.modData.ContainsKey($"{this.ModManifest.UniqueID}_Position"))
                            monster.modData[$"{this.ModManifest.UniqueID}_Position"] = posString;
                    }
                }
                return true;
            });
        }
        private void OnPlayerWarped(object? sender, WarpedEventArgs e)
        {
            foreach(var character in e.OldLocation.characters)
            {
                if (character is Monster monster && monster.modData.ContainsKey($"{this.ModManifest.UniqueID}_monsterPetted"))
                    monster.modData.Remove($"{this.ModManifest.UniqueID}_monsterPetted");
            }
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
            var ringData = AssetHandler.charmerRingData;
            if (!Context.IsPlayerFree)
                return;
            if (!Context.IsWorldReady)
                return;
            if (Game1.activeClickableMenu != null)
                return;
            if (!Game1.player.UsingTool &&
                !Game1.player.isEating &&
                !Game1.player.IsBusyDoingSomething() &&
                e.Button.IsActionButton())
            {
                foreach (var character in Game1.player.currentLocation.characters)
                {
                    if (character is Monster monster)
                    {
                        var playerTile = Game1.player.Tile;
                        var tileRect = new Microsoft.Xna.Framework.Rectangle((int)playerTile.X * 64, (int)playerTile.Y * 64, 64, 64);
                        var monsterPos = new Vector2(monster.Tile.X, monster.Tile.Y);
                        if (monster.GetBoundingBox().Intersects(tileRect) &&
                            RingPatches.MonsterIsCharmed(monster, Game1.player, out string? matchRingKey, out int matchMonsterIndex) &&
                            matchRingKey != null &&
                            !monster.modData.ContainsKey($"{this.ModManifest.UniqueID}_monsterPetted") &&
                            (string.IsNullOrEmpty(ringData[matchRingKey].CharmedMonsters[matchMonsterIndex].SpeechCondition) ||
                            GameStateQuery.CheckConditions(ringData[matchRingKey].CharmedMonsters[matchMonsterIndex].SpeechCondition)))
                        {
                            GetCharmedSpeech(monster, matchRingKey, matchMonsterIndex, out string text, out Color? color, out int style, out int duration, out int preTimer);
                            var playSound = ringData[matchRingKey].CharmedMonsters[matchMonsterIndex].Sound;
                            var date = SDate.Now();
                            if ((monster is ShadowBrute || monster is ShadowGirl || monster is ShadowGuy || monster is ShadowShaman || monster is Shooter) &&
                                date.DayOfWeek == DayOfWeek.Friday)
                            {
                                text = "...";
                                playSound = "";
                            }
                            monster.showTextAboveHead(text, color, style, duration > 0 ? duration : 1500, preTimer > -1 ? preTimer : 0);

                            if (!string.IsNullOrEmpty(playSound))
                                DelayedAction.playSoundAfterDelay(playSound, preTimer, Game1.player.currentLocation, monsterPos);
                            
                            monster.modData.Add($"{this.ModManifest.UniqueID}_monsterPetted", "true");
                        }
                    }
                }
            }
        }
        public static void GetCharmedSpeech(Monster monster, string ringKey, int monsterIndex, out string text, out Color? color, out int style, out int duration, out int preTimer)
        {
            var speechList = new List<int>();
            var speechData = AssetHandler.charmerRingData[ringKey].CharmedMonsters[monsterIndex];
            if (speechData.SpeechBubbles.Count == 1)
            {
                text = speechData.SpeechBubbles[0].Text ?? "<";
                color = SpriteText.getColorFromIndex(speechData.SpeechBubbles[0].Color);
                style = speechData.SpeechBubbles[0].Style;
                duration = speechData.SpeechBubbles[0].Duration;
                preTimer = speechData.SpeechBubbles[0].Pretimer;
                return;
            }
            else if (speechData.SpeechBubbles.Count > 1)
            {
                for (int i = 0; i < speechData.SpeechBubbles.Count; i++)
                {
                    for (int j = 0; j < speechData.SpeechBubbles[i].Weight; j++)
                    {
                        speechList.Add(i);
                    }
                }
                var speechIndex = Game1.random.Next(speechList.Count);
                var speechEntry = speechData.SpeechBubbles[speechList[speechIndex]];
                text = speechEntry.Text ?? "<";
                color = SpriteText.getColorFromIndex(speechEntry.Color);
                style = speechEntry.Style;
                duration = speechEntry.Duration;
                preTimer = speechEntry.Pretimer;
                return;
            }
            else
            {
                text = "<";
                color = SpriteText.color_Default;
                style = 2;
                duration = 1500;
                preTimer = -1;
                return;
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
