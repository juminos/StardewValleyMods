using System.Data;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;

namespace MonstersTheFramework
{
    [XmlType("Mods_spacechase0_MonstersTheFramework_CustomMonster")]
    public class CustomMonster : Monster
    {
        public readonly NetString monsterKey = new();
        public readonly NetString currentState = new();
        public readonly NetBool pendingStart = new(true);
        public readonly NetInt stateTimer = new();
        public readonly NetStringDictionary<float, NetFloat> vars = new();

        public MonsterType Type
        {
            get
            {
                var dict = Game1.content.Load<Dictionary<string, MonsterType>>("spacechase0.MonstersTheFramework/Monsters");
                if (!dict.ContainsKey(monsterKey.Value))
                    return null;
                return dict[monsterKey.Value];
            }
        }

        public MonsterType.StateData CurrentState
        {
            get
            {
                var type = Type;
                if (type == null)
                    return null;
                if (!type.States.ContainsKey(currentState.Value))
                    currentState.Value = type.StartingState;
                var state = (MonsterType.StateData)type.States[currentState.Value].Clone();
                if (!string.IsNullOrEmpty(state.InheritsFrom))
                    state = state.InheritFrom(type.States[state.InheritsFrom], type.States);

                Mod.SMonitor.Log($"{Type.Name} current state is {currentState.Value}", LogLevel.Trace);

                return state;
            }
        }

        public CustomMonster() { }

        public CustomMonster(string key)
        {
            monsterKey.Value = key;
            maxHealth.Value = health.Value = Type.MaxHealth;
            onCollision = this.onCollide;
            displayName = Type.Name;
            Name = Type.CorrespondingMonsterGoal;
            UpdateState(Type.StartingState);
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddField(monsterKey);
            NetFields.AddField(currentState);
            NetFields.AddField(pendingStart);
            NetFields.AddField(stateTimer);
        }

        public override Rectangle GetBoundingBox()
        {
            var state = CurrentState;
            return new Rectangle((int)Position.X, (int)Position.Y, (int)state.BoundingBoxSize.Value.X, (int)state.BoundingBoxSize.Value.Y);
        }

        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
            Mod.SMonitor.Log($"custom monster taking damage", LogLevel.Trace);

            var state = CurrentState;
            if (isBomb && state.CanReceiveDamageFromBomb == false)
                return -1;
            else if (state.CanReceiveDamageFromPlayer.ToLower() != "true" && !who.CurrentTool.HasEnchantment(state.CanReceiveDamageFromPlayer))
                return -1;

            damage = (int)(damage * state.IncomingDamageMultiplier);

