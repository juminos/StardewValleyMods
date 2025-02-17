using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using static StardewValley.Minigames.CraneGame;
using static StardewValley.Monsters.DinoMonster;

namespace MonsterHutchFramework
{
    internal class MonsterBuilder
    {
        public static Monster CreateMonster(Vector2 vector, MonsterHutchData data)
        {
            var type = data.MonsterType;

            // AngryRoger class (probably broken)
            if (type == "Angry Roger")
            {
                var monster = new AngryRoger(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Bat class
            if (type == "Bat" || 
                type == "Frost Bat" || 
                type == "Lava Bat" || 
                type == "Iridium Bat" || 
                type == "Haunted Skull" || 
                type == "Magma Sprite" || 
                type == "Magma Sparker")
            {
                var monster = new Bat(vector);
                if (type == "Frost Bat")
                {
                    monster.Name = "Frost Bat";
                    monster.reloadSprite();
                }
                if (type == "Lava Bat")
                {
                    monster.Name = "Lava Bat";
                    monster.reloadSprite();
                }
                if (type == "Iridium Bat")
                {
                    monster.Name = "Iridium Bat";
                    monster.reloadSprite();
                    monster.cursedDoll.Value = true;
                    monster.shakeTimer = 100;
                }
                if (type == "Haunted Skull")
                {
                    monster.Name = "Haunted Skull";
                    monster.reloadSprite();
                    monster.hauntedSkull.Value = true;
                    monster.cursedDoll.Value = true;
                    monster.shakeTimer = 100;
                }
                if (type == "Magma Sprite")
                {
                    monster.Name = "Magma Sprite";
                    monster.reloadSprite();
                    monster.magmaSprite.Value = true;
                    monster.cursedDoll.Value = true;
                    monster.shakeTimer = 100;
                }
                if (type == "Magma Sparker")
                {
                    monster.Name = "Magma Sparker";
                    monster.reloadSprite();
                    monster.magmaSprite.Value = true;
                    monster.cursedDoll.Value = true;
                    monster.shakeTimer = 100;
                    monster.canLunge.Value = true;
                }
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // BigSlime class
            if (type == "Big Slime")
            {
                var monster = new BigSlime(vector, 0);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // BlueSquid class
            if (type == "Blue Squid")
            {
                var monster = new BlueSquid(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Bug class
            if (type == "Bug")
            {
                var monster = new Bug(vector, 1);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // DinoMonster class
            if (type == "Pepper Rex")
            {
                var monster = new DinoMonster(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Duggy class
            if (type == "Duggy" || type == "Magma Duggy")
            {
                var monster = new Duggy(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // DustSpirit class
            if (type == "Dust Spirit")
            {
                var monster = new DustSpirit(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // DwarvishSentry class
            if (type == "Dwarvish Sentry")
            {
                var monster = new DwarvishSentry(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            //Fly class
            if (type == "Fly")
            {
                var monster = new Fly(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Ghost class
            if (type == "Ghost" ||
                //type == "Carbon Ghost" ||  // uses name field for animation update
                type == "Putrid Ghost")
            {
                var monster = new Ghost(vector, type);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Grub class
            if (type == "Grub")
            {
                var monster = new Grub(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // HotHead class
            if (type == "Hot Head")
            {
                var monster = new HotHead(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // LavaLurk class
            if (type == "Lava Lurk")
            {
                var monster = new LavaLurk(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Leaper class
            if (type == "Spider")
            {
                var monster = new Leaper(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // MetalHead class
            if (type == "Metal Head")
            {
                var monster = new MetalHead("Metal Head", vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Mummy class
            if (type == "Mummy")
            {
                var monster = new Mummy(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // RockCrab class
            if (type == "Rock Crab" || type == "Lava Crab" || type == "Iridium Crab" || type == "False Magma Cap" || type == "Stick Bug")
            {
                var monster = new RockCrab(vector);

                // isStickBug property evades bomb/tool interaction
                if (type == "Stick Bug")
                {
                    monster.isStickBug.Value = true;
                }
                monster.waiter = false;
                UpdateMonsterStats(monster, data);

                return monster;
            }

            //RockGolem class
            if (type == "Stone Golem" || type == "Wilderness Golem")
            {
                var monster = new RockGolem(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Serpent class
            if (type == "Serpent" || type == "Royal Serpent")
            {
                var monster = new Serpent(vector, type);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // ShadowGirl class (probably broken)
            if (type == "Shadow Girl")
            {
                var monster = new ShadowGirl(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // ShadowGuy class (probably broken)
            if (type == "Shadow Guy")
            {
                var monster = new ShadowGuy(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // ShadowBrute class
            if (type == "Shadow Brute")
            {
                var monster = new ShadowBrute(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // ShadowShaman class
            if (type == "Shadow Shaman")
            {
                var monster = new ShadowShaman(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Shooter class
            if (type == "Shadow Sniper")
            {
                var monster = new Shooter(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // Skeleton class
            if (type == "Skeleton" || 
                //type == "Skeleton Warrior" || // can't find this variant in game code
                type == "Skeleton Mage")
            {
                bool isMage = false;
                if (type == "Skeleton Mage")
                {
                    isMage = true;
                }
                var monster = new Skeleton(vector, isMage);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            // SquidKid class
            if (type == "Squid Kid")
            {
                var monster = new SquidKid(vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            else
            {
                ModEntry.SMonitor.Log($"Monster type {type} not found. Attempting to create generic monster.", LogLevel.Warn);

                var monster = new Monster(type, vector);
                UpdateMonsterStats(monster, data);

                return monster;
            }

            //if (type == "Fireball")           // Probably unused DinoMonster code
            //if (type == "Crow")               // Crow is not a valid monster type
            //if (type == "Frog")               // Frog is not a valid monster type
            //if (type == "Cat")                // Cat is not a valid monster type
            //if (type == "Spiker)              // wouldn't make sense in hutch

            // GreenSlime based monsters would likely complicate slime hutch behavior
            //if (type == "Green Slime")
            //if (type == "Frost Jelly")
            //if (type == "Sludge")
            //if (type == "Tiger Slime")
        }
        public static void UpdateMonsterStats (Monster monster, MonsterHutchData data)
        {
            monster.HideShadow = data.HideShadow;
            //monster.IsWalkingTowardPlayer = false;
            if (data.ScaleMin < data.ScaleMax)
            {
                monster.Scale = (float)Game1.random.Next(data.ScaleMin, data.ScaleMax) / 100f;
            }
            if (data.TexturePath != null)
            {
                try
                {
                    monster.Sprite.LoadTexture(data.TexturePath);
                }
                catch
                {
                    ModEntry.SMonitor.Log($"Failed loading '{data.TexturePath}' texture for {monster.Name}.", LogLevel.Error);
                }
            }
            monster.objectsToDrop.Clear();
            if (data.Drops.Count > 0)
            {
                for (int i = 0; i < data.Drops.Count; i++)
                {
                    if (Game1.random.NextDouble() < ((double)data.Drops[i].Chance / 100.0))
                    {
                        for (int j = 0; j < data.Drops[i].Quantity; j++)
                            monster.objectsToDrop.Add(data.Drops[i].ItemId);
                    }
                }
            }
            if (data.SpeedOverride > 0)
                monster.Speed = data.SpeedOverride;
            if (data.DamageToFarmerOverride > 0)
                monster.DamageToFarmer = data.DamageToFarmerOverride;
            if (data.MaxHealthOverride > 0)
                monster.MaxHealth = data.MaxHealthOverride;
            monster.farmerPassesThrough = !data.FarmerCollision;
            monster.moveTowardPlayerThreshold.Value = data.MoveTowardPlayerThresholdOverride;
            monster.Name = data.Name;
        }
    }
}
