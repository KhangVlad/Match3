using UnityEngine;

namespace Match3
{
    [System.Serializable]
    public class LevelData
    {
        public int MaxTurn;
        public int[,] Blocks;
        public TileID[,] Tiles;
        public TileID[] AvaiableTiles;
        public int[,] Quests;
        public int[] Unlock;        // index 0: character id
                                    // index 1: unlocked by character id
                                    // index 2: hearts required to unlock

        public LevelData(int width, int height)
        {
            Blocks = new int[width, height];
            Tiles = new TileID[width, height];
        }

        public void ApplyRotateMatrix()
        {
            //return;
            int width = Blocks.GetLength(0);  // Rows (original)
            int height = Blocks.GetLength(1); // Columns (original)
            int[,] rotated = new int[height, width]; // New rotated array
            TileID[,] rotatedTiles = new TileID[height, width]; 

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    rotated[y, width - 1 - x] = Blocks[x, y];
                    rotatedTiles[y, width - 1 - x] = Tiles[x, y];
                }
            }
            Blocks = rotated;
            Tiles = rotatedTiles;
        }
    }
}
