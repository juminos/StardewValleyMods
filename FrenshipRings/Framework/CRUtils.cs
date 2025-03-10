﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrenshipRings.ReflectionManager;
using StardewModdingAPI;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley;
using FrenshipRings.Toolkit.Reflection;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using FrenshipRings.Utilities;
using StardewValley.Characters;

namespace FrenshipRings.Framework;

/// <summary>
/// A utility class for this mod.
/// </summary>
internal static class CRUtils
{
    #region delegates

    private static readonly Lazy<Action<Rabbit, int>> CharacterTimerSetter = new(() =>
        typeof(Rabbit).GetCachedField("characterCheckTimer", ReflectionCache.FlagTypes.InstanceFlags)
        .GetInstanceFieldSetter<Rabbit, int>()
    );

    private static readonly Lazy<Action<Frog, int>> FrogTimerSetter = new(() =>
        typeof(Frog).GetCachedField("beforeFadeTimer", ReflectionCache.FlagTypes.InstanceFlags)
        .GetInstanceFieldSetter<Frog, int>()
    );

    #endregion

    /// <summary>
    /// Plays the sound associated with charging up.
    /// </summary>
    /// <param name="charge">The charge amount.</param>
    internal static void PlayChargeCue(int charge)
    {
        if (ModEntry.Config.PlayAudioEffects && Game1.soundBank is not null)
        {
            try
            {
                ICue cue = Game1.soundBank.GetCue("toolCharge");
                cue.SetVariable("Pitch", (Game1.random.Next(12, 16) + charge) * 100);
                cue.Play();
            }
            catch (Exception ex)
            {
                ModEntry.Config.PlayAudioEffects = false;
                ModEntry.SMonitor.Log($"Failed while trying to play charge-up cue!\n\n{ex}", LogLevel.Error);
            }
        }
    }

    /// <summary>
    /// Plays a little meep.
    /// </summary>
    internal static void PlayMeep()
    {
        if (ModEntry.Config.PlayAudioEffects && Game1.soundBank is not null)
        {
            try
            {
                Game1.playSound("dustMeep");
            }
            catch (Exception ex)
            {
                ModEntry.Config.PlayAudioEffects = false;
                ModEntry.SMonitor.Log($"Failed while trying to play hopping noise.\n\n{ex}", LogLevel.Error);
            }
        }
    }

    /// <summary>
    /// Checks to make sure it's safe to spawn butterflies.
    /// </summary>
    /// <param name="loc">Game location to check.</param>
    /// <returns>True if we should spawn butterflies, false otherwise.</returns>
    internal static bool ShouldSpawnButterflies([NotNullWhen(true)] this GameLocation? loc)
        => loc is not null && !Game1.isDarkOut(loc)
            && (ModEntry.Config.ButterfliesSpawnInRain || !loc.IsOutdoors || !Game1.IsRainingHere(loc));

    /// <summary>
    /// Checks to make sure it's safe to spawn owls.
    /// </summary>
    /// <param name="loc">Game location to check.</param>
    /// <returns>True if okay to spawn owls.</returns>
    internal static bool ShouldSpawnOwls([NotNullWhen(true)] this GameLocation? loc)
        => loc is not null && (Game1.isDarkOut(loc) || ModEntry.Config.OwlsSpawnDuringDay)
            && (ModEntry.Config.OwlsSpawnIndoors || loc.IsOutdoors);

