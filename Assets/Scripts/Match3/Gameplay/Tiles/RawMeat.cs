using Match3.Shares;
using Match3.Enums;

namespace Match3
{
    public class RawMeat : Tile
    {
        public override void Initialize()
        {
            base.Initialize();
            this.ID = TileID.Raw_Meat;
        }

        protected override void Awake()
        {
            base.Awake();
        }
    }
}