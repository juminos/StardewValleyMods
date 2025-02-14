using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.GameData;

namespace MonsterHutchFramework;

public class MonsterHutchData
{
    // MonsterType must match valid monster type (most types in Data/Monsters work)
    public string? MonsterType;
    // Name should match entry key
    public string? Name;
    // Drops replaces monster drop on kill list
    public List<Drops> Drops = new List<Drops>();
    // Will set monster speed after creation
    public int SpeedOverride;
    // probably want to keep false, collision will make navigation in hutch difficult
    public bool FarmerCollision = false;
    // keep this low if you don't want to be swarmed
    public int MoveTowardPlayerThresholdOverride = 2;
    // useful for nerfing strong monsters
    public int DamageToFarmerOverride;
    // same as above
    public int MaxHealthOverride;
    // most (all?) monsters seem to hide shadow
    public bool HideShadow = true;
    // Scale is used by dust spirit for size randomization (base.Scale = (float)Game1.random.Next(75, 101) / 100f)
    public int ScaleMin; 
    public int ScaleMax;
    // can be vanilla texture path or mod loaded texture
    public string? TexturePath = null;
    // number of watered monsters needed to produce
    public int NumberToProduce = 1;
    // chance that monster produce is dropped if water condition is met
    public int ProduceChance = 100;
    // chance that produce is replaced with deluxe produce
    public int DeluxeChance = 33;
    // weighted list of valid produce items
    public List<ProduceData> ProduceData = new List<ProduceData>();
    // weighted list of valid deluxe produce items
    public List<DeluxeProduceData> DeluxeProduceData = new List<DeluxeProduceData>();
    // item placed in incubator to create this monster
    public string? InputItemId = null;
    // number required
    public int InputItemCount = 1;
    // number of in-game minutes until incubation is done
    public int IncubationTime = 4000;
    // weighted value used to determine which monster to create if multiple monsters have the same input item
    public int OutputWeight = 1; 
}
public class Drops
{
    // ID for this entry in the list
    public string? Id;
    // unqualified or qualified item id should work
    public string? ItemId;
    // percent chance this item drops
    public int Chance = 100; 
}
public class ProduceData
{
    // id for this entry in list
    public string? Id;
    // unqualified or qualified item id should work
    public string? ItemId;
    // number to produce
    public int Count = 1;
    // whether is dropped as debris (rather than spawned like forage)
    public bool IsDropped = false;
    // weighted value for determining which item to select
    public int Weight = 1; 
}
public class DeluxeProduceData
{
    // id for this entry in list
    public string? Id;
    // unqualified or qualified item id should work
    public string? ItemId;
    // number to produce
    public int Count = 1;
    // whether is dropped as debris (rather than spawned like forage)
    public bool IsDropped = false;
    // weighted value for determining which item to select
    public int Weight = 1;
}