using UnityEngine;
using System.Collections.Generic;
using System.Linq;
namespace Match3
{
    public static class Match3Algorithm
    {
        public static void FloodFillBFS(Tile[] tiles, int startX, int startY, int w, int h, TileID targetTile, ref List<Tile> validNeighbors)
        {
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<int> visited = new HashSet<int>(); // Use index to track visited tiles
            validNeighbors.Clear();
            queue.Enqueue(new Vector2Int(startX, startY));
            validNeighbors.Add(tiles[startX + startY * w]);
            while (queue.Count > 0)
            {
                Vector2Int pos = queue.Dequeue();
                int x = pos.x;
                int y = pos.y;

                // Boundary checks
                if (x < 0 || x >= w || y < 0 || y >= h)
                {
                    continue;
                }

                int index = x + y * w;

                if (visited.Contains(index))
                    continue;
                visited.Add(index);

                if (tiles[index] == null)
                {
                    continue;
                }
                if (tiles[index].CurrentBlock is not NoneBlock)
                {
                    continue;
                }
                if (tiles[index].ID != targetTile)
                {
                    continue;
                }


                validNeighbors.Add(tiles[index]);

                // Enqueue neighbors
                queue.Enqueue(new Vector2Int(x + 1, y));
                queue.Enqueue(new Vector2Int(x - 1, y));
                queue.Enqueue(new Vector2Int(x, y + 1));
                queue.Enqueue(new Vector2Int(x, y - 1));
            }
        }

        public static Tile GetMedianTile(List<Tile> tiles)
        {
            if (tiles == null)
            {
                throw new System.ArgumentException("tiles list is null");
            }

            if (tiles.Count == 0)
            {
                throw new System.ArgumentException("Point list is empty");
            }
            //if (tiles == null || tiles.Count == 0)
            //{
            //    throw new System.ArgumentException("Point list is empty");
            //}

            var sortedX = tiles.Select(p => p.X).OrderBy(x => x).ToList();
            var sortedY = tiles.Select(p => p.Y).OrderBy(y => y).ToList();

            int mid = tiles.Count / 2;

            int medianX = sortedX.Count % 2 == 0
                ? (sortedX[mid - 1] + sortedX[mid]) / 2
                : sortedX[mid];

            int medianY = sortedY.Count % 2 == 0
                ? (sortedY[mid - 1] + sortedY[mid]) / 2
                : sortedY[mid];

            // return new Vector2Int(medianX, medianY);
            return tiles
                .OrderBy(t => Mathf.Abs(t.X - medianX) + Mathf.Abs(t.Y - medianY)) // Manhattan distance
                .First();
        }
    }
}
