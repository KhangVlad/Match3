using System.Collections.Generic;
using Match3.Enums;
using UnityEngine;
using UnityEngine.Pool;

namespace Match3
{
    public class BlockPoolManager : MonoBehaviour
    {
        public static BlockPoolManager Instance { get; private set; }

        private Dictionary<BlockID, ObjectPool<Block>> _poolMap;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            LoadBlockPool();
        }

        private void LoadBlockPool()
        {
            _poolMap = new();

            for (int i = 0; i < GameDataManager.Instance.Blocks.Length; i++)
            {
                ObjectPool<Block> pool = null;
                int cachedIndex = i;
                int defaultCapacity = GameDataManager.Instance.Blocks[i].BlockID == BlockID.None ? 30 : 10;
                pool = new ObjectPool<Block>(
                    createFunc: () =>
                    {
                        Block blockPrefab = GameDataManager.Instance.Blocks[cachedIndex];
                        Block blockInstance = Instantiate(blockPrefab, this.transform);
                        blockInstance.SetPool(pool);
                        return blockInstance;
                    },
                    actionOnGet: block => block.gameObject.SetActive(true),
                    actionOnRelease: block => block.gameObject.SetActive(false),
                    actionOnDestroy: block => Destroy(block.gameObject),
                    collectionCheck: false,
                    defaultCapacity: defaultCapacity
                );

                _poolMap.Add(GameDataManager.Instance.Blocks[i].BlockID, pool);
            }
        }

        public Block GetBlock(BlockID blockID)
        {
            if (_poolMap.TryGetValue(blockID, out var pool))
            {
                return pool.Get();
            }
            Debug.LogWarning($"No block pool registered for {blockID}");
            return null;
        }
    }
}
