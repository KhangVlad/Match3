using UnityEngine;
using System.Collections.Generic;

namespace Match3
{
    public static class Match3Algorithm
    {
        // 4-way flood fill using recursion (may cause stack overflow on large areas)
        public static void FloodFillRecursive(Tile[] tiles, int x, int y, int w, int h, ref List<Tile> validNeighbors)
        {
            int index = x + y * w;
            // Check boundaries
            if (x < 0 || x >= w || y < 0 || y >= h)
                return;

            // check valid tile
            if (tiles[index] == null) return;
            if (tiles[index].CurrentBlock is not NoneBlock) return;

            TileID targetTile = tiles[index].ID;
            if (tiles[index].ID != targetTile)
                return;

            validNeighbors.Add(tiles[index]);

            // Recursively fill neighboring pixels
            FloodFillRecursive(tiles, x + 1, y, w,h, ref validNeighbors);
            FloodFillRecursive(tiles, x - 1, y,w,h, ref validNeighbors);
            FloodFillRecursive(tiles, x, y + 1, w,h, ref validNeighbors);
            FloodFillRecursive(tiles, x, y - 1, w,h, ref validNeighbors);
        }
    }
}
