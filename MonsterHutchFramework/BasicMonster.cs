using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace MonsterHutchFramework;

public class BasicMonster : Monster
{

    public BasicMonster() 
    {
    }

    public BasicMonster(Vector2 position, MonsterHutchData monsterData)
        : base("Basic Monster", position)
    {
        //Sprite = new StardewValley.AnimatedSprite(monsterData.TexturePath, monsterData.StartingIndex, monsterData.SpriteWidth, monsterData.SpriteHeight); // add parameters to MonsterHutchData model
        base.IsWalkingTowardPlayer = true;
        Sprite.UpdateSourceRect();
        base.HideShadow = monsterData.HideShadow;

    }
}
