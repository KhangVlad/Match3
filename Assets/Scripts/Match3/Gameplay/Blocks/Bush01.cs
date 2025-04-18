using UnityEngine;
namespace Match3
{
    public class Bush01 : Block
    {
        public int ExistTurnCount { get; set; } = 0;

        public override void Initialize()
        {
            BlockID = BlockID.Bush_01;
        }

        protected override void Awake()
        {
            base.Awake();
            ExistTurnCount = 0;
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