    /// <summary>
    /// Checks to make sure it's safe to spawn frogs.
    /// </summary>
    /// <param name="loc">GameLocation to check.</param>
    /// <returns>True if this is a good place to spawn frogs.</returns>
    internal static bool ShouldSpawnFrogs([NotNullWhen(true)] this GameLocation? loc)
    {
        if (loc is null)
        {
            return false;
        }

        if (ModEntry.Config.FrogsSpawnOnlyInRain && !Game1.IsRainingHere(loc) && loc.IsOutdoors)
        {
            return false;
        }

        if (!ModEntry.Config.IndoorFrogs && !loc.IsOutdoors)
        {
            return false;
        }

        if (!ModEntry.Config.FrogsSpawnInHeat && loc is Desert or VolcanoDungeon or Caldera)
        {
            return false;
        }

        MineShaft? shaft = loc as MineShaft;

        if (!ModEntry.Config.FrogsSpawnInHeat && shaft?.getMineArea() == 80)
        {
            return false;
        }

        if (!ModEntry.Config.FrogsSpawnInCold && ((Game1.GetSeasonForLocation(loc) == Season.Winter && loc is not Desert or IslandLocation) || shaft?.getMineArea() == 40))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Spawns fireflies around the player.
    /// </summary>
    /// <param name="critters">Critters list to add to.</param>
    /// <param name="count">Number of fireflies to spawn.</param>
    internal static void SpawnFirefly(List<Critter>? critters, int count)
    {
        if (critters is not null && count > 0)
        {
            count *= ModEntry.Config.CritterSpawnMultiplier;
            for (int i = 0; i < count; i++)
            {
                critters.Add(new Firefly(Game1.player.Tile));
            }
        }
    }

    /// <summary>
    /// Spawns butterflies around the player.
    /// </summary>
    /// <param name="critters">Critters list to add to.</param>
    /// <param name="count">Number of butterflies to spawn.</param>
    internal static void SpawnButterfly(List<Critter>? critters, int count)
    {
        ModEntry.SMonitor.Log($"Attempting to spawn {count} butterflies", LogLevel.Trace);
        if (critters is not null && count > 0)
        {
            count *= ModEntry.Config.CritterSpawnMultiplier;
            for (int i = 0; i < count; i++)
            {
                critters.Add(new Butterfly(Game1.currentLocation, Game1.player.Tile, Game1.random.Next(2) == 0).setStayInbounds(true));
            }
        }
    }

    /// <summary>
    /// Spawns frogs around the player.
    /// </summary>
    /// <param name="loc">The game location.</param>
    /// <param name="critters">Critters list to add to.</param>
    /// <param name="count">Number of frogs to spawn.</param>
    internal static void SpawnFrogs(GameLocation loc, List<Critter> critters, int count)
    {
        if (critters is not null && count > 0)
        {
            count *= ModEntry.Config.CritterSpawnMultiplier * 2;
            for (int i = 0; i < count; i++)
            {
                Frog? frog = null;

                // try for a frog that leaps into water.
                if (loc.waterTiles is not null && Game1.random.Next(2) == 0)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Vector2 tile = loc.getRandomTile();
                        int xCoord = (int)tile.X;
                        int yCoord = (int)tile.Y;
                        if (!loc.isWaterTile(xCoord, yCoord) || !loc.isWaterTile(xCoord, yCoord - 1)
                            || loc.doesTileHaveProperty(xCoord, yCoord, "Passable", "Buildings") is not null
                            || ((loc is Beach || loc.catchOceanCrabPotFishFromThisSpot(xCoord, yCoord)) && !ModEntry.Config.SaltwaterFrogs))
                        {
                            continue;
                        }

                        bool flipped = Game1.random.Next(2) == 0;
                        for (int x = 1; x < 11; x++)
                        {
                            if (!loc.isTileOnMap(xCoord + x, yCoord))
                            {
                                goto breakbreak;
                            }

                            if (loc.isWaterTile(xCoord + x, yCoord))
                            {
                                frog = new(new Vector2(tile.X + x, tile.Y), true, flipped);
                                goto breakbreak;
                            }
                        }
                    }
                }
            breakbreak:
                frog ??= new(Game1.player.Tile);
                FrogTimerSetter.Value(frog, Game1.random.Next(2000, 5000));
                critters.Add(frog);
            }
        }
    }

    /// <summary>
    /// Spawns owls.
    /// </summary>
    /// <param name="loc">The game location.</param>
    /// <param name="critters">Critters list to add to.</param>
    /// <param name="count">Number of owls to spawn.</param>
    internal static void SpawnOwls(GameLocation loc, List<Critter> critters, int count)
    {
        if (critters is not null && count > 0)
        {
            count *= ModEntry.Config.CritterSpawnMultiplier;
            for (int i = 0; i < count; i++)
            {
                Vector2 owlPos;

                if (Game1.random.Next(3) == 0)
                {
                    Vector2 pos = Game1.player.Position;
                    float deltaY = pos.Y + 128;
                    owlPos = new Vector2(
                    x: Math.Clamp(pos.X - (deltaY / 4), 0, (loc.Map.Layers[0].LayerWidth - 1) * Game1.tileSize) + Game1.random.Next(-256, 128),
                    y: -128);
                }
                else
                {
                    owlPos = new Vector2(
                    x: Game1.random.Next(0, (loc.Map.Layers[0].LayerWidth - 1) * Game1.tileSize),
                    y: -128);
                }
                Owl owl = new(owlPos);
                DelayedAction.functionAfterDelay(
                    func: () =>
                    {
                        critters.Add(owl);
                    },
                    delay: (i * 150) + Game1.random.Next(-50, 150));
            }
        }
    }


    /// <summary>
    /// Add jumino at player position.
    /// </summary>
    /// <param name="loc">The current player location.</param>
    /// <param name="count">Number of junimos to spawn.</param>
    internal static void SpawnJunimo(GameLocation loc, int count)
    {
        Vector2 playerPos = Game1.player.Position;
        ModEntry.SMonitor.Log($"Attempting to spawn junimos at {playerPos}", LogLevel.Trace);

        for (int i = 0; i < count; i++)
        {
            var junimo = new Junimo(playerPos, 1, true);
            junimo.friendly.Value = true;
            junimo.temporaryJunimo.Value = false;
            junimo.currentLocation = loc;
            junimo.Position = playerPos;
            junimo.speed = Game1.player.speed + 1;
            junimo.modData["RingJunimo"] = Game1.player.UniqueMultiplayerID.ToString();
            loc.characters.Add(junimo);
        }
    }
    /// <summary>
    /// Remove junimos.
    /// </summary>
    /// <param name="loc">The current player location.</param>
    internal static void RemoveJunimos(GameLocation loc)
    {
        foreach (var character in Utility.getAllCharacters())
        {
            if (character is not Junimo)
            {
                continue;
            }
            if (character.modData.ContainsKey("RingJunimo") && character.modData["RingJunimo"] == Game1.player.UniqueMultiplayerID.ToString())
            {
                loc.characters.Remove(character);
                ModEntry.SMonitor.Log($"Attempting to remove junimos.", LogLevel.Trace);
            }
        }
    }
    /// <summary>
    /// Warp junimos to player on location change.
    /// </summary>
    /// <param name="oldLoc">The player location before warp.</param>
    /// <param name="newLoc">The player location after warp.</param>
    internal static void WarpJunimos(GameLocation oldLoc, GameLocation newLoc)
    {
        Vector2 playerPos = Game1.player.Position;
        foreach (var character in Utility.getAllCharacters())
        {
            if (character is not Junimo)
            {
                continue;
            }
            if (character.modData.ContainsKey("RingJunimo"))
            {
                Game1.warpCharacter(character, newLoc, playerPos);
                ModEntry.SMonitor.Log($"Attempting to warp junimos to {playerPos}", LogLevel.Trace);
            }
        }
    }


    /// <summary>
    /// Add bunnies to a location.
    /// </summary>
    /// <param name="critters">The critter list.</param>
    /// <param name="count">The number of bunnies to spawn.</param>
    /// <param name="bushes">The bushes on the map, for the bunnies to run towards.</param>
    internal static void AddBunnies(List<Critter> critters, int count, List<Bush>? bushes)
    {
        if (critters is not null && count > 0)
        {
            int delay = 0;
            foreach ((Vector2 position, bool flipped) in FindBunnySpawnTile(
                loc: Game1.currentLocation,
                bushes: bushes,
                playerTile: Game1.player.Tile,
                count: count * 2))
            {
                GameLocation location = Game1.currentLocation;
                DelayedAction.functionAfterDelay(
                func: () =>
                {
                    if (location == Game1.currentLocation)
                    {
                        SpawnRabbit(critters, position, location, flipped);
                    }
                },
                delay: delay += Game1.random.Next(250, 750));
            }
        }
    }

    private static IEnumerable<(Vector2, bool)> FindBunnySpawnTile(GameLocation loc, List<Bush>? bushes, Vector2 playerTile, int count)
    {
        if (count <= 0 || bushes?.Count is null or 0)
        {
            yield break;
        }

        Utility.Shuffle(Game1.random, bushes);

        count *= ModEntry.Config.CritterSpawnMultiplier;
        foreach (Bush bush in bushes)
        {
            if (count <= 0)
            {
                yield break;
            }

            if (Vector2.DistanceSquared(bush.Tile, playerTile) <= 225)
            {
                if (bush.size.Value == Bush.walnutBush && bush.tileSheetOffset.Value == 1)
                {
                    // this is a walnut bush. Turns out bunnies can collect those.
                    continue;
                }

                bool flipped = Game1.random.Next(2) == 0;
                Vector2 startTile = bush.Tile;
                startTile.X += flipped ? 2 : -2;
                int distance = Game1.random.Next(5, 12);

                for (int i = distance; i > 0; i--)
                {
                    Vector2 tile = startTile;
                    startTile.X += flipped ? 1 : -1;
                    if (!bush.getBoundingBox().Intersects(new Rectangle((int)startTile.X * 64, (int)startTile.Y * 64, 64, 64))
                        && (!loc.isTileLocationOpen(startTile) || loc.isWaterTile((int)startTile.X, (int)startTile.Y)))
                    {
                        if (distance > 3)
                        {
                            yield return (tile, flipped);
                            count--;
                        }
                        goto Continue;
                    }
                }
                yield return (startTile, flipped);
                count--;
            Continue:;
            }
        }
    }

    private static void SpawnRabbit(List<Critter>? critters, Vector2 tile, GameLocation loc, bool flipped)
    {
        if (critters is not null)
        {
            Rabbit rabbit = new(Game1.currentLocation, tile, flipped);
            // make the rabbit hang around for a little longer.
            // so it doesn't immediately exist stage left.
            CharacterTimerSetter.Value(rabbit, Game1.random.Next(750, 1500));
            critters.Add(rabbit);

            // little TAS to hide the pop in.
            TemporaryAnimatedSprite? tas = new(
                textureName: Game1.mouseCursorsName,
                sourceRect: new Rectangle(464, 1792, 16, 16),
                animationInterval: 120f,
                animationLength: 5,
                numberOfLoops: 0,
                position: (tile - Vector2.One) * Game1.tileSize,
                flicker: false,
                flipped: Game1.random.NextDouble() < 0.5,
                layerDepth: 1f,
                alphaFade: 0.01f,
                color: Color.White,
                scale: Game1.pixelZoom,
                scaleChange: 0.01f,
                rotation: 0f,
                rotationChange: 0f)
            {
                lightId = "FrenshipRings_RabbitTAS",
            };
            MultiplayerHelpers.GetMultiplayer().broadcastSprites(loc, tas);
        }
    }
}