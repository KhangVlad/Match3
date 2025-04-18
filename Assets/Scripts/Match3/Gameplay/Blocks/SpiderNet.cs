using UnityEngine;

namespace Match3
{
    public class SpiderNet : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.SpiderNet;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
        }

        public override void Unlock(Tile tile)
        {
            tile.ChangeBlock(BlockID.None);
        }
    }
}
