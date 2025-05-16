using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class Egg : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Egg;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}