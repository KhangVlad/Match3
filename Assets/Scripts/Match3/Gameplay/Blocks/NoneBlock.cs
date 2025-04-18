using UnityEngine;
using DG.Tweening;
namespace Match3
{
    public class NoneBlock : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.None;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
            int index = tile.X + tile.Y * width;
            grid[index] = null;
            tile.ReturnToPool();
        }
        public override void Unlock(Tile tile)
        {
        }
    }
}
