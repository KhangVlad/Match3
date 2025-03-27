using UnityEngine;

namespace Match3
{
    [System.Serializable]
    public class LevelData
    {
        public int MaxTurn;
        public int[,] Data;
        public TileID[,] Tiles;
        public TileID[] AvaiableTiles;
        public int[,] Quests;
        public LevelData(int width, int height)
        {
            Data = new int[width, height];
            Tiles = new TileID[width, height];
        }

        public void ApplyRotateMatrix()
        {
            //return;
            int width = Data.GetLength(0);  // Rows (original)
            int height = Data.GetLength(1); // Columns (original)
            int[,] rotated = new int[height, width]; // New rotated array
            TileID[,] rotatedTiles = new TileID[height, width]; 

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    rotated[y, width - 1 - x] = Data[x, y];
                    rotatedTiles[y, width - 1 - x] = Tiles[x, y];
                }
            }
            Data = rotated;
            Tiles = rotatedTiles;
        }
    }
}
