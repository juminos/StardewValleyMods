using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace MonsterHutchFramework
{
    internal class MonsterBuilder
    {
        public static Monster CreateMonster(Vector2 vector, MonsterHutchData data, float scale = -1f)
        {
            if (string.IsNullOrEmpty(data.Name) || string.IsNullOrEmpty(data.MonsterType))
            {
                var monster = new Monster();
                ModEntry.SMonitor.Log($"MonsterType and/or Name fields are missing.", LogLevel.Error);
                return monster;
            }
            var type = data.MonsterType;
            switch (type)
            {
                case "Angry Roger":
                    var roger = new AngryRoger(vector);
                    roger.Name = "Angry Roger";
                    roger.Sprite = new AnimatedSprite("Characters\\Monsters\\" + roger.Name, 0, 32, 32);
                    UpdateMonsterStats(roger, data, scale);
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
                    UpdateMonsterStats(bat, data, scale);
                    return bat;
                case "Big Slime":
                    var bigslime = new BigSlime(vector, 0);
                    UpdateMonsterStats(bigslime, data, scale);
                    return bigslime;
                case "Blue Squid":
                    var bluesquid = new BlueSquid(vector);
                    UpdateMonsterStats(bluesquid, data, scale);
                    return bluesquid;
                case "Bug":
                    var bug = new Bug(vector, 1);
                    UpdateMonsterStats(bug, data, scale);
                    return bug;
                case "Pepper Rex":
                    var rex = new DinoMonster(vector);
                    UpdateMonsterStats(rex, data, scale);
                    rex.timeUntilNextAttack = 99999999;
                    return rex;
                // Duggy can only appear in tillable tiles 
                //case "Duggy":
                //case "Magma Duggy":
                //    var duggy = new Duggy(vector);
                //    UpdateMonsterStats(duggy, data, scale);
                //    return duggy;
                case "Dust Spirit":
                    var dust = new DustSpirit(vector);
                    UpdateMonsterStats(dust, data, scale);
                    return dust;
                case "Dwarvish Sentry":
                    var sentry = new DwarvishSentry(vector);
                    UpdateMonsterStats(sentry, data, scale);
                    return sentry;
                case "Fly":
                    var fly = new Fly(vector);
                    UpdateMonsterStats(fly, data, scale);
                    return fly;
                case "Ghost":
                case "Carbon Ghost":
                case "Putrid Ghost":
                    var ghost = new Ghost(vector, type);
                    UpdateMonsterStats(ghost, data, scale);
                    return ghost;
                // Don't really do anything after spawning
                //case "Grub":
                //    var grub = new Grub(vector);
                //    UpdateMonsterStats(grub, data, scale);
                //    return grub;
                case "Hot Head":
                    var hothead = new HotHead(vector);
                    UpdateMonsterStats(hothead, data, scale);
                    hothead.timeUntilExplode.Value = 99999999.0f;
                    return hothead;
                case "Lava Lurk":
                    var lurk = new LavaLurk(vector);
                    UpdateMonsterStats(lurk, data, scale);
                    return lurk;
                case "Spider":
                    var spider = new Leaper(vector);
                    UpdateMonsterStats(spider, data, scale);
                    return spider;
                case "Metal Head":
                    var metalhead = new MetalHead("Metal Head", vector);
                    UpdateMonsterStats(metalhead, data, scale);
                    return metalhead;
                // Warning: damage to farmer resets to default after reassembling
                case "Mummy":
                    var mummy = new Mummy(vector);
                        UpdateMonsterStats(mummy, data, scale);
                        return mummy;
                case "Rock Crab":
                case "Lava Crab":
                case "Stick Bug":
                    var crab = new RockCrab(vector);
                    if (type == "Stick Bug")
                        crab.isStickBug.Value = true;
                    crab.waiter = false;
                    UpdateMonsterStats(crab, data, scale);
                    return crab;
                case "Iridium Crab":
                // truffle crab behavior doesn't really work in hutch
                //case "Truffle Crab":
                case "False Magma Cap":
                    var namedCrab = new RockCrab(vector, type);
                    namedCrab.waiter = false;
                    UpdateMonsterStats(namedCrab, data, scale);
                    return namedCrab;
                case "Stone Golem":
                case "Wilderness Golem":
                    var golem = new RockGolem(vector);
                    UpdateMonsterStats(golem, data, scale);
                    return golem;
                case "Serpent":
                case "Royal Serpent":
                    var serpent = new Serpent(vector, type);
                    UpdateMonsterStats(serpent, data, scale);
                    return serpent;
                // Incomplete code
                //case "Shadow Girl":
                //    var girl = new ShadowGirl(vector);
                //    UpdateMonsterStats(girl, data, scale);
                //    return girl;
                //case "Shadow Guy":
                //    var guy = new ShadowGuy(vector);
                //    UpdateMonsterStats(guy, data, scale);
                //    return guy;
                case "Shadow Brute":
                    var brute = new ShadowBrute(vector);
                    UpdateMonsterStats(brute, data, scale);
                    return brute;
                case "Shadow Shaman":
                    var shaman = new ShadowShaman(vector);
                    UpdateMonsterStats(shaman, data, scale);
                    return shaman;
                case "Shadow Sniper":
                    var sniper = new Shooter(vector);
                    UpdateMonsterStats(sniper, data, scale);
                    return sniper;
                case "Skeleton":
                //case "Skeleton Warrior": // can't find this variant in game code
                case "Skeleton Mage":
                    bool isMage = false;
                    if (type == "Skeleton Mage")
                        isMage = true;
                    var skeleton = new Skeleton(vector, isMage);
                    UpdateMonsterStats(skeleton, data, scale);
                    return skeleton;
                case "Squid Kid":
                    var kid = new SquidKid(vector);
                    UpdateMonsterStats(kid, data, scale);
                    return kid;
                //case "CustomMonster":
                //    var api = ModEntry.Mod.Helper.ModRegistry.GetApi<IMonstersTheFrameworkApi>("juminos.MonstersTheFramework1.6");
                //    if (api is null)
                //    {
                //        var errorApi = new Monster();
                //        ModEntry.SMonitor.Log($"MonstersTheFrameworkApi is null.", LogLevel.Error);
                //        return errorApi;
                //    }
                //    Monster customMonster = api.GetCustomMonster(data.Name);
                //    UpdateMonsterStats(customMonster, data, scale);
                //    customMonster.Position = vector;
                //    return customMonster;
                default:
                    var monster = new Monster();
                    ModEntry.SMonitor.Log($"Monster type {type} not found in data.", LogLevel.Error);
                    return monster;
            }
        }
        public static void UpdateMonsterStats (Monster monster, MonsterHutchData data, float scale)
        {
            // HideShadow does nothing, the monster's drawAboveAllLayers method ignores it
            //if (data.HideShadow)
            //    monster.modData.Add("MHF_hideShadow", "true");
            //monster.IsWalkingTowardPlayer = false;
            if (scale < 0)
            {
                if (data.ScaleMin < data.ScaleMax)
                {
                    scale = (float)Game1.random.Next(data.ScaleMin, data.ScaleMax) / 100f;
                    monster.Scale = scale;
                    monster.modData.Add($"{ModEntry.Mod.ModManifest.UniqueID}_Scale", scale.ToString());
                }
                else if (data.ScaleMin != 100 && data.ScaleMin > 0 && data.ScaleMin == data.ScaleMax)
                {
                    scale = data.ScaleMax;
                    monster.Scale = scale;
                    monster.modData.Add($"{ModEntry.Mod.ModManifest.UniqueID}_Scale", scale.ToString());
                }
            }
            else
                monster.Scale = scale;
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
            {
                monster.MaxHealth = data.MaxHealthOverride;
                monster.Health = monster.MaxHealth;
            }
            monster.farmerPassesThrough = !data.FarmerCollision;
            monster.moveTowardPlayerThreshold.Value = data.MoveTowardPlayerThresholdOverride;

            monster.modData.Add($"{ModEntry.Mod.ModManifest.UniqueID}_Name", data.Name);
            //monster.Name = data.Name;
        }
    }
}