            int dmg = base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, state.HitSound);
            TriggerEvent(isBomb ? "OnHurtByBomb" : "OnHurtByPlayer");
            if (health.Value <= 0)
                TriggerEvent("OnDeath");
            return dmg;
        }

        public override List<Item> getExtraDropItems()
        {
            List<Item> ret = new();

            foreach (var drop in Type.Drops)
            {
                var chosenDrop = drop.Choose();
                if (chosenDrop != null)
                {
                    Item actualDrop = null;
                    actualDrop = new StardewValley.Object(chosenDrop.Drop, chosenDrop.Quantity);

                    ret.Add(actualDrop);

                }
            }

            ret.AddRange(base.getExtraDropItems());
            return ret;
        }

        public override void onDealContactDamage(Farmer who)
        {
            TriggerEvent("OnHitPlayer");

            string debuff = CurrentState.ApplyDebuff;
            if (debuff == null)
                return;

            //if ( int.TryParse( debuff, out int debuffId ) )
            {
                if (who == Game1.player)
                    Game1.player.applyBuff(debuff);
            }
        }

        public override void shedChunks(int number, float scale)
        {
            var state = CurrentState;
            Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle((int)state.Animation.DeathChunkStartCoordinates.X, (int)state.Animation.DeathChunkStartCoordinates.Y, 16, 16), 8, this.GetBoundingBox().Center.X, this.GetBoundingBox().Center.Y, number, (int)base.Tile.Y, Color.White, 4f * scale);
        }

        public override void update(GameTime time, GameLocation location)
        {
            base.update(time, location);
            if (pendingStart.Value)
            {
                pendingStart.Value = false;
                TriggerEvent("OnStart");
            }
            stateTimer.Value++;

            var state = CurrentState;
            if (this.Sprite.Animate(time, state.Animation.StartingIndex, state.Animation.AnimationLength, state.Animation.TicksPerFrame / 60f * 1000))
            {
                Sprite.endOfAnimationFunction?.Invoke(null);
            }

            TriggerEvent("OnTick");
        }

        public override void updateMovement(GameLocation location, GameTime time)
        {
            Mod.SMonitor.Log($"custom monster updating movement", LogLevel.Trace);

            var state = CurrentState;
            if (state.Movement == null)
            {
                Mod.SMonitor.Log($"movement is null", LogLevel.Trace);

                return;
            }

            switch (state.Movement.Direction)
            {
                case MonsterType.StateData.MovementData.DirectionType.Fixed:
                    var dir = state.Movement.FixedDirection;
                    dir.Normalize();

                    switch (state.Movement.Style)
                    {
                        case MonsterType.StateData.MovementData.StyleType.Constant:
                            xVelocity = dir.X * state.Movement.MovementSpeed;
                            yVelocity = dir.Y * state.Movement.MovementSpeed;

                            break;

                        case MonsterType.StateData.MovementData.StyleType.Dash:
                            if (xVelocity != 0 || yVelocity != 0)
                            {
                                if (xVelocity != 0)
                                    xVelocity = Utility.MoveTowards(xVelocity, 0, state.Movement.DashFriction);
                                if (yVelocity != 0)
                                    yVelocity = Utility.MoveTowards(yVelocity, 0, state.Movement.DashFriction);
                            }
                            else
                            {
                                xVelocity = dir.X * state.Movement.MovementSpeed;
                                yVelocity = dir.Y * state.Movement.MovementSpeed;
                            }
                            break;
                    }
                    break;
                case MonsterType.StateData.MovementData.DirectionType.TowardPlayer:
                    break;
                case MonsterType.StateData.MovementData.DirectionType.Pathfind:
                    IsWalkingTowardPlayer = true;
                    moveTowardPlayerThreshold.Value = -1;
                    break;
            }

            var bb = GetBoundingBox();
            bb.Offset(xVelocity, yVelocity);
            if (location.isCollidingPosition(bb, Game1.viewport, false, 0, isGlider.Value, this))
            {
                xVelocity = yVelocity = 0;
                TriggerEvent("OnCollision");
            }
            else
            {
                position.X += xVelocity;
                position.Y += yVelocity;
            }
        }

        public override void draw(SpriteBatch b)
        {
            var state = CurrentState;
            this.Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, Position - state.Animation.Origin * Game1.pixelZoom), (float)this.GetBoundingBox().Center.Y / 10000f);
            //b.Draw( Game1.staminaRect, Game1.GlobalToLocal( Game1.viewport, GetBoundingBox() ), Color.Red );
        }

        private void UpdateState(string newState)
        {
            Mod.SMonitor.Log($"current state is {currentState.Value}, attempt setting to {newState}", LogLevel.Trace);

            currentState.Value = newState;
            var state = CurrentState;

            Mod.SMonitor.Log($"updated state is {currentState.Value}", LogLevel.Trace);

            this.resilience.Value = state.Defense ?? 0;
            this.isGlider.Value = state.IsGlider ?? false;
            this.DamageToFarmer = state.ContactDamage ?? 0; // todo - damagelement

            pendingStart.Value = true;

            stateTimer.Value = 0;

            if (state.Animation != null)
            {
                // juminos-changed loading textures can be done in CP
                //var texture = Mod.instance.Helper.ModContent.Load<Texture2D>(state.Animation.SpriteSheet);
                //var texturePath = Mod.instance.Helper.ModContent.GetInternalAssetName(state.Animation.SpriteSheet).BaseName;
                var texturePath = state.Animation.SpriteSheet;

                Mod.SMonitor.Log($"updating spritesheet to {state.Animation.SpriteSheet}", LogLevel.Trace);

                Sprite = new AnimatedSprite(texturePath, state.Animation.StartingIndex, (int)state.Animation.FrameSize.X, (int)state.Animation.FrameSize.Y);
                Sprite.interval = state.Animation.TicksPerFrame / 60f;
                Sprite.loop = state.Animation.Loops;
                Sprite.endOfAnimationFunction = (Farmer confusing_parameter) => TriggerEvent("OnAnimationEnd");
            }
        }

        private void onCollide(GameLocation loc)
        {
            TriggerEvent("OnCollision");
        }

        private void TriggerEvent(string evtName)
        {
            //if ( evtName != "OnTick" )
            ;// Log.Debug( "triggering event " + evtName );

            Mod.SMonitor.Log($"{evtName} event triggered", LogLevel.Trace);

            var state = CurrentState;

            Mod.SMonitor.Log($"current state is {currentState.Value}", LogLevel.Trace);

            using DataTable dt = new();

            for (int i = 0; i < state.Events.Count; ++i)
            {
                var evt = state.Events[i];
                if (evt.Event != evtName)
                    continue;

                if (evt.When == null || (bool)dt.Compute(DoConditionReplacements(evt.When), string.Empty))
                {
                    Mod.SMonitor.Log($"found state match {evt.Event} with {evt.Actions.Count} actions", LogLevel.Trace);

                    string oldState = currentState.Value;
                    foreach (var action in evt.Actions)
                    {
                        Mod.SMonitor.Log($"doing action {action.Key}, {action.Value.ToString()}", LogLevel.Trace);

                        DoAction(action.Key, action.Value.ToString());
                    }
                    if (currentState.Value != oldState)
                        break;
                }
            }
        }

        private void DoAction(string action, string args)
        {
            switch (action.ToLower())
            {
                case "shoot":
                    Mod.SMonitor.Log("TODO", StardewModdingAPI.LogLevel.Warn);
                    return;
                case "state":
                    UpdateState(args);
                    return;
                case "heal":
                    Health = (int)Math.Min(Health + Convert.ToInt32(args), MaxHealth);
                    return;
                case "explosion":
                    currentLocation.explode(Tile, Convert.ToInt32(args), null);
                    return;
                case "sound":
                    currentLocation.playSound(args);
                    return;
                case "break":
                    currentLocation.characterDestroyObjectWithinRectangle(GetBoundingBox(), true);
                    return;
                case "spawn":
                    var m = new CustomMonster(args);
                    m.Position = Position;
                    currentLocation.characters.Add(m);
                    return;
            }

            if (action.StartsWith("variable_", StringComparison.OrdinalIgnoreCase))
            {
                string v = action.Substring("variable_".Length);
                using var dt = new DataTable();
                object val_ = dt.Compute(DoConditionReplacements(args.Trim()), string.Empty);
                float val = 0;
                if (val_ is int)
                    val = (int)val_;
                else if (val_ is float)
                    val = (float)val_;
                else
                {
                    Mod.SMonitor.Log("Variable result for " + action + " in " + currentState.Value + " resulted in something not a number", StardewModdingAPI.LogLevel.Warn);
                    return;
                }
                if (!vars.ContainsKey(v.Trim()))
                    vars.Add(v.Trim(), val);
                else
                    vars[v.Trim()] = val;
            }
        }

        private string DoConditionReplacements(string cond)
        {
            Mod.SMonitor.Log($"checking event condition: {cond}", LogLevel.Warn);

            cond = cond.Replace("$HEALTH", health.Value.ToString());
            cond = cond.Replace("$STATE_TIMER", stateTimer.Value.ToString());
            if (cond.Contains("$CLOSEST_PLAYER"))
                cond = cond.Replace("$CLOSEST_PLAYER", ((findPlayer().Position - Position).Length() / Game1.tileSize).ToString());
            if (cond.Contains("$RANDOM"))
                cond = cond.Replace("$RANDOM", Game1.random.NextDouble().ToString());
            foreach (var v in vars.Pairs)
            {
                cond = cond.Replace("$$" + v.Key, v.Value.ToString());
            }

            Mod.SMonitor.Log($"returning event condition: {cond}", LogLevel.Warn);

            return cond;
        }
    }
}
