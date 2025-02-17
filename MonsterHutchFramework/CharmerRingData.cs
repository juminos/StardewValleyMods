using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonsterHutchFramework;

public class CharmerRingData
{
    public string? RingId;
    public List<CharmedMonsterData> CharmedMonsters = new List<CharmedMonsterData>();
}
public class CharmedMonsterData
{
    // Id for entry in this list
    public string? Id { get; set; }
    // should match a game monster name or one added through this framework
    public string? MonsterName { get; set; }
    // Sound used for "petting" monster
    public string? Sound { get; set; }
    // Conditional speech bubble
    public string? SpeechCondition { get; set; }
    public List<SpeechBubbleData> SpeechBubbles { get; set; } = new List<SpeechBubbleData>();
}
public class SpeechBubbleData
{
    // Id for entry in this list
    public string? Id { get; set; }
    // text to display in the speech bubble
    public string? Text { get; set; }
    // weight for choosing which speech text to use
    public int Weight { get; set; } = 1;
    // time delay in milliseconds before speech bubble appears after triggered
    public int Pretimer { get; set; } = 0;
    // time in milliseconds the speech bubble is displayed
    public int Duration { get; set; } = 1500;
    // not sure what this does but game code seems to use 0 or 2
    public int Style { get; set; } = 2;
    // text color (-1 = Default, 1 = Blue, 2 = Red, 3 = Purple, 4 = White, 5 = Orange, 6 = Green, 7 = Cyan, 8 = Gray, 9 = JojaBlue, _ = Black)
    public int Color { get; set; }
}
