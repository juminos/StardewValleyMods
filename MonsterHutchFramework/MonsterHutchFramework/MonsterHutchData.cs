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
    public List<string> Drops = [];
    public int SpeedOverride = 1;
    public bool FarmerCollision = true;
    public int MoveTowardPlayerThresholdOverride = 2;
    public string? TexturePath = null;
    public float DeluxeChance;
    public List<ProduceData> ProduceData = new List<ProduceData>();
    public List<DeluxeProduceData> DeluxeProduce = new List<DeluxeProduceData>();
    public string? InputItemId = null;
    public int InputItemCount = 0;
    public int IncubationTime = 0;
}
public class ProduceData
{
    public string? Id;
    public string? ItemId;
    public int? Count;
    public bool IsDropped = false;
    public int Weight = 0;
}
public class DeluxeProduceData
{
    public string? Id;
    public string? ItemId;
    public int? Count;
    public bool IsDropped = false;
    public int Weight = 0;
}