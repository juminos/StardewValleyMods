using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewModdingAPI;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley;
using Microsoft.Xna.Framework;
using FrenshipRings.Toolkit;
using FrenshipRings.Toolkit.Reflection;
using FrenshipRings.ReflectionManager;
using FrenshipRings.Framework;

using XLocation = xTile.Dimensions.Location;
using FrenshipRings.Toolkit.Extensions;

namespace FrenshipRings.Framework.Managers
{
    /// <summary>
    /// Manages a jump for a player.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:Field names should not contain underscore", Justification = "Preference.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1204:Static elements should appear before instance elements", Justification = "Preference.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1214:Readonly fields should appear before non-readonly fields", Justification = "Preference.")]
    internal sealed class JumpManager : IDisposable
    {
        private const int DEFAULT_TICKS = 200;

        private static bool hasSwim = false;

        // event handlers.
        private IGameLoopEvents gameEvents;
        private IDisplayEvents displayEvents;

        private bool disposedValue;
        private WeakReference<Farmer> farmerRef;
        private bool previousCollisionValue = false; // keeps track of whether or not the farmer had noclip on.
        private bool prevInvincibility; // we set invincibility because the farmer's sprite is not drawn anywhere
        private int prevInvincibilityTimer; // near the actual position, so this way the farmer can't appear to get hit out of nowhere.
        private readonly bool forceTimePass;

        private State state = State.Charging;
        private int ticks = DEFAULT_TICKS;
        private readonly Vector2 direction;
        private Keybind keybind;

        // charging fields.
        private int distance = 1;
        private readonly Vector2 startTile;
        private Vector2 currentTile = Vector2.Zero;
        private Vector2 openTile = Vector2.Zero;
        private bool isCurrentTileBlocked = false;
        private bool blocked = false;
        private NeedsBigJump needsBigJump = NeedsBigJump.False;

        // jumping fields.
        private JumpFrame frame;
        private float velocityX;

        #region delegates

        // this exists because if currentAnimationFrames is 1 or lower,
        // the game will unset PauseForSingleAnimation on its own.
        // so we just manually set it to two.
        private static readonly Lazy<Action<FarmerSprite, int>> currentFramesSetter = new(() =>
            typeof(FarmerSprite).GetCachedField("currentAnimationFrames", ReflectionCache.FlagTypes.InstanceFlags).GetInstanceFieldSetter<FarmerSprite, int>()
        );
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="JumpManager"/> class.
        /// </summary>
        /// <param name="farmer">The farmer we're tracking.</param>
        /// <param name="gameEvents">The game event manager.</param>
        /// <param name="displayEvents">The display event manager.</param>
        /// <param name="keybind">The keybind that triggered this jump.</param>
        internal JumpManager(Farmer farmer, IGameLoopEvents gameEvents, IDisplayEvents displayEvents, Keybind keybind)
        {
            ModEntry.SMonitor.Log("(FrogRing) Starting -> Charging");
            farmerRef = new(farmer);
            this.gameEvents = gameEvents;
            this.displayEvents = displayEvents;
            this.keybind = keybind;

            this.gameEvents.UpdateTicked += OnUpdateTicked;
            this.displayEvents.RenderedWorld += OnRenderedWorld;

            // save the values here too in case we get interrupted.
            previousCollisionValue = Game1.player.ignoreCollisions;
            prevInvincibility = Game1.player.temporarilyInvincible;
            prevInvincibilityTimer = Game1.player.temporaryInvincibilityTimer;

            // forcing time to pass even while we're preparing to jump.
            forceTimePass = Game1.player.forceTimePass;
            Game1.player.forceTimePass = true;

            direction = Game1.player.FacingDirection switch
            {
                Game1.up => -Vector2.UnitY,
                Game1.left => -Vector2.UnitX,
                Game1.down => Vector2.UnitY,
                _ => Vector2.UnitX,
            };

            startTile = openTile = farmer.Tile;
            RecalculateTiles(farmer, Game1.currentLocation);

            farmer.completelyStopAnimatingOrDoingAction();
            farmer.CanMove = false;
            SetCrouchAnimation(farmer);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="JumpManager"/> class.
        /// </summary>
        ~JumpManager() => Dispose(false);

        private enum State
        {
            Inactive,
            Charging,
            Jumping,
        }

        private enum JumpFrame
        {
            Start,
            Transition,
            Hold,
        }

        private enum NeedsBigJump
        {
            False,
            IfPastMedium,
            Medium,
            IfPastBig,
            Big,
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// sets some initial state.
        /// </summary>
        /// <param name="registry">Modregistry.</param>
        internal static void Initialize(IModRegistry registry)
            => hasSwim = registry.IsLoaded("aedenthorn.Swim");

        /// <summary>
        /// Checks to see if this JumpManager is valid (ie, not disposed, and has an active farmer associated).
        /// </summary>
        /// <param name="farmer">The relevant farmer.</param>
        /// <returns>True if valid.</returns>
        internal bool IsValid([NotNullWhen(true)] out Farmer? farmer)
        {
            farmer = null;
            return !disposedValue && state != State.Inactive
                        && farmerRef?.TryGetTarget(out farmer) == true && farmer is not null;
        }

        [MethodImpl(TKConstants.Hot)]
        private bool IsCurrentFarmer()
            => farmerRef?.TryGetTarget(out Farmer? farmer) == true && ReferenceEquals(farmer, Game1.player);

        [MethodImpl(TKConstants.Hot)]
        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if (!IsCurrentFarmer())
            {
                return;
            }

            e.SpriteBatch.Draw(
                texture: Game1.mouseCursors,
                new Vector2(openTile.X * Game1.tileSize - Game1.viewport.X, openTile.Y * Game1.tileSize - Game1.viewport.Y),
                new Rectangle(194, 388, 16, 16),
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                scale: 4f,
                effects: SpriteEffects.None,
                layerDepth: 0.01f);

            if (isCurrentTileBlocked && state == State.Charging)
            {
                e.SpriteBatch.Draw(
                    texture: Game1.mouseCursors,
                    new Vector2(currentTile.X * Game1.tileSize - Game1.viewport.X, currentTile.Y * Game1.tileSize - Game1.viewport.Y),
                    new Rectangle(210, 388, 16, 16),
                    color: Color.White,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    scale: 4f,
                    effects: SpriteEffects.None,
                    layerDepth: 0.01f);
            }
        }

        [MethodImpl(TKConstants.Hot)]
        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!IsCurrentFarmer() || !Game1.game1.IsActive)
            {
                return;
            }

