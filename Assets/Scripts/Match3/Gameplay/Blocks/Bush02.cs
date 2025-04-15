using UnityEngine;

namespace Match3
{
    public class Bush02 : Block
    {
        public int ExistTurnCount { get; set; } = 0;

        private void Awake()
        {
            ExistTurnCount = 0;
        }
        
        public override void Match(Tile tile, Tile[] grid, int width)
        {
        }

        public override void Unlock(Tile tile)
        {
            tile.ChangeBlock(BlockID.Bush_01);
        }
    }
}
