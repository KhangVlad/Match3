using UnityEngine;

namespace Match3
{
    public class Spider : Block
    {
        public override void Match(Tile tile, Tile[] grid, int width)
        {
            tile.ChangeBlock(BlockID.None);
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
