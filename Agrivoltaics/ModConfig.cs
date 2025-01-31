using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agrivoltaics
{
    public class ModConfig
    {
        public int SolarPanelCost { get; set; } = 100000;
        public int RefinedQuartzAmount { get; set; } = 40;
        public int IronBarAmount { get; set; } = 20;
        public int GoldBarAmount { get; set; } = 20;
        public bool ConditionalBuild { get; set; } = true;
        public string BuildCondition { get; set; } = "PLAYER_HAS_CRAFTING_RECIPE Current Solar Panel";
        public int SpringBatteryCount { get; set; } = 1;
        public int SummerBatteryCount { get; set; } = 1;
        public int FallBatteryCount { get; set; } = 1;
        public int WinterBatteryCount { get; set; } = 1;
        public int SpringBatteryCounter { get; set; } = 2;
        public int SummerBatteryCounter { get; set; } = 1;
        public int FallBatteryCounter { get; set; } = 2;
        public int WinterBatteryCounter { get; set; } = 3;
        public bool GrowGrass { get; set; } = true;
        public float GrowRate { get; set; } = 0.15f;
        public bool SpreadGrass { get; set; } = true;
        public float SpreadRate { get; set; } = 0.65f;
        public float RetentionIncrease { get; set; } = 1f;
        public bool SeasonReset { get; set; } = true;
    }
}
