using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Monsters;
using static StardewValley.Minigames.CraneGame;

namespace MonsterHutchFramework
{
    internal class MonsterBuilder
    {
        public static Monster CreateMonster(Vector2 vector, MonsterHutchData data)
        {
            var type = data.MonsterType;

            // Dust Spirit
            if (type == "Dust Spirit")
            {
                var monster = new DustSpirit(vector);
                monster.Sprite.interval = 45f;
                //monster.voice = (byte)Game1.random.Next(1, 24);
                monster.HideShadow = data.HideShadow;
                monster.IsWalkingTowardPlayer = false;
                if (data.ScaleMin < data.ScaleMax)
                {
                    monster.Scale = (float)Game1.random.Next(data.ScaleMin, data.ScaleMax) / 100f;
                }
                if (data.TexturePath != null)
                {
                    var texture = Game1.content.Load<Texture2D>($"{data.TexturePath}");
                    try
                    {
                        monster.Sprite.LoadTexture(texture.Name);
                    }
                    catch
                    {
                        ModEntry.SMonitor.Log($"Failed loading '{texture}' texture for {monster.Name}.", LogLevel.Error);
                    }
                }
                monster.objectsToDrop.Clear();
                for (int i = 0; i < data.Drops.Count; i++)
                {
                    monster.objectsToDrop.Add(data.Drops[i]);
                }
                monster.Speed = data.SpeedOverride;
                monster.addedSpeed = 0;
                monster.farmerPassesThrough = !data.FarmerCollision;
                monster.moveTowardPlayerThreshold.Value = data.MoveTowardPlayerThresholdOverride;
                monster.Name = data.Name;

                return monster;
            }

            // Bat class monsters
            if (type == "Bat" || type == "Frost Bat" || type == "Lava Bat" || type == "Iridium Bat" || type == "Haunted Skull" || type == "Magma Sprite" || type == "Magma Sparker")
            {
                var monster = new Bat(vector);
                if (type == "Iridium Bat" || type == "Haunted Skull" || type == "Magma Sprite" || type == "Magma Sparker")
                {
                    monster.shakeTimer = 100;
                    monster.cursedDoll.Value = true;
                    if(type == "Haunted Skull")
                    {
                        monster.hauntedSkull.Value = true;
                    }
                    if(type == "Magma Sprite" || type == "Magma Sparker")
                    {
                        monster.magmaSprite.Value = true;
                    }
                }
                monster.HideShadow = data.HideShadow;
                monster.IsWalkingTowardPlayer = false;
                monster.Name = data.Name;
                if (data.ScaleMin < data.ScaleMax)
                {
                    monster.Scale = (float)Game1.random.Next(data.ScaleMin, data.ScaleMax) / 100f;
                }
                if (data.TexturePath != null)
                {
                    var texture = Game1.content.Load<Texture2D>($"{data.TexturePath}");
                    try
                    {
                        monster.Sprite.LoadTexture(texture.Name);
                    }
                    catch
                    {
                        ModEntry.SMonitor.Log($"Failed loading '{texture}' texture for {monster.Name}.", LogLevel.Error);
                    }
                }
                monster.Speed = data.SpeedOverride;
                monster.addedSpeed = 0;
                monster.farmerPassesThrough = !data.FarmerCollision;
                monster.objectsToDrop.Clear();
                for (int i = 0; i < data.Drops.Count; i++)
                {
                    monster.objectsToDrop.Add(data.Drops[i]);
                }
                monster.moveTowardPlayerThreshold.Value = data.MoveTowardPlayerThresholdOverride;

                return monster;
            }

            if (type == "Stone Golem")
            {

            }
            if (type == "Wilderness Golem")
            {

            }
            if (type == "Grub")
            {

            }
            if (type == "Fly")
            {

            }
            if (type == "Frost Jelly")
            {

            }
            if (type == "Sludge")
            {

            }
            if (type == "Shadow Guy")
            {

            }
            if (type == "Ghost")
            {

            }
            if (type == "Carbon Ghost")
            {

            }
            if (type == "Duggy")
            {

            }
            if (type == "Rock Crab")
            {

            }
            if (type == "Lava Crab")
            {

            }
            if (type == "Iridium Crab")
            {

            }
            if (type == "Fireball")
            {

            }
            if (type == "Squid Kid")
            {

            }
            if (type == "Skeleton Warrior")
            {

            }
            if (type == "Crow")
            {

            }
            if (type == "Frog")
            {

            }
            if (type == "Cat")
            {

            }
            if (type == "Shadow Brute")
            {

            }
            if (type == "Shadow Shaman")
            {

            }
            if (type == "Skeleton")
            {

            }
            if (type == "Skeleton Mage")
            {

            }
            if (type == "Metal Head")
            {

            }
            if (type == "Spiker")
            {

            }
            if (type == "Bug")
            {
                var monster = new Bug(vector, 1);

                monster.Sprite.SpriteHeight = 16;
                monster.Sprite.UpdateSourceRect();
                //monster.onCollision = monster.collide;
                monster.yOffset = -32f;
                monster.setMovingInFacingDirection();
                monster.defaultAnimationInterval.Value = 40;
                monster.collidesWithOtherCharacters.Value = false;
                monster.HideShadow = data.HideShadow;
                monster.IsWalkingTowardPlayer = false;
                if (data.ScaleMin < data.ScaleMax)
                {
                    monster.Scale = (float)Game1.random.Next(data.ScaleMin, data.ScaleMax) / 100f;
                }
                if (data.TexturePath != null)
                {
                    var texture = Game1.content.Load<Texture2D>($"{data.TexturePath}");
                    try
                    {
                        monster.Sprite.LoadTexture(texture.Name);
                    }
                    catch
                    {
                        ModEntry.SMonitor.Log($"Failed loading '{texture}' texture for {monster.Name}.", LogLevel.Error);
                    }
                }
                monster.objectsToDrop.Clear();
                for (int i = 0; i < data.Drops.Count; i++)
                {
                    monster.objectsToDrop.Add(data.Drops[i]);
                }
                monster.Speed = data.SpeedOverride;
                monster.addedSpeed = 0;
                monster.farmerPassesThrough = !data.FarmerCollision;
                monster.moveTowardPlayerThreshold.Value = data.MoveTowardPlayerThresholdOverride;
                monster.Name = data.Name;

                return monster;
            }
            if (type == "Mummy")
            {

            }

            // big slime color looks complicated
            //if (type == "Big Slime")
            //{
            ////big slime
            //ignoreMovementAnimations = true;
            //Sprite.ignoreStopAnimation = true;
            //Sprite.SpriteWidth = 32;
            //Sprite.SpriteHeight = 32;
            //Sprite.UpdateSourceRect();
            //Sprite.framesPerAnimation = 8;
            //c.Value = Color.White;
            //Sprite.interval = 300f;
            //base.HideShadow = true;
            //}
            if (type == "Serpent")
            {

            }
            if (type == "Pepper Rex")
            {

            }
            if (type == "Tiger Slime")
            {

            }
            if (type == "Lava Lurk")
            {

            }
            if (type == "Hot Head")
            {

            }
            if (type == "Magma Sprite")
            {

            }
            if (type == "Magma Duggy")
            {

            }
            if (type == "Magma Sparker")
            {

            }
            if (type == "False Magma Cap")
            {

            }
            if (type == "Dwarvish Sentry")
            {

            }
            if (type == "Putrid Ghost")
            {

            }
            if (type == "Shadow Sniper")
            {

            }
            if (type == "Spider")
            {

            }
            if (type == "Royal Serpent")
            {

            }
            if (type == "Blue Squid")
            {
                var monster = new BlueSquid(vector);
                monster.Sprite.SpriteHeight = 24;
                monster.Sprite.SpriteWidth = 24;
                monster.reloadSprite();
                monster.Sprite.UpdateSourceRect();
                monster.Slipperiness = Game1.random.Next(6, 9);
                monster.canMoveTimer = Game1.random.Next(500);
                monster.isHardModeMonster.Value = true;
                monster.HideShadow = data.HideShadow;
                monster.IsWalkingTowardPlayer = false;
                if (data.ScaleMin < data.ScaleMax)
                {
                    monster.Scale = (float)Game1.random.Next(data.ScaleMin, data.ScaleMax) / 100f;
                }
                if (data.TexturePath != null)
                {
                    var texture = Game1.content.Load<Texture2D>($"{data.TexturePath}");
                    try
                    {
                        monster.Sprite.LoadTexture(texture.Name);
                    }
                    catch
                    {
                        ModEntry.SMonitor.Log($"Failed loading '{texture}' texture for {monster.Name}.", LogLevel.Error);
                    }
                }
                monster.objectsToDrop.Clear();
                for (int i = 0; i < data.Drops.Count; i++)
                {
                    monster.objectsToDrop.Add(data.Drops[i]);
                }
                monster.Speed = data.SpeedOverride;
                monster.addedSpeed = 0;
                monster.farmerPassesThrough = !data.FarmerCollision;
                monster.moveTowardPlayerThreshold.Value = data.MoveTowardPlayerThresholdOverride;
                monster.Name = data.Name;

                return monster;
            }

            //green slime based monsters would likely complicate slime hutch behavior
            //if (type == "Green Slime")
            //{
            //base.Slipperiness = 4;
            //readyToMate = Game1.random.Next(1000, 120000);
            //int green = Game1.random.Next(200, 256);
            //color.Value = new Color(green / Game1.random.Next(2, 10), Game1.random.Next(180, 256), (Game1.random.NextDouble() < 0.1) ? 255 : (255 - green));
            //firstGeneration.Value = true;
            //flip = Game1.random.NextBool();
            //cute.Value = Game1.random.NextDouble() < 0.49;
            //base.HideShadow = true;
            //}

            else
            {
                var monster = new Monster("Angry Roger", vector);
                ModEntry.SMonitor.Log("You've summoned an angry spirit", LogLevel.Warn);

                return monster;
            }









            //pepper rex
            Sprite.SpriteWidth = 32;
            Sprite.SpriteHeight = 32;
            Sprite.UpdateSourceRect();
            timeUntilNextAttack = 2000;
            nextChangeDirectionTime = Game1.random.Next(1000, 3000);
            nextWanderTime = Game1.random.Next(1000, 2000);

            //duggy
            base.IsWalkingTowardPlayer = false;
            base.IsInvisible = true;
            base.DamageToFarmer = 0;
            Sprite.currentFrame = 0;
            base.HideShadow = true;


            //dwarvish sentry
            Sprite.SpriteHeight = 16;
            base.IsWalkingTowardPlayer = false;
            Sprite.UpdateSourceRect();
            base.HideShadow = true;
            isGlider.Value = true;
            base.Slipperiness = 1;
            pauseTimer = 10000f;
            DelayedAction.playSoundAfterDelay("DwarvishSentry", 500);

            //fly
            base.Slipperiness = 24 + Game1.random.Next(-10, 10);
            Halt();
            base.IsWalkingTowardPlayer = false;
            base.HideShadow = true;

            //ghost
            lightSourceId = GenerateLightSourceId(identifier);
            base.Slipperiness = 8;
            isGlider.Value = true;
            base.HideShadow = true;


            //grub
            if (Game1.random.NextBool())
            {
                leftDrift.Value = true;
            }
            FacingDirection = Game1.random.Next(4);
            targetRotation.Value = (rotation = (float)Game1.random.Next(4) / (float)Math.PI);
            this.hard.Value = hard;

            //lava lurk
            Sprite.SpriteWidth = 16;
            Sprite.SpriteHeight = 16;
            Sprite.UpdateSourceRect();
            Initialize();
            ignoreDamageLOS.Value = true;
            SetRandomMovement();
            stateTimer = Utility.RandomFloat(3f, 5f);

            //spider
            forceOneTileWide.Value = true;
            base.IsWalkingTowardPlayer = false;
            nextLeap = Utility.RandomFloat(1f, 1.5f);
            isHardModeMonster.Value = true;
            reloadSprite();

            //metal head
            Sprite.SpriteHeight = 16;
            Sprite.UpdateSourceRect();
            c.Value = Color.White;
            base.IsWalkingTowardPlayer = true;

            //mummy
            Sprite.SpriteHeight = 32;
            Sprite.ignoreStopAnimation = true;
            Sprite.UpdateSourceRect();
            _damageToFarmer = damageToFarmer.Value;

            //rock crab
            waiter = Game1.random.NextDouble() < 0.4;
            moveTowardPlayerThreshold.Value = 3;

            //stone golem
            base.IsWalkingTowardPlayer = false;
            base.Slipperiness = 2;
            jitteriness.Value = 0.0;
            base.HideShadow = true;

            //serpent
            base.Slipperiness = 24 + Game1.random.Next(10);
            Halt();
            base.IsWalkingTowardPlayer = false;
            Sprite.SpriteWidth = 32;
            Sprite.SpriteHeight = 32;
            base.Scale = 0.75f;
            base.HideShadow = true;

            //shadow brute
            Sprite.SpriteHeight = 32;
            Sprite.UpdateSourceRect();

            //shadow girl
            base.IsWalkingTowardPlayer = false;
            moveTowardPlayerThreshold.Value = 8;

            //shadow sniper
            Sprite.SpriteHeight = 32;
            Sprite.SpriteWidth = 32;
            forceOneTileWide.Value = true;
            Sprite.UpdateSourceRect();
            InitializeVariant();

            //spiker
            Sprite.SpriteWidth = 16;
            Sprite.SpriteHeight = 16;
            Sprite.UpdateSourceRect();
            targetDirection = direction;
            base.speed = 14;
            ignoreMovementAnimations = true;
            onCollision = collide;

            //squid kid
            Sprite.SpriteHeight = 16;
            base.IsWalkingTowardPlayer = false;
            Sprite.UpdateSourceRect();
            base.HideShadow = true;

        }
    }
}
