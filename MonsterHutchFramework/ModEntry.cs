using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Monsters;
using StardewValley;
using StardewValley.Buildings;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonsterHutchFramework.HarmonyPatches;
using StardewValley.BellsAndWhistles;

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
            Helper.Events.Player.Warped += OnPlayerWarped;
            Helper.Events.Content.AssetRequested += OnAssetRequested;
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            Helper.Events.Content.AssetsInvalidated += OnAssetsInvalidated;

            try
            {
                HarmonyPatches.HutchPatches.PatchAll(this);
                HarmonyPatches.RingPatches.PatchAll(this);
                HarmonyPatches.ObjectPatches.PatchAll(this);
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
                (hutch.Name.Contains("MonsterHutchFramework") || hutch.Name.Contains("Winery")))
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
                if (building?.indoors?.Value is SlimeHutch hutch && 
                (hutch.Name.Contains("MonsterHutchFramework") || hutch.Name.Contains("Winery")))
                {
                    foreach (var monster in hutch.characters)
                    {
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
        private void OnPlayerWarped(object? sender, WarpedEventArgs e)
        {
            foreach(var character in e.OldLocation.characters)
            {
                if (character is Monster monster &&
                    //(monster is ShadowBrute || monster is Shooter || monster is ShadowShaman || monster is ShadowGirl || monster is ShadowGuy) &&
                    monster.modData.ContainsKey($"{this.ModManifest.UniqueID}_monsterPetted"))
                {
                    monster.modData.Remove($"{this.ModManifest.UniqueID}_monsterPetted");
                }
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
                    //if (monster is ShadowBrute || monster is Shooter || monster is ShadowShaman || monster is ShadowGirl || monster is ShadowGuy)
                    {
                        SMonitor.Log($"left ring name: {Game1.player.leftRing.Name}, basename: {Game1.player.leftRing.Value.BaseName}, itemid: {Game1.player.leftRing.Value.ItemId}, name {Game1.player.leftRing.Value.Name}", LogLevel.Trace);
                        var playerTile = Game1.player.Tile;
                        var tileRect = new Microsoft.Xna.Framework.Rectangle((int)playerTile.X * 64, (int)playerTile.Y * 64, 64, 64);
                        var monsterPos = new Vector2(monster.Tile.X, monster.Tile.Y);
                        if (monster.GetBoundingBox().Intersects(tileRect) &&
                            //Math.Abs(monsterPos.X - playerTile.X) <= 1 && 
                            //Math.Abs(monsterPos.Y - playerTile.Y) <= 1 &&
                            RingPatches.MonsterIsCharmed(monster, Game1.player, out string? matchRingKey, out int matchMonsterIndex) &&
                            matchRingKey != null &&
                            !monster.modData.ContainsKey($"{this.ModManifest.UniqueID}_monsterPetted"))
                        {
                            GetCharmedSpeech(monster, matchRingKey, matchMonsterIndex, out string text, out Microsoft.Xna.Framework.Color? color, out int style, out int duration, out int preTimer);

                            monster.showTextAboveHead(text, color, style, duration > 0 ? duration : 1500, preTimer > -1 ? preTimer : 0);

                            if (AssetHandler.charmerRingData[matchRingKey].CharmedMonsters[matchMonsterIndex].Sound != null)
                            {
                                DelayedAction.playSoundAfterDelay(AssetHandler.charmerRingData[matchRingKey].CharmedMonsters[matchMonsterIndex].Sound, preTimer, Game1.player.currentLocation, monsterPos);
                                //monster.playNearbySoundAll(AssetHandler.charmerRingData[matchRingKey].CharmedMonsters[matchMonsterIndex].Sound);
                            }
                            //DelayedAction.textAboveHeadAfterDelay("<", monster, Game1.random.Next(600));
                            monster.modData.Add($"{this.ModManifest.UniqueID}_monsterPetted", "true");
                        }
                    }
                }
            }
        }
        public static void GetCharmedSpeech(Monster monster, string ringKey, int monsterIndex, out string text, out Microsoft.Xna.Framework.Color? color, out int style, out int duration, out int preTimer)
        {
            var speechList = new List<int>();
            var ringData = AssetHandler.charmerRingData;
            if (ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles.Count == 1)
            {
                text = ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[0].Text ?? "<";
                color = SpriteText.getColorFromIndex(ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[0].Color);
                style = ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[0].Style;
                duration = ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[0].Duration;
                preTimer = ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[0].Pretimer;
                return;
            }
            else if (ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles.Count > 1)
            {
                for (int i = 0; i < ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles.Count; i++)
                {
                    for (int j = 0; j < ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[i].Weight; j++)
                    {
                        speechList.Add(i);
                    }
                }
                var speechIndex = Game1.random.Next(speechList.Count);
                text = ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[speechList[speechIndex]].Text ?? "<";
                color = SpriteText.getColorFromIndex(ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[speechList[speechIndex]].Color);
                style = ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[speechList[speechIndex]].Style;
                duration = ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[speechList[speechIndex]].Duration;
                preTimer = ringData[ringKey].CharmedMonsters[monsterIndex].SpeechBubbles[speechList[speechIndex]].Pretimer;
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
