using UnityEngine;

namespace Match3
{
    public static class TileExtension
    {
        public const float TILE_WIDTH = 1f;
        public const float TILE_HEIGHT = 1f;
        public const float SPACING_X = 0.1f;
        public const float SPACING_Y = 0.1f;


        public static Vector2 GetWorldPosition(this Tile tile, int offsetX = 0, int offsetY = 0)
        {
            return new Vector2((tile.X + offsetX) * (TILE_WIDTH + SPACING_X), (tile.Y + offsetY) * (TILE_HEIGHT + SPACING_Y));
        }

        public static Vector2 Center(this Tile tile)
        {
            return (Vector2)tile.transform.position + new Vector2(TILE_WIDTH/ 2f, TILE_HEIGHT / 2f);
        }

         public static Vector2 TileCenter()
        {
            return  new Vector2(TILE_WIDTH/ 2f, TILE_HEIGHT / 2f);
        }
    }
}
