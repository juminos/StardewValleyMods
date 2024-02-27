using Microsoft.Xna.Framework;
using StardewValley;

namespace BetterFruitTrees
{
    public class ToolHelper
    {
        // Calculate the tile position in front of the player based on the direction
        public static Vector2 GetTargetTile(Vector2 playerPosition, int direction)
        {
            Vector2 targetTile = playerPosition;

            switch (direction)
            {
                case Game1.down:
                    targetTile.Y += 1;
                    break;
                case Game1.up:
                    targetTile.Y -= 1;
                    break;
                case Game1.left:
                    targetTile.X -= 1;
                    break;
                case Game1.right:
                    targetTile.X += 1;
                    break;
            }

            return targetTile;
        }
    }
}
