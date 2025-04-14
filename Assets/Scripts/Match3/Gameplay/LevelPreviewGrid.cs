using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
using Match3.Enums;

namespace Match3
{
    public class LevelPreviewGrid : MonoBehaviour
    {
        public static LevelPreviewGrid Instance { get; private set; }

        [SerializeField] private Tilemap _tileMap;
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
            Debug.Log($"{LevelManager.Instance.CharacterLevelData.CharacterID}   {LevelManager.Instance.CurrentLevelIndex}");
            LoadTilemap(LevelManager.Instance.CharacterLevelData.CharacterID, LevelManager.Instance.CurrentLevelIndex);
        }


        private void LoadTilemap(CharacterID charcterID, int levelIndex)
        {
            string fileName = $"id_{(int)charcterID}_{levelIndex + 1}";
            TextAsset levelText = Resources.Load<TextAsset>($"TilemapLevels/{fileName}");
            if (levelText == null)
            {
                Debug.Log("File not found! " + fileName);
                return;
            }

            string json = levelText.text;
            TilemapSaver.TilemapSaveData saveData = JsonUtility.FromJson<TilemapSaver.TilemapSaveData>(json);
            _tileMap.ClearAllTiles();

            for (int i = 0; i < saveData.Tiles.Count; i++)
            {
                TilemapSaver.TileData tileData = saveData.Tiles[i];
                if (GameDataManager.Instance.TryGetTilebaseByName(tileData.TileName, out TileBase tilebase))
                {
                    _tileMap.SetTile(tileData.Position, tilebase);
                }
                else
                {
                    Debug.LogWarning("Tile not found !!!!!!" + tileData.TileName);
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
