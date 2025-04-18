using UnityEngine;

namespace Match3
{
    public class Wall03 : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.Wall_03;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
            tile.ChangeBlock(BlockID.Wall_02);
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
