using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class Corn : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Corn;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}