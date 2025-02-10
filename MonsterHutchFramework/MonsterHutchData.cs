using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.GameData;

namespace MonsterHutchFramework;

public class MonsterHutchData
{
    public string? MonsterType; // must match valid monster type (most types in Data/Monsters work)
    public string? Name; // should be unique
    public string? Sound; // not implemented yet
    public List<Drops> Drops = new List<Drops>(); // replaces drop on kill list
    public int SpeedOverride; // set monster speed after creation
    public bool FarmerCollision = false; // probably want to keep false, collision will make navigation in hutch difficult
    public int MoveTowardPlayerThresholdOverride = 2; // keep this low if you don't want to be swarmed
    public int DamageToFarmerOverride; // useful for nerfing strong monsters
    public int MaxHealthOverride; // same as above
    public bool HideShadow = true; // most monsters seem to hide shadow
    public int ScaleMin; // e.g. used by dust spirit for size randomization (base.Scale = (float)Game1.random.Next(75, 101) / 100f)
    public int ScaleMax; // same as above
    public string? TexturePath = null; // can be vanilla texture path or mod loaded texture
    //public List<SkinData> Skins = new List<SkinData>(); // haven't figured out how to save and load skin texture on save loaded event
    public int NumberWatered = 1; // number that 1 full water trough will water
    public int NumberToProduce = 1; // number of watered monsters needed to produce
    public int ProduceChance = 100; // chance that monster produce is dropped if water condition is met
    public int DeluxeChance = 33; // chance that produce is replaced with deluxe produce
    public List<ProduceData> ProduceData = new List<ProduceData>(); // weighted list of valid produce items
    public List<DeluxeProduceData> DeluxeProduceData = new List<DeluxeProduceData>(); // weighted list of valid deluxe produce items
    public string? InputItemId = null; // item placed in incubator to create this monster
    public int InputItemCount = 1; // number required
    public int IncubationTime = 2; // days until incubation is done
    public int OutputWeight = 1; // weighted value used to determine which monster to create if multiple monsters have the same input item
}
public class Drops
{
    public string? Id; // id for this entry in list
    public string? ItemId; // item id
    public int Chance = 100; // percent chance this item drops
}
public class ProduceData
{
    public string? Id; // id for this entry in list
    public string? ItemId; // item id
    public int Count = 1; // number to produce
    public bool IsDropped = false; // whether is dropped as debris (rather than spawned like forage)
    public int Weight = 1; // weighted value for determining which item to select
}
public class DeluxeProduceData
{
    public string? Id; // id for this entry in list
    public string? ItemId; // item id
    public int Count =1; // number to produce
    public bool IsDropped = false; // whether is dropped as debris (rather than spawned like forage)
    public int Weight = 1; // weighted value for determining which item to select
}
public class SkinData
{
    public string? Id;
    public int Weight;
    public string? Texture;
}