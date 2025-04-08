using UnityEngine;

namespace Match3
{
    [System.Serializable]
    public class LevelDataV1
    {
        public int MaxTurn;
        public int[,] Blocks;
        public TileID[,] Tiles;
        public TileID[] AvaiableTiles;
        public int[,] Quests;
        public int[] Unlock;        // index 0: character id
                                    // index 1: hearts required to unlock

        public LevelDataV1(int width, int height)
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


    [System.Serializable]
    public class LevelDataV2
    {
        public int MaxTurn;
        public int[,] Blocks;
        public TileID[,] Tiles;
        public TileID[] AvaiableTiles;
        public int[,] Quests;
        public int Heart;
        public int Energy;

        public LevelDataV2(int width, int height)
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
