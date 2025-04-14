using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Match3
{
    [RequireComponent(typeof(Tilemap))]
    public class TilemapSaver : MonoBehaviour
    {
        private Tilemap _tileMap;
        [SerializeField] private string _fileName;

        private void Awake()
        {
            _tileMap = GetComponent<Tilemap>();
        }

        [ContextMenu("Save tilemap")]
        public void SaveTilemap()
        {
            if (_tileMap == null)
            {
                _tileMap = GetComponent<Tilemap>();
            }

            TilemapSaveData saveData = new TilemapSaveData();
            BoundsInt bounds = _tileMap.cellBounds;
            foreach (var pos in bounds.allPositionsWithin)
            {
                TileBase tile = _tileMap.GetTile(pos);
                if (tile != null)
                {
                    saveData.Tiles.Add(new TileData()
                    {
                        Position = pos,
                        TileName = tile.name    // make sure each tile has unique name
                    });
                }
            }

            string json = JsonUtility.ToJson(saveData, true);
            System.IO.File.WriteAllText(Application.streamingAssetsPath + $"/{_fileName}.json", json);
            Debug.Log("Tilemap saved to: " + Application.streamingAssetsPath + $"/{_fileName}.json");
        }



        [ContextMenu("Load tilemap")]
        private void LoadTilemap()
        {
            TextAsset levelText = Resources.Load<TextAsset>($"TilemapLevels/{_fileName}");
            if (levelText == null)
            {
                Debug.Log("File not found! " + _fileName);
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



        [System.Serializable]
        public class TileData
        {
            public Vector3Int Position;
            public string TileName;
        }

        public class TilemapSaveData
        {
            public List<TileData> Tiles = new();
        }
    }
}
