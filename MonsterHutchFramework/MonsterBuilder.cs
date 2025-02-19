
using System.Threading;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using static StardewValley.Minigames.CraneGame;

namespace MonsterHutchFramework
{
    internal class MonsterBuilder
    {
        public static Monster CreateMonster(Vector2 vector, MonsterHutchData data)
        {
            var type = data.MonsterType;
            switch (type)
            {
                case "Angry Roger":
                    var roger = new AngryRoger(vector);
                    roger.Name = "Angry Roger";
                    roger.Sprite = new AnimatedSprite("Characters\\Monsters\\" + roger.Name, 0, 32, 32);
                    UpdateMonsterStats(roger, data);
                    return roger;
                case "Bat":
                case "Frost Bat":
                case "Lava Bat":
                case "Iridium Bat":
                case "Haunted Skull":
                case "Magma Sprite":
                case "Magma Sparker":
                    var bat = new Bat(vector);
                    if (type == "Frost Bat")
                    {
                        bat.Name = "Frost Bat";
                        bat.reloadSprite();
                    }
                    if (type == "Lava Bat")
                    {
                        bat.Name = "Lava Bat";
                        bat.reloadSprite();
                    }
                    if (type == "Iridium Bat")
                    {
                        bat.Name = "Iridium Bat";
                        bat.reloadSprite();
                        bat.cursedDoll.Value = true;
                        bat.shakeTimer = 100;
                    }
                    if (type == "Haunted Skull")
                    {
                        bat.Name = "Haunted Skull";
                        bat.reloadSprite();
                        bat.hauntedSkull.Value = true;
                        bat.cursedDoll.Value = true;
                        bat.shakeTimer = 100;
                    }
                    if (type == "Magma Sprite")
                    {
                        bat.Name = "Magma Sprite";
                        bat.reloadSprite();
                        bat.magmaSprite.Value = true;
                        bat.cursedDoll.Value = true;
                        bat.shakeTimer = 100;
                    }
                    if (type == "Magma Sparker")
                    {
                        bat.Name = "Magma Sparker";
                        bat.reloadSprite();
                        bat.magmaSprite.Value = true;
                        bat.cursedDoll.Value = true;
                        bat.shakeTimer = 100;
                        bat.canLunge.Value = true;
                    }
                    UpdateMonsterStats(bat, data);
                    return bat;
                case "Big Slime":
                    var bigslime = new BigSlime(vector, 0);
                    UpdateMonsterStats(bigslime, data);
                    return bigslime;
                case "Blue Squid":
                    var bluesquid = new BlueSquid(vector);
                    UpdateMonsterStats(bluesquid, data);
                    return bluesquid;
                case "Bug":
                    var bug = new Bug(vector, 1);
                    UpdateMonsterStats(bug, data);
                    return bug;
                case "Pepper Rex":
                    var rex = new DinoMonster(vector);
                    UpdateMonsterStats(rex, data);
                    return rex;
                case "Duggy":
                case "Magma Duggy":
                    var duggy = new Duggy(vector);
                    UpdateMonsterStats(duggy, data);
                    return duggy;
                case "Dust Spirit":
                    var dust = new DustSpirit(vector);
                    UpdateMonsterStats(dust, data);
                    return dust;
                case "Dwarvish Sentry":
                    var sentry = new DwarvishSentry(vector);
                    UpdateMonsterStats(sentry, data);
                    return sentry;
                case "Fly":
                    var fly = new Fly(vector);
                    UpdateMonsterStats(fly, data);
                    return fly;
                case "Ghost":
                //case "Carbon Ghost": // uses name field for animation update
                case "Putrid Ghost":
                    var ghost = new Ghost(vector, type);
                    UpdateMonsterStats(ghost, data);
                    return ghost;
                case "Grub":
                    var grub = new Grub(vector);
                    UpdateMonsterStats(grub, data);
                    return grub;
                case "Hot Head":
                    var hothead = new HotHead(vector);
                    UpdateMonsterStats(hothead, data);
                    return hothead;
                case "Lava Lurk":
                    var lurk = new LavaLurk(vector);
                    UpdateMonsterStats(lurk, data);
                    return lurk;
                case "Spider":
                    var spider = new Leaper(vector);
                    UpdateMonsterStats(spider, data);
                    return spider;
                case "Metal Head":
                    var metalhead = new MetalHead("Metal Head", vector);
                    UpdateMonsterStats(metalhead, data);
                    return metalhead;
                case "Mummy":
                    var mummy = new Mummy(vector);
                        UpdateMonsterStats(mummy, data);
                        return mummy;
                case "Rock Crab":
                case "Lava Crab":
                case "Iridium Crab":
                case "False Magma Cap":
                case "Stick Bug":
                    var crab = new RockCrab(vector);
                    if (type == "Stick Bug")
                        crab.isStickBug.Value = true;
                    crab.waiter = false;
                    UpdateMonsterStats(crab, data);
                    return crab;

                case "Stone Golem":
                case "Wilderness Golem":
                    var golem = new RockGolem(vector);
                    UpdateMonsterStats(golem, data);
                    return golem;
                case "Serpent":
                case "Royal Serpent":
                    var serpent = new Serpent(vector, type);
                    UpdateMonsterStats(serpent, data);
                    return serpent;
                case "Shadow Girl":
                    var girl = new ShadowGirl(vector);
                    UpdateMonsterStats(girl, data);
                    return girl;
                case "Shadow Guy":
                    var guy = new ShadowGuy(vector);
                    UpdateMonsterStats(guy, data);
                    return guy;
                case "Shadow Brute":
                    var brute = new ShadowBrute(vector);
                    UpdateMonsterStats(brute, data);
                    return brute;
                case "Shadow Shaman":
                    var shaman = new ShadowShaman(vector);
                    UpdateMonsterStats(shaman, data);
                    return shaman;
                case "Shadow Sniper":
                    var sniper = new Shooter(vector);
                    UpdateMonsterStats(sniper, data);
                    return sniper;
                case "Skeleton":
                //case "Skeleton Warrior": // can't find this variant in game code
                case "Skeleton Mage":
                    bool isMage = false;
                    if (type == "Skeleton Mage")
                        isMage = true;
                    var skeleton = new Skeleton(vector, isMage);
                    UpdateMonsterStats(skeleton, data);
                    return skeleton;
                case "Squid Kid":
                    var kid = new SquidKid(vector);
                    UpdateMonsterStats(kid, data);
                    return kid;
                //case "CustomMonster":
                //    var custom = new MonstersTheFramework.CustomMonster.CustomMonster(data.Name);
                //    UpdateMonsterStats(custom, data);
                //    custom.Position = vector;
                //    return custom;
                default:
                    var monster = new Monster();
                    ModEntry.SMonitor.Log($"Monster type {type} not found in data.", LogLevel.Error);
                    return monster;
            }
        }
        public static void UpdateMonsterStats (Monster monster, MonsterHutchData data)
        {
            // HideShadow does nothing, the monster's drawAboveAllLayers method ignores it
            //if (data.HideShadow)
            //    monster.modData.Add("MHF_hideShadow", "true");
            //monster.IsWalkingTowardPlayer = false;
            if (data.ScaleMin < data.ScaleMax)
                monster.Scale = (float)Game1.random.Next(data.ScaleMin, data.ScaleMax) / 100f;
            else if (data.ScaleMin != 100 && data.ScaleMin > 0 && data.ScaleMin == data.ScaleMax)
                monster.Scale = data.ScaleMax;
            if (!string.IsNullOrEmpty(data.TexturePath))
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
