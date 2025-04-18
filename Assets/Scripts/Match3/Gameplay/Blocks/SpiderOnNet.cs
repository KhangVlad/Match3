using UnityEngine;

namespace Match3
{
    public class SpiderOnNet : Block
    {
        public override void Initialize()
        {
            BlockID = BlockID.SpiderOnNet;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
            tile.ChangeBlock(BlockID.Spider);
        }

        public override void Unlock(Tile tile)
        {
            tile.ChangeBlock(BlockID.Spider);
        }
    }
}
