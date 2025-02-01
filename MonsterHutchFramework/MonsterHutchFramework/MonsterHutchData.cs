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
    public Dictionary<string, ProduceData> ProduceData = new Dictionary<string, ProduceData>();
    public List<string> DeluxeProduce = [];
}
public class ProduceData
{
    public string? Id;
    public string? ItemId;
    public int? Count;
    public bool Dropped = false;
}