using System.Collections.Generic;
using Match3.Shares;
using Match3.Enums;
using UnityEngine;
using UnityEngine.Pool;
using System.Linq;


namespace Match3
{
    public class TilePoolManager : MonoBehaviour
    {
        public static TilePoolManager Instance { get; private set; }

        private Dictionary<TileID, ObjectPool<Tile>> _poolMap;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            LoadTilePool();
        }

        private void LoadTilePool()
        {
            Debug.Log("Load Tile Pool");
            _poolMap = new();

            for (int i = 0; i < GameDataManager.Instance.Tiles.Length; i++)
            {
                ObjectPool<Tile> pool = null;
                int cachedIndex = i;
                pool = new ObjectPool<Tile>(
                    createFunc: () =>
                    {
                        Tile tilePrefab = GameDataManager.Instance.Tiles[cachedIndex];
                        Tile tileInstance = Instantiate(tilePrefab, this.transform);
                        tileInstance.SetPool(pool);
                        return tileInstance;
                    },
                    actionOnGet: tile => tile.gameObject.SetActive(true),
                    actionOnRelease: tile => tile.gameObject.SetActive(false),
                    actionOnDestroy: tile => Destroy(tile.gameObject),
                    collectionCheck: false,
                    defaultCapacity: 20
                );

                _poolMap.Add(GameDataManager.Instance.Tiles[i].ID, pool);
            }
        }

        public Tile GetTile(TileID tileID)
        {
            if (_poolMap.TryGetValue(tileID, out var pool))
            {
                return pool.Get();
            }
            Debug.LogWarning($"No tile pool registered for {tileID}");
            return null;
        }
    }
}
