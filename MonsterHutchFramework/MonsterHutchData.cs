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
    public string? MonsterType { get; set; }
    // Name for this monster
    public string? Name { get; set; }
    // Drops replaces monster drop on kill list
    public List<Drops> Drops { get; set; } = new List<Drops>();
    // Will set monster speed after creation
    public int SpeedOverride { get; set; }
    // probably want to keep false, collision will make navigation in hutch difficult
    public bool FarmerCollision { get; set; } = false;
    // keep this low if you don't want to be swarmed
    public int MoveTowardPlayerThresholdOverride { get; set; } = 2;
    // useful for nerfing strong monsters
    public int DamageToFarmerOverride { get; set; }
    // same as above
    public int MaxHealthOverride { get; set; }
    // Usually set to true but is never used (only for events?) and ignored by drawAboveAllLayers method
    //public bool HideShadow { get; set; } = true;
    // Scale is used by dust spirit for size randomization (base.Scale = (float)Game1.random.Next(75, 101) / 100f)
    public int ScaleMin { get; set; } = 100;
    public int ScaleMax { get; set; } = 100;
    // can be vanilla texture path or mod loaded texture
    public string? TexturePath { get; set; } = null;
    // number of watered monsters required to produce
    public int NumberRequiredToProduce { get; set; } = 1;
    // chance that monster produce is dropped if water condition is met
    public int ProduceChance { get; set; } = 100;
    // Conditional check for deluxe produce
    public string? ProduceCondition { get; set; }
    // chance that produce is replaced with deluxe produce
    public int DeluxeChance { get; set; } = 33;
    // Conditional check for deluxe produce
    public string? DeluxeCondition { get; set; }
    // weighted list of valid produce items
    public List<ProduceData> ProduceData { get; set; } = new List<ProduceData>();
    // weighted list of valid deluxe produce items
    public List<DeluxeProduceData> DeluxeProduceData { get; set; } = new List<DeluxeProduceData>();
    // item placed in incubator to create this monster
    public string? InputItemId { get; set; } = null;
    // number required
    public int InputItemCount { get; set; } = 1;
    // number of in-game minutes until incubation is done
    public int IncubationTime { get; set; } = 4000;
    // weighted value used to determine which monster to create if multiple monsters have the same input item
    public int OutputWeight { get; set; } = 1; 
}
public class Drops
{
    // ID for this entry in the list
    public string? Id { get; set; }
    // unqualified or qualified item id should work
    public string? ItemId { get; set; }
    // percent chance this item drops
    public int Chance { get; set; } = 100;
    // Quantity to drop
    public int Quantity { get; set; } = 1;
}
public class ProduceData
{
    // id for this entry in list
    public string? Id { get; set; }
    // unqualified or qualified item id should work
    public string? ItemId { get; set; }
    // number to produce
    public int Count { get; set; } = 1;
    // whether is dropped as debris (rather than spawned like forage)
    public bool IsDropped { get; set; } = false;
    // weighted value for determining which item to select
    public int Weight { get; set; } = 1; 
}
public class DeluxeProduceData
{
    // id for this entry in list
    public string? Id { get; set; }
    // unqualified or qualified item id should work
    public string? ItemId { get; set; }
    // number to produce
    public int Count { get; set; } = 1;
    // whether is dropped as debris (rather than spawned like forage)
    public bool IsDropped { get; set; } = false;
    // weighted value for determining which item to select
    public int Weight { get; set; } = 1;
}