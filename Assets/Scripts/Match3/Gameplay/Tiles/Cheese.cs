using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class Cheese : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Cheese;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}