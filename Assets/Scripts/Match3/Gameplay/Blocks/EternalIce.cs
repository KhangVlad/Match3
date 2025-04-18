using UnityEngine;

namespace Match3
{
    public class EternalIce : Block
    {
         public override void Initialize()
        {
            BlockID = BlockID.EternalIce;
        }

        public override void Match(Tile tile, Tile[] grid, int width)
        {
            
        }

        public override void Unlock(Tile tile)
        {
            tile.ChangeBlock(BlockID.HardIce);
        }
    }
}