            switch (state)
            {
                case State.Charging:
                    if (keybind.GetState().IsDown())
                    {
                        ticks -= ModEntry.Config.JumpChargeSpeed;
                        if (ticks <= 0)
                        {
                            if (distance < ModEntry.Config.MaxFrogJumpDistance && !blocked)
                            {
                                ++distance;
                                CRUtils.PlayChargeCue(distance);
                                RecalculateTiles(Game1.player, Game1.currentLocation);
                            }
                            ticks = DEFAULT_TICKS;
                            ModEntry.SMonitor.Log($"(Frog Ring) distance: {distance}");
                        }
                    }
                    else if (startTile == openTile)
                    {
                        ModEntry.SMonitor.Log($"(Frog Ring) Switching Charging -> Invalid", LogLevel.Info);
                        Game1.player.synchronizedJump(3f); // a tiny little hop
                        state = State.Inactive;
                        Dispose();
                    }
                    else
                    {
                        ModEntry.SMonitor.Log($"(Frog Ring) Switching Charging -> Jumping", LogLevel.Info);
                        state = State.Jumping;

                        CRUtils.PlayMeep();

                        #region velocity calculations

                        float initialVelocityY = Math.Max(6f, 4f * MathF.Sqrt(distance));

                        // we're just gonna hope the big jump is enough to clear most buildings :(
                        if (needsBigJump == NeedsBigJump.Medium)
                        {
                            initialVelocityY = Math.Max(12f, initialVelocityY);
                        }
                        else if (needsBigJump == NeedsBigJump.Big)
                        {
                            initialVelocityY = Math.Max(16f, initialVelocityY);
                        }

                        // okay, let's adjust for possible front tiles if needed
                        Vector2? tileToCheck = null;
                        if (direction == Vector2.UnitY)
                        {
                            tileToCheck = startTile;
                        }
                        else if (direction == -Vector2.UnitY)
                        {
                            tileToCheck = openTile;
                        }

                        if (tileToCheck is not null)
                        {
                            int verticalHeightNeeded = 4;
                            int startX = (int)tileToCheck.Value.X * Game1.tileSize;
                            int startY = (int)(tileToCheck.Value.Y - 3) * Game1.tileSize;
                            while (startY > 0 && HasFrontOrAlwaysFrontTile(startX, startY))
                            {
                                ++verticalHeightNeeded;
                                startY -= 64;
                            }

                            ModEntry.SMonitor.Log($"Additional vertical height: {verticalHeightNeeded}", LogLevel.Debug);

                            if (verticalHeightNeeded > 4)
                            {
                                initialVelocityY = Math.Max(initialVelocityY, 6 * MathF.Sqrt(verticalHeightNeeded));
                            }
                        }

                        // a little sanity here.
                        initialVelocityY = Math.Min(initialVelocityY, 128f);

                        float tileTravelDistance = openTile.ManhattanDistance(startTile);
                        if (ModEntry.Config.JumpCostsStamina && Game1.CurrentEvent is null)
                        {
                            Game1.player.Stamina -= tileTravelDistance;
                        }
                        float travelDistance = tileTravelDistance * Game1.tileSize;

                        // gravity is 0.5f, so total time is 2 * initialVelocityY / 0.5 = 4 * initialVelocityY;
                        velocityX = travelDistance / (4 * initialVelocityY - 1);
                        Game1.player.synchronizedJump(initialVelocityY);

                        #endregion

                        // track player state
                        previousCollisionValue = Game1.player.ignoreCollisions;
                        Game1.player.ignoreCollisions = true;

                        prevInvincibility = Game1.player.temporarilyInvincible;
                        prevInvincibilityTimer = Game1.player.temporaryInvincibilityTimer;
                        Game1.player.temporarilyInvincible = true;
                        Game1.player.temporaryInvincibilityTimer = int.MinValue;

                        StartJumpAnimation(Game1.player);
                    }
                    break;
                case State.Jumping:
                    if (Game1.player.yJumpOffset == 0 && Game1.player.yJumpVelocity.WithinMargin(0f))
                    {
                        ModEntry.SMonitor.Log($"(Frog Ring) Switching Jumping -> Inactive", LogLevel.Info);
                        state = State.Inactive;
                        Dispose();
                    }
                    else
                    {
                        Game1.player.Position += velocityX * direction;
                        // Handle switching the jump frame.
                        switch (frame)
                        {
                            case JumpFrame.Start:
                                {
                                    if (Game1.player.yJumpOffset < -20)
                                    {
                                        ModEntry.SMonitor.Log("(Frog Ring) Setting Jump Frame: START -> TRANSITION");
                                        SetTransitionAnimation(Game1.player);
                                        frame = JumpFrame.Transition;
                                    }
                                    break;
                                }
                            case JumpFrame.Transition:
                                {
                                    if (Game1.player.yJumpVelocity < 0)
                                    {
                                        ModEntry.SMonitor.Log("(Frog Ring) Setting Jump Frame: TRANSITION -> HOLD");
                                        HoldJumpAnimation(Game1.player);
                                        frame = JumpFrame.Hold;
                                    }
                                    break;
                                }
                        }
                    }
                    break;
            }
        }

