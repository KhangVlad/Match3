using UnityEngine;

namespace Match3
{
   public class Wall02 : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.Wall_02;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
            tile.ChangeBlock(BlockID.Wall_01);
        }

        public override void Unlock(Tile tile)
        {
        }
    }
}
