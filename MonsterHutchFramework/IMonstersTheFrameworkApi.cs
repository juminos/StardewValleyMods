using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley.Monsters;

namespace MonsterHutchFramework;

public interface IMonstersTheFrameworkApi
{
    Monster GetCustomMonster(string key);
}
