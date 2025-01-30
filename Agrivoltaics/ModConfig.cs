using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agrivoltaics
{
    public class ModConfig
    {
        public int MinBatteryCount { get; set; } = 1;
        public int MaxBatteryCount { get; set; } = 3;
        public int BatteryRate { get; set; } = 1;
        public bool GrowGrass { get; set; } = true;
        public float GrowRate { get; set; } = 0.15f;
        public bool SpreadGrass { get; set; } = true;
        public float SpreadRate { get; set; } = 0.65f;
        public float RetentionIncrease { get; set; } = 1f;
    }
}