        private static bool HasFrontOrAlwaysFrontTile(int x, int y)
            => Game1.currentLocation.map.GetLayer("Front")?.PickTile(new XLocation(x * Game1.tileSize, y * Game1.tileSize), Game1.viewport.Size) is not null
                || Game1.currentLocation.map.GetLayer("AlwaysFront")?.PickTile(new XLocation(x * Game1.tileSize, y * Game1.tileSize), Game1.viewport.Size) is not null;

        private void RecalculateTiles(Farmer farmer, GameLocation? location)
        {
            if (location is null)
            {
                return;
            }
            currentTile = startTile + direction * distance;
            bool isValidTile = location.isTileOnMap(currentTile)
                && location.isTilePassable(new XLocation((int)currentTile.X, (int)currentTile.Y), Game1.viewport)
                && !location.isWaterTile((int)currentTile.X, (int)currentTile.Y);

            // let the user jump into water if they have swim mod. But not lava.
            if (hasSwim && !isValidTile && location is not VolcanoDungeon or Caldera)
            {
                isValidTile = location.isOpenWater((int)currentTile.X, (int)currentTile.Y);
            }

            if (isValidTile)
            {
                Rectangle box = farmer.GetBoundingBox();
                box.X += (int)direction.X * distance * Game1.tileSize;
                box.Y += (int)direction.Y * distance * Game1.tileSize;
                isValidTile &= !location.isCollidingPosition(box, Game1.viewport, true, 0, false, farmer);
            }

            // check a tile property here, so mapmakers can block the jump.
            if (location.doesEitherTileOrTileIndexPropertyEqual((int)currentTile.X, (int)currentTile.Y, "atravita.FrogJump", "Back", "Forbidden"))
            {
                blocked = true;
                isValidTile = false;
                Game1.showRedMessage(I18n.FrogRing_Blocked());
            }

            if (needsBigJump != NeedsBigJump.IfPastBig && needsBigJump != NeedsBigJump.Big)
            {
                if (location.terrainFeatures.TryGetValue(currentTile, out TerrainFeature? feat)
                    && feat is Tree or FruitTree)
                {
                    needsBigJump = NeedsBigJump.IfPastBig;
                }
                else if (HasFrontOrAlwaysFrontTile((int)currentTile.X, (int)currentTile.Y))
                {
                    needsBigJump = NeedsBigJump.IfPastMedium;
                }
                else if (location is Farm farm)
                {
                    foreach (Building? building in farm.buildings)
                    {
                        if (building is not ShippingBin or FishPond && building.occupiesTile(currentTile))
                        {
                            needsBigJump = NeedsBigJump.IfPastBig;
                            break;
                        }
                    }
                }
            }

            if (isValidTile)
            {
                openTile = currentTile;
                isCurrentTileBlocked = false;
                if (needsBigJump == NeedsBigJump.IfPastBig)
                {
                    needsBigJump = NeedsBigJump.Big;
                }
                else if (needsBigJump == NeedsBigJump.IfPastBig)
                {
                    needsBigJump = NeedsBigJump.Medium;
                };
            }
            else
            {
                isCurrentTileBlocked = true;
            }
        }

