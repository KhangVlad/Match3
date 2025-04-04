using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace Match3.LevelEditor
{
    public class LevelPreviewManager : MonoBehaviour
    {
        public static LevelPreviewManager Instance { get; private set; }

     
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }


        public Texture2D GetLevelTexture(LevelDataV1 levelData)
        {
            int width = levelData.Tiles.GetLength(0);
            int height = levelData.Tiles.GetLength(1);
            int textureSize = Mathf.Max(width, height);

            if (width == 0 || height == 0)
            {
                return null;
            }

            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            int offsetX = (textureSize - width) / 2;
            int offsetY = (textureSize - height) / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = GetTileColor(levelData.Tiles[x, y]);
      
                    // Apply the offset to center the grid in the square texture
                    texture.SetPixel(x + offsetX, y + offsetY, pixel);
                }
            }

            texture.Apply();
            return texture;
        }


        Color GetTileColor(TileID tile)
        {
            switch (tile)
            {
                case TileID.RedFlower: return new Color(1f, 0f, 0f); // Pure Red
                case TileID.YellowFlower: return new Color(1f, 1f, 0f); // Bright Yellow
                case TileID.PurpleFlower: return new Color(0.6f, 0f, 1f); // Vivid Purple
                case TileID.BlueFlower: return new Color(0f, 0.3f, 1f); // Deep Blue
                case TileID.WhiteFlower: return Color.white; // Pure White

                case TileID.RedCandle: return new Color(0.8f, 0.1f, 0.1f); // Deep Red
                case TileID.YellowCandle: return new Color(1f, 0.8f, 0.1f); // Golden Yellow
                case TileID.GreenCandle: return new Color(0f, 1f, 0f); // Bright Green
                case TileID.BlueCandle: return new Color(0f, 0.7f, 1f); // Strong Cyan-Blue
                case TileID.WhiteCandle: return new Color(0.9f, 0.9f, 0.9f); // Light Gray

                case TileID.RedRibbon: return new Color(1f, 0.1f, 0.3f); // Bright Red-Pink
                case TileID.YellowRibbon: return new Color(1f, 0.7f, 0f); // Orange-Yellow
                case TileID.GreenRibbon: return new Color(0f, 0.8f, 0.2f); // Vivid Green
                case TileID.BlueRibbon: return new Color(0.2f, 0.2f, 1f); // Strong Blue
                case TileID.PurpleRibbon: return new Color(0.7f, 0.1f, 0.8f); // Vibrant Purple

                case TileID.MagnifyingGlass: return new Color(0.3f, 0.3f, 0.3f); // Dark Gray (Glass Effect)


                case TileID.None:
                default:
                    return Color.clear; // Transparent for empty tiles
            }
        }
    }
}
