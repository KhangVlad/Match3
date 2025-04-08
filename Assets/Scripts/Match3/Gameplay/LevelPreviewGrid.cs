using UnityEngine;
using UnityEngine.Tilemaps;

namespace Match3
{
    public class LevelPreviewGrid : MonoBehaviour
    {
        public static LevelPreviewGrid Instance { get; private set; }

        [SerializeField] private Tilemap _tileMap;
        [SerializeField] private TileBase _tileBase;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            LoadPreviewGrid();
        }

        private void LoadPreviewGrid()
        {
            LevelDataV2 levelData = LevelManager.Instance.LevelData;
            int width = levelData.Blocks.GetLength(0);
            int height = levelData.Blocks.GetLength(1);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    BlockID blockID = (BlockID)System.Enum.ToObject(typeof(BlockID), levelData.Blocks[x, y]);
                    TileID tileID = levelData.Tiles[x, y];

                    if(blockID != BlockID.Void && blockID != BlockID.Fill)
                    {
                        _tileMap.SetTile(new Vector3Int(x, y, 0), _tileBase);
                    }
                    //else
                    //{
                    //    Debug.Log("???");
                    //}
                }
            }
        }
    }
}
