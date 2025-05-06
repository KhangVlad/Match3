using UnityEngine;
using RoboRyanTron.SearchableEnum;
using UnityEngine.Pool;

namespace Match3
{
    public abstract class Block : MonoBehaviour, IBlock
    {
        protected ObjectPool<Block> pool;

        public BlockID BlockID { get; protected set; }
        public bool IsUnlock { get; set; } = false;
        public abstract void Match(Tile tile, Tile[] grid, int width);
        public abstract void Unlock(Tile tile);
        public abstract void Initialize();

        protected virtual void Awake()
        {
            Initialize();
        }

        #region  Pool
        public void SetPool(ObjectPool<Block> pool)
        {
            this.pool = pool;
        }
        public virtual void ReturnToPool()
        {
            transform.SetParent(BlockPoolManager.Instance.transform);
            ResetBlock();
            pool?.Release(this);
        }

        protected virtual void ResetBlock()
        {
            IsUnlock = false;
        }
        #endregion
    }

    public static class BlockExtensions
    {
        public static bool CanNormalMatch(this Block block)
        {
            return block is NoneBlock ||
                    block is Lock;
        }

        public static bool CanFillDownThrough(this Block block)
        {
            return block is NoneBlock || block is VoidBlock;
        }
    }
}
