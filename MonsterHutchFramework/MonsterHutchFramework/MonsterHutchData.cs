using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.GameData;

namespace MonsterHutchFramework.MonsterHutchFramework;

public class MonsterHutchData
{
    public string? MonsterType;
    public string? Name;
    public string? Sound;
    public List<string> Drops = [];
    public int SpeedOverride = 1;
    public bool FarmerCollision = false;
    public int MoveTowardPlayerThresholdOverride = 2;
    public string? TexturePath = null;
    public int NumberWatered = 1;
    public float ProduceChance;
    public float DeluxeChance;
    public List<ProduceData> ProduceData = new List<ProduceData>();
    public List<DeluxeProduceData> DeluxeProduce = new List<DeluxeProduceData>();
    public string? InputItemId = null;
    public int InputItemCount = 0;
    public int IncubationTime = 0;
    public string? CharmerRingId;
}
public class ProduceData
{
    public string? Id;
    public string? ItemId;
    public int Count;
    public bool IsDropped = false;
    public int Weight = 1;
}
public class DeluxeProduceData
{
    public string? Id;
    public string? ItemId;
    public int Count;
    public bool IsDropped = false;
    public int Weight = 1;
}