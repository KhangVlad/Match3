using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

namespace Match3
{
    public class LevelPreviewGrid : MonoBehaviour
    {
        public static LevelPreviewGrid Instance { get; private set; }

        [SerializeField] private Tilemap _tileMap;
        [SerializeField] private Tilemap _tileMapFrame;
        [SerializeField] private TileBase _tileBase;
        [SerializeField] private TileBase _tileBaseFrame;

        [SerializeField] private GameplayGridSlot _gridSlotPrefab;
        [SerializeField] private GameplayGridSlot[] _gridSlots;
        public Dictionary<Vector2Int, List<Vector2Int>> MatchTileDictionary;


        private LevelDataV2 _levelData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            MatchTileDictionary = new();
        }

        private void Start()
        {
            LoadPreviewGrid();
        }

        private void LoadPreviewGrid()
        {
            _levelData = LevelManager.Instance.LevelData;
            int width = _levelData.Blocks.GetLength(0);
            int height = _levelData.Blocks.GetLength(1);
            _gridSlots = new GameplayGridSlot[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    BlockID blockID = (BlockID)System.Enum.ToObject(typeof(BlockID), _levelData.Blocks[x, y]);
                    TileID tileID = _levelData.Tiles[x, y];

                    GameplayGridSlot newSlot = Instantiate(_gridSlotPrefab, new Vector3(x, y, 0), Quaternion.identity);
                    newSlot.SetDefaultColor();
                    _gridSlots[x + y * width] = newSlot;

                    if (blockID != BlockID.Void && blockID != BlockID.Fill)
                    {
                        //  _tileMap.SetTile(new Vector3Int(x, y, 0), _tileBase);
                        _tileMapFrame.SetTile(new Vector3Int(x, y, 0), _tileBaseFrame);
                    }
                    else
                    {
                        newSlot.SpriteRenderer.enabled = false;
                    }
                }
            }
        }


        public void Add(Vector2Int key, Vector2Int value)
        {
            if (MatchTileDictionary.ContainsKey(key) == false)
            {
                MatchTileDictionary.Add(key, new());
                MatchTileDictionary[key].Add(value);
            }
            else
            {
                MatchTileDictionary[key].Add(value);
            }
        }

        public void PlayGridCollectAnimation()
        {
            StartCoroutine(PlaygridCollectCoroutine());
        }

        private IEnumerator PlaygridCollectCoroutine()
        {
            foreach (var e in MatchTileDictionary)
            {
                for (int i = 0; i < e.Value.Count; i++)
                {
                    int x = e.Value[i].x;
                    int y = e.Value[i].y;
                    int width = _levelData.Blocks.GetLength(0);
                    int height = _levelData.Blocks.GetLength(1);
                    if (x < 0 || x >= width || y < 0 || y >= height)
                    {
                        Debug.LogError("Out of range!!!");
                        break;
                    }
                    int index = x + y * width;
                    _gridSlots[index].PlayMatchEffect(0.5f);

                    // yield return new WaitForSeconds(0.1f);
                }
                yield return null;

            }

            MatchTileDictionary.Clear();
        }
    }
}