        #region cleanup

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Unhook();
                if (farmerRef?.TryGetTarget(out Farmer? farmer) == true && farmer is not null)
                {
                    farmer.CanMove = true;
                    farmer.ignoreCollisions = previousCollisionValue;
                    farmer.temporarilyInvincible = prevInvincibility;
                    farmer.temporaryInvincibilityTimer = prevInvincibilityTimer;
                    farmer.forceTimePass = forceTimePass;
                    if (disposing)
                    {
                        // Game1.moveViewportTo(farmer.Position, 5f, 60, null, ModEntry.cameraAPI is null ? null : ModEntry.cameraAPI.Reset);
                    }
                    farmer.completelyStopAnimatingOrDoingAction();
                }
                keybind = null!;
                farmerRef = null!;
                gameEvents = null!;
                displayEvents = null!;
                disposedValue = true;
            }
        }

        private void Unhook()
        {
            if (gameEvents is not null)
            {
                gameEvents.UpdateTicked -= OnUpdateTicked;
            }
            if (displayEvents is not null)
            {
                displayEvents.RenderedWorld -= OnRenderedWorld;
            }
        }

        #endregion

        #region animationFrames

        /***************************************************************************
         * Animations here are mostly from the watering and hoe-ing animations
         * and are made basically by inspecting FarmerSprite.getAnimationFromIndex
         * and Tool.endUsing.
         ***************************************************************************/

        private static void SetCrouchAnimation(Farmer farmer)
        {
            farmer.FarmerSprite.setCurrentSingleFrame(
                which: farmer.FacingDirection switch
                {
                    Game1.down => 54,
                    Game1.right => 58,
                    Game1.up => 62,
                    _ => 58,
                }, flip: farmer.FacingDirection == Game1.left,
                interval: 2000);
            farmer.FarmerSprite.PauseForSingleAnimation = true;
            farmer.FarmerSprite.timer = 0f;
            currentFramesSetter.Value(farmer.FarmerSprite, 2);
        }

        private static void StartJumpAnimation(Farmer farmer)
        {
            farmer.FarmerSprite.setCurrentSingleFrame(
                which: farmer.FacingDirection switch
                {
                    Game1.down => 55,
                    Game1.right => 59,
                    Game1.up => 63,
                    _ => 59,
                }, flip: farmer.FacingDirection == Game1.left);
            farmer.FarmerSprite.PauseForSingleAnimation = true;
            currentFramesSetter.Value(farmer.FarmerSprite, 2);
        }

        private static void SetTransitionAnimation(Farmer farmer)
        {
            farmer.FarmerSprite.setCurrentSingleFrame(
                which: farmer.FacingDirection switch
                {
                    Game1.down => 25,
                    Game1.right => 45,
                    Game1.up => 46,
                    _ => 45,
                }, flip: farmer.FacingDirection == Game1.left,
                secondaryArm: true);
            farmer.FarmerSprite.PauseForSingleAnimation = true;
            currentFramesSetter.Value(farmer.FarmerSprite, 2);
        }

        private static void HoldJumpAnimation(Farmer farmer)
        {
            farmer.FarmerSprite.setCurrentSingleFrame(
            which: farmer.FacingDirection switch
            {
                Game1.down => 70,
                Game1.right => 52,
                Game1.up => 62,
                _ => 52,
            },
            flip: farmer.FacingDirection == Game1.left);
            farmer.FarmerSprite.PauseForSingleAnimation = true;
            currentFramesSetter.Value(farmer.FarmerSprite, 2);
        }

        #endregion
    }
}
