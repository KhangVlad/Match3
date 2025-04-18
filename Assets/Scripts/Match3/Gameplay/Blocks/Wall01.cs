using UnityEngine;

namespace Match3
{
    public class Wall01 : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.Wall_01;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
            tile.ChangeBlock(BlockID.None);
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
