using UnityEngine;
namespace Match3
{
    public class Lock : Block
    {
        public static event System.Action<Lock> OnLockMatch;

         public override void Initialize()
        {
            BlockID = BlockID.Lock;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
            int index = tile.X + tile.Y * width;
            grid[index] = null;
            tile.ReturnToPool();

            OnLockMatch?.Invoke(this);
        }

        public override void Unlock(Tile tile)
        {
         
        }
    }
}
